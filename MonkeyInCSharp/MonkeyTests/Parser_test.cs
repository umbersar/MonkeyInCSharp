using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonkeyInCSharp;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MonkeyTests {
    [TestClass]
    public class Parser_test {

        struct Test_Let_Statements {
            public string input;
            public string expectedIdentifier;
            public object expectedValue;
        }

        struct Test_Return_Statements {
            public string input;
            public object expectedReturnValue;
        }

        struct Test_Identifier {
            public string expectedIdentifier;
        }

        struct Test_Prefix_Operators {
            public string input;
            public string Operator;
            public object rightValue;
        }

        struct Test_Infix_Operators {
            public string input;
            public object leftValue;
            public string Operator;
            public object rightValue;
        }

        [TestMethod]
        public void TestLetStatements() {

            var tests = new[]{
                                new Test_Let_Statements { input="let x = 5;",expectedIdentifier = "x", expectedValue=5},
                                new Test_Let_Statements { input="let y = true;",expectedIdentifier = "y" ,expectedValue=true},
                                new Test_Let_Statements { input="let foobar = y;",expectedIdentifier = "foobar",expectedValue= "y"}

                            };

            foreach (var test in tests) {
                Lexer l = new Lexer(test.input);
                Parser p = new Parser(l);

                var program = p.ParseProgram();
                checkParserErrors(p);

                if (program == null) {
                    throw new AssertFailedException("ParseProgram() returned nil");
                }

                if (program.Statements.Count != 1) {
                    throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
                }


                var stmt = program.Statements[0];
                if (!testLetStatement(stmt, test.expectedIdentifier)) { }

                AST_Helper.Expression value = ((LetStatement)stmt).Value;
                if (!testLiteralExpression(value, test.expectedValue)) { }
            }
        }

        [TestMethod]
        public void Test_Malformed_LetStatements() {
            string inputString = @"let x 5;
                                    let = 10;
                                    let 838383;
                                ";

            Lexer l = new Lexer(inputString);
            Parser p = new Parser(l);

            var program = p.ParseProgram();
            try {
                checkParserErrors(p);
            } catch (AssertFailedException ex) {
                Assert.AreEqual(@"parser has 4 errors
parser error: expected next token to be ASSIGN, got INT instead
parser error: expected next token to be IDENT, got ASSIGN instead
parser error: no prefix parse function for ASSIGN found
parser error: expected next token to be IDENT, got INT instead", ex.Message);
            }
        }

        private bool testLetStatement(AST_Helper.Statement s, string name) {
            if (s.TokenLiteral() != "let") {
                throw new AssertFailedException(string.Format("s.TokenLiteral not 'let'. got ={0}", s.TokenLiteral()));
            }

            LetStatement letStmt;
            try {
                letStmt = (LetStatement)s;
            } catch (Exception) {
                throw new AssertFailedException(string.Format("Expected LetStatement. got={0}", s));
            }

            if (letStmt.Name.Value != name) {
                throw new AssertFailedException(string.Format("letStmt.Name.Value not '{0}'. got={1}", name, letStmt.Name.Value));
            }

            if (letStmt.Name.TokenLiteral() != name) {
                throw new AssertFailedException(string.Format("letStmt.Name.TokenLiteral() not '{0}'. got={1}",
                                                                name, letStmt.Name.TokenLiteral()));
            }

            return true;
        }
        private void checkParserErrors(Parser p) {
            var errors = p.Errors();
            if (errors.Count == 0) return;

            var errorMessage = string.Format("parser has {0} errors", errors.Count);
            foreach (var error in errors) {
                errorMessage = errorMessage + Environment.NewLine + string.Format("parser error: {0}", error);
            }

            throw new AssertFailedException(errorMessage);
        }

        [TestMethod]
        public void TestReturnStatements() {
            var tests = new[]{
                                new Test_Return_Statements { input="return 5;", expectedReturnValue=5},
                                new Test_Return_Statements { input="return true;" ,expectedReturnValue=true},
                                new Test_Return_Statements { input="return y;",expectedReturnValue= "y"}

                            };

            foreach (var test in tests) {
                Lexer l = new Lexer(test.input);
                Parser p = new Parser(l);

                var program = p.ParseProgram();
                checkParserErrors(p);

                if (program == null) {
                    throw new AssertFailedException("ParseProgram() returned nil");
                }

                if (program.Statements.Count != 1) {
                    throw new AssertFailedException(string.Format("program.Statements does not contain 3 statements. got={0}", program.Statements.Count));
                }

                var stmt = program.Statements[0];
                if (!testReturnStatement(stmt)) { }

                AST_Helper.Expression returnValue = ((ReturnStatement)stmt).ReturnValue;
                if (!testLiteralExpression(returnValue, test.expectedReturnValue)) { }
            }
        }

        private bool testReturnStatement(AST_Helper.Statement s) {
            ReturnStatement returnStmt;
            try {
                returnStmt = (ReturnStatement)s;
            } catch (Exception) {
                throw new AssertFailedException(string.Format("Expected ReturnStatement. got={0}", s));
            }

            if (returnStmt.TokenLiteral() != "return") {
                throw new AssertFailedException(string.Format("returnStmt.TokenLiteral not 'return'. got ={0}", returnStmt.TokenLiteral()));
            }

            return true;
        }

        [TestMethod]
        public void TestIdentifierExpression() {
            string inputString = "foobar;";

            Lexer l = new Lexer(inputString);
            Parser p = new Parser(l);

            var program = p.ParseProgram();
            checkParserErrors(p);

            if (program.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
            }

            ExpressionStatement expressionStatement;
            try {
                expressionStatement = (ExpressionStatement)program.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
            }

            //Expression in the ExpressionStatement can be a Identifier(foobar) or a IntegerLiteral(5) or a PrefixExpression(prefix operator and an Expression)
            Identifier ident;
            try {
                ident = (Identifier)expressionStatement.Expression;
            } catch (Exception) {

                throw new AssertFailedException(string.Format("The Expression associated with ExpressionStatement is not " +
                                            "a Identifier. got={0}", expressionStatement.Expression));
            }

            if (ident.Value != "foobar") {
                throw new AssertFailedException(string.Format("Identifier value expected {0}. got={1}", "foobar", ident.Value));
            }

            //in this case the TokenLiteral and Value both are strings. But had it been a integer expression, the TokenLiteral would have been a string
            //and the Value field would have been a int holding the actual parsed number.
            if (ident.TokenLiteral() != "foobar") {
                throw new AssertFailedException(string.Format("Identifier TokenLiteral expected {0}. got={1}", "foobar", ident.TokenLiteral()));
            }
        }
        [TestMethod]
        public void TestIntegerLiteralExpression() {
            string inputString = "5;";

            Lexer l = new Lexer(inputString);
            Parser p = new Parser(l);

            var program = p.ParseProgram();
            checkParserErrors(p);

            if (program.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
            }

            ExpressionStatement expressionStatement;
            try {
                expressionStatement = (ExpressionStatement)program.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
            }

            //Expression in the ExpressionStatement can be a Identifier(foobar) or a IntegerLiteral(5) or a PrefixExpression(prefix operator and an Expression)
            IntegerLiteral intLiteral;
            try {
                intLiteral = (IntegerLiteral)expressionStatement.Expression;//todo:why are null casts allowed. Expression set to NULL is casted without error?
            } catch (Exception) {

                throw new AssertFailedException(string.Format("The Expression associated with ExpressionStatement is not " +
                                            "a Identifier. got={0}", expressionStatement.Expression));
            }

            if (intLiteral.Value != 5) {
                throw new AssertFailedException(string.Format("Identifier value expected {0}. got={1}", 5, intLiteral.Value));
            }

            //in this case the TokenLiteral and Value both are strings. But had it been a integer expression, the TokenLiteral would have been a string
            //and the Value field would have been a int holding the actual parsed number.
            if (intLiteral.TokenLiteral() != "5") {
                throw new AssertFailedException(string.Format("Identifier TokenLiteral expected {0}. got={1}", "foobar", intLiteral.TokenLiteral()));
            }
        }
        [TestMethod]
        public void TestParsingPrefixExpressions() {
            var prefixTests = new[] {
                                     new Test_Prefix_Operators() { input= "!5", Operator="!", rightValue=5},
                                     new Test_Prefix_Operators() { input= "-15", Operator="-", rightValue=15},
                                     new Test_Prefix_Operators() { input= "!false", Operator="!", rightValue=false},
                                     new Test_Prefix_Operators() { input= "!true", Operator="!", rightValue=true},
                                };

            foreach (var test in prefixTests) {


                Lexer l = new Lexer(test.input);
                Parser p = new Parser(l);

                var program = p.ParseProgram();
                checkParserErrors(p);

                if (program.Statements.Count != 1) {
                    throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
                }

                ExpressionStatement expressionStatement;
                try {
                    expressionStatement = (ExpressionStatement)program.Statements[0];
                } catch (Exception) {

                    throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
                }

                //Expression in the ExpressionStatement can be a Identifier(foobar) or a IntegerLiteral(5) or a PrefixExpression(prefix operator and an Expression)
                PrefixExpression prefixExpr;
                try {
                    prefixExpr = (PrefixExpression)expressionStatement.Expression;//todo:why are null casts allowed. Expression set to NULL is casted without error?
                } catch (Exception) {

                    throw new AssertFailedException(string.Format("The Expression associated with ExpressionStatement is not " +
                                                "a PrefixExpression. got={0}", expressionStatement.Expression));
                }

                if (prefixExpr.Operator != test.Operator) {
                    throw new AssertFailedException(string.Format("prefixExpr.Operator value expected {0}. got={1}", test.Operator, prefixExpr.Operator));
                }

                //if (!testIntegerLiteral(prefixExpr.Right, test.integerValue)) {
                //    return;//todo:shouldn't i throw an exception??
                //}
                testLiteralExpression(prefixExpr.Right, test.rightValue);
            }
        }

        [TestMethod]
        public void TestParsingPrefixExpressions_with_InfixOperators() {
            var infixTests = new[] {
                                     new { input= "-5+6", PrefixOperator="-",leftValue=5, InfixOperator="+", rightValue=6}
                                };
            /*
            infixExpr = {((-5) + 6)}

            ((PrefixExpression)infixExpr.Left)
            {(-5)}
                Operator: "-"
                Right: {5}
                Token: {TokenType=MINUS : Literal:-} 

            (IntegerLiteral)infixExpr.Right
            {6}
                Token: {TokenType=INT : Literal:6}
                Value: 6   
            */

            foreach (var test in infixTests) {
                Lexer l = new Lexer(test.input);
                Parser p = new Parser(l);

                var program = p.ParseProgram();
                checkParserErrors(p);

                if (program.Statements.Count != 1) {
                    throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
                }

                ExpressionStatement expressionStatement;
                try {
                    expressionStatement = (ExpressionStatement)program.Statements[0];
                } catch (Exception) {

                    throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
                }

                //Expression in the ExpressionStatement can be a Identifier(foobar) or a IntegerLiteral(5) or a PrefixExpression(prefix operator and an Expression)
                InfixExpression infixExpr;
                try {
                    infixExpr = (InfixExpression)expressionStatement.Expression;//todo:why are null casts allowed. Expression set to NULL is casted without error?
                } catch (Exception) {

                    throw new AssertFailedException(string.Format("The Expression associated with ExpressionStatement is not " +
                                                "a InfixExpression. got={0}", expressionStatement.Expression));
                }

                if (((PrefixExpression)infixExpr.Left).Operator != test.PrefixOperator) {
                    throw new AssertFailedException(string.Format("infixExpr.Operator value expected {0}. got={1}", test.PrefixOperator,
                                                        ((PrefixExpression)infixExpr.Left).Operator));
                }

                if (!testIntegerLiteral(((PrefixExpression)infixExpr.Left).Right, test.leftValue)) {
                    return;//todo:shouldn't i throw an exception??
                }

                if (infixExpr.Operator != test.InfixOperator) {
                    throw new AssertFailedException(string.Format("infixExpr.Operator value expected {0}. got={1}", test.InfixOperator, infixExpr.Operator));
                }

                if (!testIntegerLiteral(infixExpr.Right, test.rightValue)) {
                    return;//todo:shouldn't i throw an exception??
                }
            }
        }

        private bool testIntegerLiteral(AST_Helper.Expression expr, int integerValue) {
            IntegerLiteral intLiteral;
            try {
                intLiteral = (IntegerLiteral)expr;
            } catch (Exception) {

                throw new AssertFailedException(string.Format("expression is not a IntegerLiteral. got={0}", expr));
            }

            if (intLiteral.Value != integerValue) {
                throw new AssertFailedException(string.Format("intLiteral.Value not {0}. got={1}", integerValue, intLiteral.Value));
            }

            return true;
        }

        [TestMethod]
        private bool testIdentifier(AST_Helper.Expression expr, string literalValue) {
            Identifier ident;
            try {
                ident = (Identifier)expr;
            } catch (Exception) {

                throw new AssertFailedException(string.Format("expression not a Identifier. got={0}", expr));
            }

            //in the case of Identifier, both TokenLiteral and Value are same. Had it been a IntergerLiteral, the value would have been int 
            //and tokenliteral would have been int in the form of a string
            if (ident.Value != literalValue) {
                throw new AssertFailedException(string.Format("Identifier.Value not {0}. got={1}", literalValue, ident.Value));
            }

            if (ident.TokenLiteral() != literalValue) {
                throw new AssertFailedException(string.Format("Identifier.TokenLiteral() not {0}. got={1}", literalValue, ident.TokenLiteral()));
            }

            return true;
        }

        private bool testBooleanLiteral(AST_Helper.Expression expr, bool boolValue) {
            BooleanLiteral boolLiteral;
            try {
                boolLiteral = (BooleanLiteral)expr;
            } catch (Exception) {

                throw new AssertFailedException(string.Format("expression is not a BooleanLiteral. got={0}", expr));
            }

            if (boolLiteral.Value != boolValue) {
                throw new AssertFailedException(string.Format("boolLiteral.Value not {0}. got={1}", boolValue, boolLiteral.Value));
            }

            //User can use lower or upercase bool literals and we have to parse it correctly so that it is recognized as keywork and not a identifier. 
            //the string literals to TokenTypes mapping is encoded in keywords dictinary in TokenHelper class. 
            //When we use the LookupIdent function, which checks the dictionary for keywords, use case insensitive lookups
            if (boolLiteral.TokenLiteral().ToLower() != boolValue.ToString().ToLower()) {
                throw new AssertFailedException(string.Format("boolLiteral.Value not {0}. got={1}", boolValue, boolLiteral.Value));
            }

            return true;
        }

        [TestMethod]
        private bool testLiteralExpression(AST_Helper.Expression expr, object expected) {
            if (expected is int) {
                return testIntegerLiteral(expr, (int)expected);
            } else if (expected is string) {
                return testIdentifier(expr, (string)expected);
            } else if (expected is bool) {
                return testBooleanLiteral(expr, (bool)expected);
            } else
                throw new AssertFailedException(string.Format("type of expression not handled. got={0}", expr));
        }

        [TestMethod]
        public void TestBooleanExpression() {
            string inputString = "tRue;";

            Lexer l = new Lexer(inputString);
            Parser p = new Parser(l);

            var program = p.ParseProgram();
            checkParserErrors(p);

            if (program.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
            }

            ExpressionStatement expressionStatement;
            try {
                expressionStatement = (ExpressionStatement)program.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
            }

            //Expression in the ExpressionStatement can be a Identifier(foobar) or a IntegerLiteral(5) or a PrefixExpression(prefix operator and an Expression)
            BooleanLiteral boolLiteral;
            try {
                boolLiteral = (BooleanLiteral)expressionStatement.Expression;//todo:why are null casts allowed. Expression set to NULL is casted without error?
            } catch (Exception) {

                throw new AssertFailedException(string.Format("The Expression associated with ExpressionStatement is not " +
                                            "a BooleanLiteral. got={0}", expressionStatement.Expression));
            }

            if (boolLiteral.Value != true) {
                throw new AssertFailedException(string.Format("Boolean value expected {0}. got={1}", true, boolLiteral.Value));
            }

            //in this case the TokenLiteral and Value both are strings. But had it been a integer expression, the TokenLiteral would have been a string
            //and the Value field would have been a int holding the actual parsed number.
            if (boolLiteral.TokenLiteral() != "tRue") {
                throw new AssertFailedException(string.Format("Boolean TokenLiteral expected {0}. got={1}", "true", boolLiteral.TokenLiteral()));
            }
        }

        [TestMethod]
        private bool testInfixExpression(AST_Helper.Expression expr, object left, string op, object right) {
            InfixExpression infixExp;
            try {
                infixExp = (InfixExpression)expr;
            } catch (Exception) {

                throw new AssertFailedException(string.Format("expression is not a InfixEpxression. got={0}", expr));
            }

            if (!(testLiteralExpression(infixExp.Left, left))) {
                return false;//we do not need to return here
            }

            if (infixExp.Operator != op) {
                throw new AssertFailedException(string.Format("infixExpr.Operator value expected {0}. got={1}", op, infixExp.Operator));
            }

            if (!(testLiteralExpression(infixExp.Right, right))) {
                return false;//we do not need to return here
            }

            return true;//as we are using exceptions, we do not need return. modify the return types of funcs and remove returns
        }

        [TestMethod]
        public void TestParsingInfixExpressions() {
            var infixTests = new[] {
                                    //refer TestParsingPrefixExpressions_with_InfixOperators for test case using prefix expr in a infix exprs
                                     //new Test_Infix_Operators() { input= "-5+6", leftValue=-5, Operator="+", rightValue=6},
                                     new Test_Infix_Operators() { input= "5+6", leftValue=5, Operator="+", rightValue=6},
                                     new Test_Infix_Operators() { input= "5-6", leftValue=5, Operator="-", rightValue=6},
                                     new Test_Infix_Operators() { input= "5*6", leftValue=5, Operator="*", rightValue=6},
                                     new Test_Infix_Operators() { input= "5/6", leftValue=5, Operator="/", rightValue=6},
                                     new Test_Infix_Operators() { input= "5>6", leftValue=5, Operator=">", rightValue=6},
                                     new Test_Infix_Operators() { input= "5<6", leftValue=5, Operator="<", rightValue=6},
                                     new Test_Infix_Operators() { input= "5==6", leftValue=5, Operator="==", rightValue=6},
                                     new Test_Infix_Operators() { input= "5!=6", leftValue=5, Operator="!=", rightValue=6},
                                     new Test_Infix_Operators() { input= "true==true", leftValue=true, Operator="==", rightValue=true},
                                     new Test_Infix_Operators() { input= "true!=false", leftValue=true, Operator="!=", rightValue=false},
                                     new Test_Infix_Operators() { input= "false==false", leftValue=false, Operator="==", rightValue=false}
                                };

            foreach (var test in infixTests) {
                Lexer l = new Lexer(test.input);
                Parser p = new Parser(l);

                var program = p.ParseProgram();
                checkParserErrors(p);

                if (program.Statements.Count != 1) {
                    throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
                }

                ExpressionStatement expressionStatement;
                try {
                    expressionStatement = (ExpressionStatement)program.Statements[0];
                } catch (Exception) {

                    throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
                }

                #region MyRegion
                ////Expression in the ExpressionStatement can be a Identifier(foobar) or a IntegerLiteral(5) or a PrefixExpression(prefix operator and an Expression)
                //InfixExpression infixExpr;
                //try {
                //    infixExpr = (InfixExpression)expressionStatement.Expression;//todo:why are null casts allowed. Expression set to NULL is casted without error?
                //} catch (Exception) {

                //    throw new AssertFailedException(string.Format("The Expression associated with ExpressionStatement is not " +
                //                                "a InfixExpression. got={0}", expressionStatement.Expression));
                //}

                //if (!testIntegerLiteral(infixExpr.Left, test.leftValue)) {
                //    return;//todo:shouldn't i throw an exception??
                //}

                //if (infixExpr.Operator != test.Operator) {
                //    throw new AssertFailedException(string.Format("infixExpr.Operator value expected {0}. got={1}", test.Operator, infixExpr.Operator));
                //}

                //if (!testIntegerLiteral(infixExpr.Right, test.rightValue)) {
                //    return;//todo:shouldn't i throw an exception??
                //}
                #endregion

                testInfixExpression(expressionStatement.Expression, test.leftValue, test.Operator, test.rightValue);
            }
        }

        [TestMethod]
        public void TestOperatorPrecedenceParsing() {
            var infixTests = new[] {
                                     new { input= "-a*b", expected="((-a) * b)"},
                                     new { input= "!-a", expected="(!(-a))"},
                                     new { input= "a+b+c", expected="((a + b) + c)"},
                                     new { input= "a+b-c", expected="((a + b) - c)"},
                                     new { input= "a*b*c", expected="((a * b) * c)"},
                                     new { input= "a*b/c", expected="((a * b) / c)"},
                                     new { input= "a+b/c", expected="(a + (b / c))"},
                                     new { input= "a + b * c + d / e - f", expected="(((a + (b * c)) + (d / e)) - f)"},
                                     new { input= "3 + 4; -5 * 5", expected="(3 + 4)((-5) * 5)"},
                                     new { input= "5 > 4 == 3 < 4", expected="((5 > 4) == (3 < 4))"},
                                     new { input= "5 < 4 != 3 > 4", expected="((5 < 4) != (3 > 4))"},
                                     new { input= "3 + 4 * 5 == 3 * 1 + 4 * 5", expected="((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))"},
                                     new { input= "true", expected="true"},
                                     new { input= "false", expected="false"},
                                     new { input= "3>5==false", expected="((3 > 5) == false)"},
                                     new { input= "3<5==true", expected="((3 < 5) == true)"},
                                     new { input= "1 + (2 + 3) + 4", expected="((1 + (2 + 3)) + 4)"},
                                     new { input= "(5 + 5) * 2", expected="((5 + 5) * 2)"},
                                     new { input= "2/(5+5)", expected="(2 / (5 + 5))"},
                                     new { input= "-(5+5)", expected="(-(5 + 5))"},
                                     new { input= "!(true==true)", expected="(!(true == true))"},
                                    
                                     //following works due to correct function CALL precedence for LPAREN. Otherwise LPAREN would have behaved as a 
                                     //infix operator.todo: how CALL precedence for LPAREN not needed for correctly parsing the FunctionExpression 
                                     //arguments but is needed for FunctionCallEpxression parameters??
                                     new { input= "a + add(b*c) + d", expected="((a + add((b * c))) + d)"},
                                
                                     //following is a function call embedded in a function call
                                     new { input= "add(a, b, 1, 2 * 3, 4 + 5, add(6, 7 * 8))", expected="add(a,b,1,(2 * 3),(4 + 5),add(6,(7 * 8)))"},
                                     new { input= "add(a + b + c * d / f + g)", expected="add((((a + b) + ((c * d) / f)) + g))"},

                                     
                                     //index operator predence is higher than both the * in infix expressions and function call expressions
                                     new { input= "a * [1,2,3,4][b*c] * d", expected="((a * ([1,2,3,4][(b * c)])) * d)"},
                                     new { input= "add(a * b[2], b[1], 2 * [1,2][1]) ", expected="add((a * (b[2])),(b[1]),(2 * ([1,2][1])))"},
            };

            foreach (var test in infixTests) {
                Lexer l = new Lexer(test.input);//construct the AST model
                Parser p = new Parser(l);

                var program = p.ParseProgram();
                checkParserErrors(p);

                string serializedModel = program.ToString();//serialize back the AST model to string

                if (serializedModel != test.expected) {
                    throw new AssertFailedException(string.Format("expected {0}. got={1}", test.expected, serializedModel));
                }

            }
        }

        [TestMethod]
        public void TestIfExpression() {
            string input = "if (x < y) { x }";

            Lexer l = new Lexer(input);//construct the AST model
            Parser p = new Parser(l);

            var program = p.ParseProgram();
            checkParserErrors(p);

            if (program.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
            }

            ExpressionStatement expressionStatement;
            try {
                expressionStatement = (ExpressionStatement)program.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
            }

            //Expression in the ExpressionStatement can be a Identifier(foobar) or a IntegerLiteral(5) or a PrefixExpression(prefix operator and an Expression)
            //or a IfExpression. Parser class classifies Let and Return as Statement(s) and everything else as ExpressionStatement
            IfExpression ifExpr;
            try {
                ifExpr = (IfExpression)expressionStatement.Expression;//todo:why are null casts allowed. Expression set to NULL is casted without error?
            } catch (Exception) {

                throw new AssertFailedException(string.Format("The Expression associated with ExpressionStatement is not " +
                                            "a IfExpression. got={0}", expressionStatement.Expression));
            }

            if (!testInfixExpression(ifExpr.Condition, "x", "<", "y")) {
                return;//we do not need return statement as an exception will be thrown but this is a literal copy of book code
            }

            if (ifExpr.Consequence.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("consequence is not 1 statements. got={0}", program.Statements.Count));
            }

            ExpressionStatement consequence;
            try {
                consequence = (ExpressionStatement)ifExpr.Consequence.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("ifExpr.Consequence is not a ExpressionStatement. got={0}", ifExpr.Consequence.Statements[0]));
            }

            if (!testIdentifier(consequence.Expression, "x")) {
                return;//we do not need return statement as an exception will be thrown but this is a literal copy of book code
            }

            if (ifExpr.Alternative != null) {
                throw new AssertFailedException(string.Format("ifExpr.Alternative was not null. got={0}", ifExpr.Alternative));
            }
        }

        [TestMethod]
        public void TestIfElseExpression() {
            string input = "if (x < y) { x } else { y }";

            Lexer l = new Lexer(input);//construct the AST model
            Parser p = new Parser(l);

            var program = p.ParseProgram();
            checkParserErrors(p);

            if (program.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
            }

            ExpressionStatement expressionStatement;
            try {
                expressionStatement = (ExpressionStatement)program.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
            }

            //Expression in the ExpressionStatement can be a Identifier(foobar) or a IntegerLiteral(5) or a PrefixExpression(prefix operator and an Expression)
            //or a IfExpression. Parser class classifies Let and Return as Statement(s) and everything else as ExpressionStatement
            IfExpression ifExpr;
            try {
                ifExpr = (IfExpression)expressionStatement.Expression;//todo:why are null casts allowed. Expression set to NULL is casted without error?
            } catch (Exception) {

                throw new AssertFailedException(string.Format("The Expression associated with ExpressionStatement is not " +
                                            "a IfExpression. got={0}", expressionStatement.Expression));
            }

            if (!testInfixExpression(ifExpr.Condition, "x", "<", "y")) {
                return;//we do not need return statement as an exception will be thrown but this is a literal copy of book code
            }

            if (ifExpr.Consequence.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("ifExpr.consequence is not 1 statements. got={0}", program.Statements.Count));
            }

            ExpressionStatement consequence;
            try {
                consequence = (ExpressionStatement)ifExpr.Consequence.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("ifExpr.Consequence is not a ExpressionStatement. got={0}", ifExpr.Consequence.Statements[0]));
            }

            if (!testIdentifier(consequence.Expression, "x")) {
                return;//we do not need return statement as an exception will be thrown but this is a literal copy of book code
            }

            if (ifExpr.Alternative.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("ifExpr.Alternative does not contain 1 statements. got={0}", ifExpr.Alternative));
            }

            ExpressionStatement alternative;
            try {
                alternative = (ExpressionStatement)ifExpr.Alternative.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("ifExpr.Alternative is not a ExpressionStatement. got={0}", ifExpr.Alternative.Statements[0]));
            }

            if (!testIdentifier(alternative.Expression, "y")) {
                return;//we do not need return statement as an exception will be thrown but this is a literal copy of book code
            }

        }

        [TestMethod]
        public void TestFunctionLiteralParsing() {
            string input = "fn(x, y) { x + y; }";

            Lexer l = new Lexer(input);//construct the AST model
            Parser p = new Parser(l);

            var program = p.ParseProgram();
            checkParserErrors(p);

            if (program.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
            }

            //parser only recognizes 3 things at top level: let statement, return statement and epxressionstatement. I think
            //that a function definition(as well as If statement from above) are expressions and not expressionstatements but that is how the parser works
            //and exects them to ExpressionStatements.
            ExpressionStatement expressionStatement;
            try {
                expressionStatement = (ExpressionStatement)program.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
            }

            FunctionExpression funcExpr;
            try {
                funcExpr = (FunctionExpression)expressionStatement.Expression;
            } catch (Exception) {

                throw new AssertFailedException(string.Format("expressionStatement.Expression is not FunctionExpression. got={0}",
                                                    expressionStatement.Expression));
            }

            if (funcExpr.Parameters.Count != 2) {
                throw new AssertFailedException(string.Format("funcExpr.Parameters does not contain 2 parameters. got={0}", funcExpr.Parameters.Count));
            }

            testLiteralExpression(funcExpr.Parameters[0], "x");
            testLiteralExpression(funcExpr.Parameters[1], "y");

            if (funcExpr.Body.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("funcExpr.Body.Statements does not contain 1 statements. got={0}", funcExpr.Body.Statements.Count));
            }

            ExpressionStatement stmntInBody;
            try {
                stmntInBody = (ExpressionStatement)funcExpr.Body.Statements[0];
            } catch (Exception) {
                throw new AssertFailedException(string.Format("function body statement is not ExpressionStatement. got={0}",
                                                    funcExpr.Body.Statements[0]));
            }

            testInfixExpression(stmntInBody.Expression, "x", "+", "y");

        }

        [TestMethod]
        public void TestFunctionParameterParsing() {
            var infixTests = new[] {
                                     new { input= "fn() {}; ", expectedParams= new List<string>()},
                                     new { input= "fn(x) {}; ", expectedParams= new List<string>{ "x"}},
                                     new { input= "fn(x, y, z) {}; ", expectedParams= new List<string>{ "x", "y", "z"}},
                                };

            foreach (var test in infixTests) {
                Lexer l = new Lexer(test.input);
                Parser p = new Parser(l);

                var program = p.ParseProgram();
                checkParserErrors(p);

                ExpressionStatement expressionStatement;
                try {
                    expressionStatement = (ExpressionStatement)program.Statements[0];
                } catch (Exception) {

                    throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
                }

                FunctionExpression funcExpr;
                try {
                    funcExpr = (FunctionExpression)expressionStatement.Expression;
                } catch (Exception) {

                    throw new AssertFailedException(string.Format("expressionStatement.Expression is not FunctionExpression. got={0}",
                                                        expressionStatement.Expression));
                }

                if (funcExpr.Parameters.Count != test.expectedParams.Count) {
                    throw new AssertFailedException(string.Format("length of parameter list wrong. want ={0} got={1}",
                                                    test.expectedParams.Count,
                                                    funcExpr.Parameters.Count));
                }

                int index = 0;
                foreach (var ident in test.expectedParams) {
                    testLiteralExpression(funcExpr.Parameters[index], ident);
                    index++;
                }
            }
        }

        [TestMethod]
        public void TestFunctionCallExpressionParsing() {
            string input = "add(1, 2 * 3, 4 + 5);";

            Lexer l = new Lexer(input);//construct the AST model
            Parser p = new Parser(l);

            var program = p.ParseProgram();
            checkParserErrors(p);

            if (program.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
            }

            ExpressionStatement expressionStatement;
            try {
                expressionStatement = (ExpressionStatement)program.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
            }

            //todo: how CALL precedence for LPAREN not needed for correctly parsing the FunctionExpression arguments but is needed for
            //FunctionCallEpxression parameters??
            FunctionCallExpression funCallExpr;
            try {
                funCallExpr = (FunctionCallExpression)expressionStatement.Expression;
            } catch (Exception) {

                throw new AssertFailedException(string.Format("expressionStatement.Expression is not FunctionCallExpression. got={0}",
                                                    expressionStatement.Expression));
            }

            if (!testIdentifier(funCallExpr.Function, "add")) {
                return;
            }

            if (funCallExpr.Arguments.Count != 3) {
                throw new AssertFailedException(string.Format("wrong length of arguments. got={0}", funCallExpr.Arguments.Count));
            }

            testLiteralExpression(funCallExpr.Arguments[0], 1);
            testInfixExpression(funCallExpr.Arguments[1], 2, "*", 3);
            testInfixExpression(funCallExpr.Arguments[2], 4, "+", 5);
        }

        public void TestCallExpressionParameterParsing() {
            throw new NotImplementedException("not implemented");
        }

        [TestMethod]
        public void TestStringLiteralExpression() {
            string input = @"""hello world"";";

            Lexer l = new Lexer(input);//construct the AST model
            Parser p = new Parser(l);

            var program = p.ParseProgram();
            checkParserErrors(p);

            if (program.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
            }

            ExpressionStatement expressionStatement;
            try {
                expressionStatement = (ExpressionStatement)program.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
            }

            StringLiteral strLiteral;
            try {
                strLiteral = (StringLiteral)expressionStatement.Expression;
            } catch (Exception) {

                throw new AssertFailedException(string.Format("expressionStatement.Expression is not StringLiteral. got={0}",
                                                    expressionStatement.Expression));
            }

            if (strLiteral.Value != "hello world") {
                throw new AssertFailedException(string.Format("StringLiteral.Value not {0}. got={1}", "hello world", strLiteral.Value));
            }
        }

        [TestMethod]
        public void TestParsingArrayLiterals() {
            string input = "[1, 2 * 2, 3 + 3]";

            Lexer l = new Lexer(input);//construct the AST model
            Parser p = new Parser(l);

            var program = p.ParseProgram();
            checkParserErrors(p);

            if (program.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
            }

            ExpressionStatement expressionStatement;
            try {
                expressionStatement = (ExpressionStatement)program.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
            }

            ArrayLiteral arrLiteral;
            try {
                arrLiteral = (ArrayLiteral)expressionStatement.Expression;
            } catch (Exception) {

                throw new AssertFailedException(string.Format("expressionStatement.Expression is not ArrayLiteral. got={0}",
                                                    expressionStatement.Expression));
            }

            if (arrLiteral.Elements.Count != 3) {
                throw new AssertFailedException(string.Format("arrLiteral.Elements.Count is not 3. got={0}",
                                                    arrLiteral.Elements.Count));
            }

            testIntegerLiteral(arrLiteral.Elements[0], 1);
            testInfixExpression(arrLiteral.Elements[1], 2, "*", 2);
            testInfixExpression(arrLiteral.Elements[2], 3, "+", 3);
        }

        [TestMethod]
        public void TestParsingIndexExpressions() {
            string input = "myArray[1 + 1]";

            Lexer l = new Lexer(input);//construct the AST model
            Parser p = new Parser(l);

            var program = p.ParseProgram();
            checkParserErrors(p);

            if (program.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
            }

            ExpressionStatement expressionStatement;
            try {
                expressionStatement = (ExpressionStatement)program.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
            }

            ArrayIndexExpression arrayIndexExpression;
            try {
                arrayIndexExpression = (ArrayIndexExpression)expressionStatement.Expression;
            } catch (Exception) {

                throw new AssertFailedException(string.Format("expressionStatement.Expression is not ArrayIndexExpression. got={0}",
                                                    expressionStatement.Expression));
            }

            if (!testIdentifier(arrayIndexExpression.Left, "myArray")) {
                return;//we do not need return statement as an exception will be thrown but this is a literal copy of book code
            }

            if (!testInfixExpression(arrayIndexExpression.Index, 1, "+", 1)) {
                return;//we do not need return statement as an exception will be thrown but this is a literal copy of book code
            }
        }

        [TestMethod]
        public void TestParsingHashLiteralsStringKeys() {
            string input = @"{ ""one"": 1, ""two"": 2, ""three"": 3}";

            Lexer l = new Lexer(input);//construct the AST model
            Parser p = new Parser(l);

            var program = p.ParseProgram();
            checkParserErrors(p);

            if (program.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
            }

            ExpressionStatement expressionStatement;
            try {
                expressionStatement = (ExpressionStatement)program.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
            }

            HashLiteral hashLiteralExpression;
            try {
                hashLiteralExpression = (HashLiteral)expressionStatement.Expression;
            } catch (Exception) {

                throw new AssertFailedException(string.Format("expressionStatement.Expression is not HashLiteral. got={0}",
                                                    expressionStatement.Expression));
            }

            if (hashLiteralExpression.Pairs.Count != 3) {
                throw new AssertFailedException(string.Format("HashLiteral.Pairs has wrong length. got={0}", hashLiteralExpression.Pairs.Count));
            }

            Dictionary<string, int> expected = new Dictionary<string, int> {
                                                                                {"one",1},
                                                                                {"two",2},
                                                                                {"three",3},
                                                                            };
            foreach (var item in hashLiteralExpression.Pairs) {
                StringLiteral strKey = item.Key as StringLiteral;
                if (strKey == null) {
                    throw new AssertFailedException(string.Format("key is not StringLiteral. got={0}", item.Key));
                }

                int expectedVal = expected[strKey.Value];
                testIntegerLiteral(item.Value, expectedVal);
            }
        }

        [TestMethod]
        public void TestParsingEmptyHashLiteral() {
            string input = @"{}";

            Lexer l = new Lexer(input);//construct the AST model
            Parser p = new Parser(l);

            var program = p.ParseProgram();
            checkParserErrors(p);

            if (program.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
            }

            ExpressionStatement expressionStatement;
            try {
                expressionStatement = (ExpressionStatement)program.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
            }

            HashLiteral hashLiteralExpression;
            try {
                hashLiteralExpression = (HashLiteral)expressionStatement.Expression;
            } catch (Exception) {

                throw new AssertFailedException(string.Format("expressionStatement.Expression is not HashLiteral. got={0}", expressionStatement.Expression));
            }

            if (hashLiteralExpression.Pairs.Count != 0) {
                throw new AssertFailedException(string.Format("HashLiteral.Pairs has wrong length. got={0}", hashLiteralExpression.Pairs.Count));
            }
        }

        [TestMethod]
        public void TestParsingHashLiteralsBooleanKeys() {
            string input = @"{ true: 1, false: 2}";

            Lexer l = new Lexer(input);//construct the AST model
            Parser p = new Parser(l);

            var program = p.ParseProgram();
            checkParserErrors(p);

            if (program.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
            }

            ExpressionStatement expressionStatement;
            try {
                expressionStatement = (ExpressionStatement)program.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
            }

            HashLiteral hashLiteralExpression;
            try {
                hashLiteralExpression = (HashLiteral)expressionStatement.Expression;
            } catch (Exception) {

                throw new AssertFailedException(string.Format("expressionStatement.Expression is not HashLiteral. got={0}",
                                                    expressionStatement.Expression));
            }

            if (hashLiteralExpression.Pairs.Count != 2) {
                throw new AssertFailedException(string.Format("HashLiteral.Pairs has wrong length. got={0}", hashLiteralExpression.Pairs.Count));
            }

            Dictionary<bool, int> expected = new Dictionary<bool, int> {
                                                                                {true,1},
                                                                                {false,2},
                                                                            };
            foreach (var item in hashLiteralExpression.Pairs) {
                BooleanLiteral boolKey = item.Key as BooleanLiteral;
                if (boolKey == null) {
                    throw new AssertFailedException(string.Format("key is not BooleanLiteral. got={0}", item.Key));
                }

                int expectedVal = expected[boolKey.Value];
                testIntegerLiteral(item.Value, expectedVal);
            }
        }

        [TestMethod]
        public void TestParsingHashLiteralsIntegerKeys() {
            string input = @"{ 1: 1, 2: 2, 3: 3}";

            Lexer l = new Lexer(input);//construct the AST model
            Parser p = new Parser(l);

            var program = p.ParseProgram();
            checkParserErrors(p);

            if (program.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
            }

            ExpressionStatement expressionStatement;
            try {
                expressionStatement = (ExpressionStatement)program.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
            }

            HashLiteral hashLiteralExpression;
            try {
                hashLiteralExpression = (HashLiteral)expressionStatement.Expression;
            } catch (Exception) {

                throw new AssertFailedException(string.Format("expressionStatement.Expression is not HashLiteral. got={0}",
                                                    expressionStatement.Expression));
            }

            if (hashLiteralExpression.Pairs.Count != 3) {
                throw new AssertFailedException(string.Format("HashLiteral.Pairs has wrong length. got={0}", hashLiteralExpression.Pairs.Count));
            }

            Dictionary<int, int> expected = new Dictionary<int, int> {
                                                                                {1,1},
                                                                                {2,2},
                                                                                {3,3},
                                                                            };
            foreach (var item in hashLiteralExpression.Pairs) {
                IntegerLiteral intKey = item.Key as IntegerLiteral;
                if (intKey == null) {
                    throw new AssertFailedException(string.Format("key is not IntegerLiteral. got={0}", item.Key));
                }

                int expectedVal = expected[intKey.Value];
                testIntegerLiteral(item.Value, expectedVal);
            }
        }


        delegate void InfixExprWrapper(AST_Helper.Expression expr);
        [TestMethod]
        public void TestParsingHashLiteralsWithExpressions() {
            string input = @"{ ""one"": 0 + 1, ""two"": 10 - 8, ""three"": 15 / 5}";

            Lexer l = new Lexer(input);//construct the AST model
            Parser p = new Parser(l);

            var program = p.ParseProgram();
            checkParserErrors(p);

            if (program.Statements.Count != 1) {
                throw new AssertFailedException(string.Format("program.Statements does not contain 1 statements. got={0}", program.Statements.Count));
            }

            ExpressionStatement expressionStatement;
            try {
                expressionStatement = (ExpressionStatement)program.Statements[0];
            } catch (Exception) {

                throw new AssertFailedException(string.Format("Expected ExpressionStatement. got={0}", program.Statements[0]));
            }

            HashLiteral hashLiteralExpression;
            try {
                hashLiteralExpression = (HashLiteral)expressionStatement.Expression;
            } catch (Exception) {

                throw new AssertFailedException(string.Format("expressionStatement.Expression is not HashLiteral. got={0}",
                                                    expressionStatement.Expression));
            }

            if (hashLiteralExpression.Pairs.Count != 3) {
                throw new AssertFailedException(string.Format("HashLiteral.Pairs has wrong length. got={0}", hashLiteralExpression.Pairs.Count));
            }


            //Using Action in-built delegate instead of user defined custom delegate
            //Dictionary<string, InfixExprWrapper> tests = new Dictionary<string, InfixExprWrapper> {
            //                                                                        {"one", (AST_Helper.Expression expr)=> testInfixExpression(expr,0,"+",1) },
            //                                                                        {"two", (AST_Helper.Expression expr)=> testInfixExpression(expr,10,"-",8) },
            //                                                                        {"three", (AST_Helper.Expression expr)=> testInfixExpression(expr,15,"/",5) },
            //                                                                    };

            Dictionary<string, Action<AST_Helper.Expression>> tests = new Dictionary<string, Action<AST_Helper.Expression>> {
            { "one", (AST_Helper.Expression expr) => testInfixExpression(expr, 0, "+", 1) },
                                                                                { "two", (AST_Helper.Expression expr) => testInfixExpression(expr, 10, "-", 8) },
                                                                                { "three", (AST_Helper.Expression expr) => testInfixExpression(expr, 15, "/", 5) },
                                                                            };

            foreach (var item in hashLiteralExpression.Pairs) {
                StringLiteral strKey = item.Key as StringLiteral;
                if (strKey == null) {
                    throw new AssertFailedException(string.Format("key is not StringLiteral. got={0}", item.Key));
                }

                //Using Action in-built delegate instead of user defined custom delegate
                //if(tests.TryGetValue(strKey.Value, out InfixExprWrapper testFunc))                
                if (tests.TryGetValue(strKey.Value, out Action<AST_Helper.Expression> testFunc))
                    testFunc(item.Value);
                else
                    throw new AssertFailedException(string.Format("No test function for key {0} found", strKey.Value));
            }
        }
    }
}

