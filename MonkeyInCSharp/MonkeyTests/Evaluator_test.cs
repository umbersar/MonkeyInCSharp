using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonkeyInCSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyTests {
    [TestClass]
    public class Evaluator_test {
        [TestMethod]
        public void TestEvalIntegerExpression() {
            var tests = new[] { new { input="5", expected= new Integer_AST(5) },
                                new { input = "10", expected = new Integer_AST(10) },

                                new { input = "-5", expected = new Integer_AST(-5)},//- prefix operator test
                                new { input = "-10", expected = new Integer_AST(-10)},//- prefix operator test

                                //infix operators
                                new { input = "5 + 5 + 5 + 5 - 10", expected = new Integer_AST(10)},//infix operators
                                new { input = "2 * 2 * 2 * 2 * 2", expected = new Integer_AST(32)},
                                new { input = "-50 + 100 + -50", expected = new Integer_AST(0)},
                                new { input = "5 * 2 + 10", expected = new Integer_AST(20)},
                                new { input = "5 + 2 * 10", expected = new Integer_AST(25)},
                                new { input = "20 + 2 * -10", expected = new Integer_AST(0)},
                                new { input = "50 / 2 * 2 + 10", expected = new Integer_AST(60)},
                                new { input = "2 * (5 + 10)", expected = new Integer_AST(30)},
                                new { input = "3 * 3 * 3 + 10", expected = new Integer_AST(37)},
                                new { input = "3 * (3 * 3) + 10", expected = new Integer_AST(37)},
                                new { input = "(5 + 10 * 2 + 15 / 3) * 2 + -10", expected = new Integer_AST(50)}
                              };

            foreach (var test in tests) {
                var evaluated = testEval(test.input);
                testIntegerObject(evaluated, test.expected);
            }
        }

        private void testIntegerObject(Object_AST resultObj, Object_AST expectedObj) {
            Integer_AST result = resultObj as Integer_AST;
            if (result is null) {
                throw new AssertFailedException(string.Format("resultObj is not Integer_AST. got={0}", resultObj));
            }

            Integer_AST expected = expectedObj as Integer_AST;
            if (expected is null) {
                throw new AssertFailedException(string.Format("expectedObj is not Integer_AST. got={0}", expected));
            }

            Assert.AreEqual(result.Value, expected.Value, string.Format("resultObj has wrong value. got={0}, want={1}", result.Value, expected.Value));
        }

        private void testStringObject(Object_AST resultObj, Object_AST expectedObj) {
            String_AST result = resultObj as String_AST;
            if (result is null) {
                throw new AssertFailedException(string.Format("resultObj is not String_AST. got={0}", resultObj));
            }

            String_AST expected = expectedObj as String_AST;
            if (expected is null) {
                throw new AssertFailedException(string.Format("expectedObj is not String_AST. got={0}", expected));
            }

            Assert.AreEqual(result.Value, expected.Value, string.Format("resultObj has wrong value. got={0}, want={1}", result.Value, expected.Value));
        }


        private Object_AST testEval(string input) {
            Environment_AST env = Environment_AST.NewEnvironment_AST();

            Lexer lex = new Lexer(input);
            Parser p = new Parser(lex);
            Program prog = p.ParseProgram();
            return Evaluator.Eval(prog, env);
        }

        [TestMethod]
        public void TestEvalBooleanExpression() {
            var tests = new[] { new { input="false", expected= Evaluator.false_AST },
                                new { input = "true", expected = Evaluator.true_AST},
            
                                //bool infix expressions with non-bool operands
                                new { input = "1 < 2", expected = Evaluator.true_AST},
                                new { input = "1 > 2", expected = Evaluator.false_AST},
                                new { input = "1 < 1", expected = Evaluator.false_AST},
                                new { input = "1 > 1", expected = Evaluator.false_AST},
                                new { input = "1 == 1", expected = Evaluator.true_AST},
                                new { input = "1 != 1", expected = Evaluator.false_AST},
                                new { input = "1 == 2", expected = Evaluator.false_AST},
                                new { input = "1 != 2", expected = Evaluator.true_AST},

                                //bool infix expressions with bool operands 
                                new { input = "true == true", expected = Evaluator.true_AST},
                                new { input = "false == false", expected = Evaluator.true_AST},
                                new { input = "true == false", expected = Evaluator.false_AST},
                                new { input = "true != false", expected = Evaluator.true_AST},
                                new { input = "false != true", expected = Evaluator.true_AST},

                                new { input = "(1 < 2) == true", expected = Evaluator.true_AST},
                                new { input = "(1 < 2) == false", expected = Evaluator.false_AST},
                                new { input = "(1 > 2) == true", expected = Evaluator.false_AST},
                                new { input = "(1 > 2) == false", expected = Evaluator.true_AST},
                            };

            foreach (var test in tests) {
                var evaluated = testEval(test.input);
                testBooleanObject(evaluated, test.expected);
            }
        }

        private void testBooleanObject(Object_AST resultObj, Object_AST expectedObj) {
            Boolean_AST result = resultObj as Boolean_AST;
            if (result is null) {
                throw new AssertFailedException(string.Format("resultObj is not Boolean. got={0}", resultObj));
            }

            Boolean_AST expected = expectedObj as Boolean_AST;
            if (result is null) {
                throw new AssertFailedException(string.Format("expectedObj is not Boolean. got={0}", resultObj));
            }

            Assert.AreEqual(result.Value, expected.Value, string.Format("resultObj has wrong value. got={0}, want={1}", result.Value, expected.Value));
        }

        [TestMethod]
        public void TestBangOperator() {
            var tests = new[] { new { input="!false", expected=Evaluator.true_AST },
                                new { input = "!true", expected = Evaluator.false_AST},
                                new { input = "!5", expected = Evaluator.false_AST},
                                new { input = "!!true", expected = Evaluator.true_AST},
                                new { input = "!!false", expected = Evaluator.false_AST},
                                new { input = "!!5", expected = Evaluator.true_AST},
                                new { input = "!!0", expected = Evaluator.true_AST},//our language implementation evaluates every number to be true in conditions..
                                                                      //i think C treats only the numbers >0 to be true
                              };

            foreach (var test in tests) {
                var evaluated = testEval(test.input);
                testBooleanObject(evaluated, test.expected);
            }
        }

        struct ConditonalTestData {
            public string input;
            public Object_AST expected;
        }
        [TestMethod]
        public void TestIfElseExpressions() {
            var tests = new ConditonalTestData[] {
                                new ConditonalTestData{ input="if (true) { 10 }", expected=new Integer_AST(10)},
                                new ConditonalTestData{ input="if (false) { 10 }", expected=Evaluator.null_AST},//anonymous types could not handle the varying 'type' for expected field
                                new ConditonalTestData{ input="if (1) { 10 }", expected=new Integer_AST(10)},
                                new ConditonalTestData{ input="if (1<2) { 10 }", expected=new Integer_AST(10)},
                                new ConditonalTestData{ input="if (1>2) { 10 }", expected=Evaluator.null_AST},
                                new ConditonalTestData{ input="if (1<2) { 10 } else {20}", expected=new Integer_AST(10)},
                                new ConditonalTestData{ input="if (1>2) { 10 } else {20}", expected=new Integer_AST(20)},
                              };

            foreach (var test in tests) {
                var evaluated = testEval(test.input);
                if (test.expected is Integer_AST) {
                    testIntegerObject(evaluated, test.expected);
                } else
                    testNullObject(evaluated, test.expected);
            }
        }

        private void testNullObject(Object_AST resultObj, Object_AST expectedObj) {
            Assert.AreEqual(resultObj, expectedObj, string.Format("resultObj is not NULL. got={0}", resultObj));
        }

        [TestMethod]
        public void TestReturnStatements() {
            var tests = new[] {
                                //new { input="return 10;", expected=new ReturnValue_AST(new Integer_AST(10)) },
                                new { input="return 10;", expected=new Integer_AST(10) },//ReturnValue_AST is used internally in the interpretor while walking the AST. the end user would only see the actual result
                                new { input="return 10; 9;", expected=new Integer_AST(10) },
                                new { input="return 2*5; 9;", expected=new Integer_AST(10) },
                                new { input="9; return 2*5; 9;", expected=new Integer_AST(10) },
                                new {input="if (10 > 1) { return 10; }", expected=new Integer_AST(10)},

                
                                //a return in a nested block of statements in the same exeution context(in the same function or at the same level in call stack) should cause a return to a level up in 
                                //the call hierachy(calling function)
                                new { input="if(10>1){ " +
                                "                   if(10>1) " +
                                "                       {return 10;}" + //execution of the if block should return/stop here

                                "                   return 1;" +
                                "                   }", expected=new Integer_AST(10) },

                                new { input=@"let f = fn(x) {
                                                        return x;
                                                        x + 10;
                                                    };
                                                    f(10);"
                                    , expected=new Integer_AST(10) },

                                new { input=@"let f = fn(x) {
                                                               let result = x + 10;
                                                               return result;
                                                               return 10;
                                                            };
                                                            f(10);"
                                    , expected=new Integer_AST(20) },


                            };

            foreach (var test in tests) {
                var evaluated = testEval(test.input);
                testIntegerObject(evaluated, test.expected);
            }
        }

        [TestMethod]
        public void TestErrorHandling() {
            var tests = new[] {
                                new { input="5 + true; 5;", expected=new Error_AST("type mismatch: Integer_AST + Boolean_AST") },
                                new { input="-true; 5;", expected=new Error_AST("unknown operator: -Boolean_AST") },
                                new { input="true + false;", expected=new Error_AST("unknown operator: Boolean_AST + Boolean_AST") },
                                new { input="5; true + false; 5;", expected=new Error_AST("unknown operator: Boolean_AST + Boolean_AST") },
                                new { input="if (10 > 1) { true + false; }", expected=new Error_AST("unknown operator: Boolean_AST + Boolean_AST") },

                                new { input="if(10>1){ " +
                                "                   if(10>1) " +
                                "                       {return true + false;}" + //execution of the if block should at this return

                                "                   return 1;" +
                                "                   }", expected=new Error_AST("unknown operator: Boolean_AST + Boolean_AST") },

                                new { input="5 + -true;", expected=new Error_AST("unknown operator: -Boolean_AST") },

                                new {input="foobar", expected=new Error_AST("identifier not found: foobar")},

                                new {input=@"""Hello"" - "" World!""", expected=new Error_AST("unknown operator: String_AST - String_AST")},

                                new {input=@"{""name"": ""Monkey""}[fn(x) { x }];", expected=new Error_AST("unusable as hash key: Function_AST")},
                              };

            foreach (var test in tests) {
                var evaluated = testEval(test.input);
                testErrorObject(evaluated, test.expected);
            }
        }

        private void testErrorObject(Object_AST resultObj, Error_AST expectedObj) {
            Error_AST result = resultObj as Error_AST;
            if (result is null) {
                Assert.Fail(string.Format("resultObj is not Error_AST. got={0}", resultObj));//Assert.Fail will internally anyways throw an AssertFailedException
            }

            Error_AST expected = expectedObj as Error_AST;
            if (expected is null) {
                Assert.Fail(string.Format("expectedObj is not Error_AST. got={0}", expected));
            }

            Assert.AreEqual(result.Message, expected.Message, string.Format("resultObj has wrong error message. got={0}, want={1}", result.Message, expected.Message));
        }


        //it only assert integer aray elements at the moment..implement functionality in Array_AST to test equality 
        private void testArrayObject(Object_AST resultObj, Array_AST expectedObj) {
            Array_AST result = resultObj as Array_AST;
            if (result is null) {
                Assert.Fail(string.Format("resultObj is not Array_AST. got={0}", resultObj));//Assert.Fail will internally anyways throw an AssertFailedException
            }

            Array_AST expected = expectedObj as Array_AST;
            if (expected is null) {
                Assert.Fail(string.Format("expectedObj is not Array_AST. got={0}", expected));
            }

            Assert.AreEqual(result.Elements.Count, expected.Elements.Count, string.Format("resultObj has wrong number of array elements. got={0}, want={1}", result.Elements.Count, expected.Elements.Count));

            for (int i = 0; i < expected.Elements.Count; i++) {
                Integer_AST exp_int = expected.Elements[i] as Integer_AST;
                Integer_AST res_int = result.Elements[i] as Integer_AST;
                Assert.AreEqual(res_int.Value, exp_int.Value, string.Format("resultObj array elements do not match. got={0}, want={1}", res_int, exp_int));
            }
        }

        [TestMethod]
        public void TestLetStatements() {
            var tests = new[] {
                                new { input="let a = 5; a;", expected=new Integer_AST(5) },
                                new { input="let a = 5*5; a;", expected=new Integer_AST(25) },
                                new { input="let a = 5; let b=a; b", expected=new Integer_AST(5) },
                                new { input="let a = 5; let b=a; let c = a + b + 5; c;", expected=new Integer_AST(15) },
                              };

            foreach (var test in tests) {
                var evaluated = testEval(test.input);
                testIntegerObject(evaluated, test.expected);
            }
        }

        [TestMethod]
        public void TestFunctionObject() {
            string input = "fn(x) { x + 2; };";
            Object_AST evaluated = testEval(input);

            Function_AST fn = evaluated as Function_AST;
            if (fn == null) {
                Assert.Fail(string.Format("object is not Function. got={0}", evaluated));
            }

            if (fn.Parameters.Count != 1) {
                Assert.Fail(string.Format("function has wrong parameters. Parameters={0}", fn.Parameters));
            }

            if (fn.Parameters[0].Value != "x") {//could have used ToString() as well
                Assert.Fail(string.Format("parameter is not 'x'. got={0}", fn.Parameters[0]));
            }

            string expectedBody = "(x + 2)";
            if (fn.Body.ToString() != expectedBody) {
                Assert.Fail(string.Format("body is not {0}. got={1}", expectedBody, fn.Body.ToString()));
            }
        }

        [TestMethod]
        public void TestFunctionApplication() {
            var tests = new[] {
                                new { input= "let identity = fn(x) { x; }; identity(5);", expected=new Integer_AST(5) },//implicit return
                                new { input= "let identity = fn(x) { return x; }; identity(5);", expected=new Integer_AST(5) },//explicit return

                                new { input= "let identity = fn(x) { x; }; let double = fn(x) { x * 2; }; double(5);", expected=new Integer_AST(10) },
                                new { input= "let add = fn(x, y) { x + y; }; add(5, 5);", expected=new Integer_AST(10) },

                                //evaluating arguments before passing them to the function. implementation
                                new { input= "let add = fn(x, y) { x + y; }; add(5 + 5, add(5, 5));", expected=new Integer_AST(20) },

                                new { input= "fn(x) { x; }(5)", expected=new Integer_AST(5) },//call using lambda/anonymous func
                              };

            foreach (var test in tests) {
                var evaluated = testEval(test.input);
                testIntegerObject(evaluated, test.expected);
            }
        }

        [TestMethod]
        public void TestClosures() {
            //Higher-order functions are functions that either return other functions or receive them as arguments. newAdder here is a higher-order function that returns a function(implicit return 
            //as it being the last line). And that returned function is also a closure as it encloses/captures a variable outside its own scope (x is the parameter of newAdder but not the 
            //inner returned function) and we can executed the returned func by just apssing it the y parameter.
            //Note: When a FunctionExpression is evaluated (as is the closure in this case), we build an Function_AST object and keep a reference to the current environment in its.Env field. That
            //Env will hold the variable name x and its value
            string input = @"
                            let newAdder = fn(x) {
                                fn(y) { x + y };
                            };
                            let addTwo = newAdder(2);
                            addTwo(3);";

            var evaluated = testEval(input);
            testIntegerObject(evaluated, new Integer_AST(5));

            //closure which receives a function as an argument
            input = @"
                      let add = fn(a, b) { a + b };
                      let applyFunc = fn(a, b, func) { func(a, b) };
                      applyFunc(2, 6, add);
                    ";

            evaluated = testEval(input);
            testIntegerObject(evaluated, new Integer_AST(8));
        }

        [TestMethod]
        public void TestStackOverFlowException() {
            return;//the reason i am returning here is that the following breaks the testing process and causes rest of the tests to not execute.

            string input = @"let counter = fn(x) {
                                                    if (x > 1000) {
                                                        return true;
                                                    } else {
                                                        let foobar = 9999;
                                                        counter(x + 1);
                                                    }
                                                };
                            counter(0);";
            try {
                var evaluated = testEval(input);
                testBooleanObject(evaluated, new Boolean_AST(true));
            } catch (System.StackOverflowException) {

                Assert.IsTrue(true);
                return;
            } catch (Exception) {

                Assert.IsTrue(true);
                return;
            }

            //let counter = fn(x) {if (x > 1000) {return true;} else {let foobar = 9999;counter(x + 1);}};
            Assert.Fail("Running the same code snippet from REPL window would throw an stackoverflow exception but does not throw any exception here but still fails. This current Assert.Fail " +
                "is also not executed! Also, if we execute this test, the rest of the tests in this test library are not executed!");
        }

        [TestMethod]
        public void TestStringLiteral() {
            string input = @"""Hello World""";

            var evaluated = testEval(input);
            String_AST str = evaluated as String_AST;
            if (str is null) {
                Assert.Fail(string.Format("object is not String. got={0}", evaluated));
            }

            if (str.Value != "Hello World") {
                Assert.Fail(string.Format("String has wrong value. got={0}", str.Value));
            }
        }

        [TestMethod]
        public void TestStringConcatenation() {
            string input = @"""Hello"" + "" World!""";

            var evaluated = testEval(input);
            String_AST str = evaluated as String_AST;
            if (str is null) {
                Assert.Fail(string.Format("object is not String. got={0}", evaluated));
            }

            if (str.Value != "Hello World!") {
                Assert.Fail(string.Format("String has wrong value. got={0}", str.Value));
            }
        }

        struct TestInfo {
            public string input;
            public Object_AST expected;
        }
        [TestMethod]
        public void TestBuiltinFunctions() {
            TestInfo[] tests = new[] {
                                        new TestInfo() { input=@"len("""")", expected=new Integer_AST(0) },
                                        new TestInfo() { input=@"len(""four"")", expected=new Integer_AST(4) },
                                        new TestInfo() { input=@"len(""hello world"")", expected=new Integer_AST(11) },
                                        new TestInfo() { input=@"len(1)", expected=new Error_AST(@"argument to ""len"" not supported, got Integer_AST")},
                                        new TestInfo() { input=@"len(""one"",""two"")", expected=new Error_AST("wrong number of arguments. got=2, want=1")},

                                        new TestInfo() { input=@"len([1, 2, 3])", expected=new Integer_AST(3)},
                                        new TestInfo() { input=@"len([])", expected=new Integer_AST(0)},

                                        //new TestInfo() { input=@"puts(""hello"", ""world!"")", expected=Evaluator.null_AST},

                                        new TestInfo() { input=@"first([1, 2, 3])", expected=new Integer_AST(1)},
                                        new TestInfo() { input=@"first([])", expected=Evaluator.null_AST},
                                        new TestInfo() { input=@"first(1)", expected=new Error_AST(@"argument to ""first"" must be an array, got Integer_AST")},

                                        new TestInfo() { input=@"last([1, 2, 3])", expected=new Integer_AST(3)},
                                        new TestInfo() { input=@"last([])", expected=Evaluator.null_AST},
                                        new TestInfo() { input=@"last(1)", expected=new Error_AST(@"argument to ""last"" must be an array, got Integer_AST")},

                                        new TestInfo() { input=@"rest([1, 2, 3])", expected=new Array_AST(){
                                                                                                            Elements=new List<Object_AST>{new Integer_AST(2), new Integer_AST(3)}
                                                                                                            }},
                                        new TestInfo() { input=@"rest([])", expected=Evaluator.null_AST},

                                        new TestInfo() { input=@"push([],1)", expected=new Array_AST(){
                                                                                                        Elements=new List<Object_AST>{new Integer_AST(1)}
                                                                                                       }},
                                        new TestInfo() { input=@"push(1,1)", expected=new Error_AST(@"argument to ""push"" must be an array, got Integer_AST")},

            };

            foreach (var test in tests) {
                var evaluated = testEval(test.input);
                switch (test.expected) {
                    case Integer_AST i:
                        testIntegerObject(evaluated, i);
                        break;
                    case Array_AST arr:
                        testArrayObject(evaluated, arr);
                        break;
                    case Error_AST err:
                        testErrorObject(evaluated, err);
                        break;
                    default:
                        break;
                }
            }
        }

        [TestMethod]
        public void TestArrayLiterals() {
            string input = "[1, 2 * 2, 3 + 3]";

            var evaluated = testEval(input);
            Array_AST arr = evaluated as Array_AST;
            if (arr is null) {
                Assert.Fail(string.Format("object is not Array. got={0}", evaluated));
            }

            if (arr.Elements.Count != 3) {
                Assert.Fail(string.Format("array has wrong num of elements. got={0}", arr.Elements.Count));
            }

            testIntegerObject(arr.Elements[0], new Integer_AST(1));
            testIntegerObject(arr.Elements[1], new Integer_AST(4));
            testIntegerObject(arr.Elements[2], new Integer_AST(6));
        }

        [TestMethod]
        public void TestArrayIndexExpressions() {
            TestInfo[] tests = new[] {
                                        new TestInfo() { input=@"[1, 2, 3][0]", expected=new Integer_AST(1) },
                                        new TestInfo() { input=@"[1, 2, 3][1]", expected=new Integer_AST(2) },
                                        new TestInfo() { input=@"[1, 2, 3][2]", expected=new Integer_AST(3) },

                                        new TestInfo() { input=@"let i = 0; [1][i];", expected=new Integer_AST(1) },
                                        new TestInfo() { input=@"[1, 2, 3][1+1]", expected=new Integer_AST(3) },
                                        new TestInfo() { input=@"let myArray = [1, 2, 3]; myArray[2];", expected=new Integer_AST(3) },
                                        new TestInfo() { input=@"let myArray = [1, 2, 3]; myArray[0] + myArray[1] + myArray[2];",
                                                        expected=new Integer_AST(6) },
                                        new TestInfo() { input=@"let myArray = [1, 2, 3]; let i = myArray[0]; myArray[i]",
                                                        expected=new Integer_AST(2) },

                                        new TestInfo() { input=@"[1, 2, 3][3]", expected=Evaluator.null_AST },
                                        new TestInfo() { input=@"[1, 2, 3][-1]", expected=Evaluator.null_AST },
                                    };


            foreach (var test in tests) {
                var evaluated = testEval(test.input);
                switch (test.expected) {
                    case Integer_AST i:
                        testIntegerObject(evaluated, test.expected);
                        break;
                    case Null_AST null_:
                        testNullObject(evaluated, test.expected);
                        break;
                    default:
                        break;
                }
            }
        }

        [TestMethod]
        public void TestHashLiterals() {
            string input = @"let two = ""two"";
                            {
                                ""one"": 10 - 9,
                                two: 1 + 1,
                                ""thr"" + ""ee"": 6 / 2,
                                4: 4,
                                true: 5,
                                false: 6
                            }
                            ";

            var evaluated = testEval(input);
            Hash_AST result_hash_AST = evaluated as Hash_AST;
            if (result_hash_AST is null) {
                Assert.Fail(string.Format("object is not hash_AST. got={0}", evaluated));
            }

            Dictionary<HashKey, Object_AST> expected = new Dictionary<HashKey, Object_AST> {
                {new String_AST("one").HashKey(), new Integer_AST(1) },
                {new String_AST("two").HashKey(), new Integer_AST(2) },
                {new String_AST("three").HashKey(), new Integer_AST(3) },
                {new Integer_AST(4).HashKey(), new Integer_AST(4) },
                {Evaluator.true_AST.HashKey(), new Integer_AST(5) },
                {Evaluator.false_AST.HashKey(), new Integer_AST(6) },
            };

            if (result_hash_AST.Pairs.Count != expected.Count) {
                Assert.Fail(string.Format("Hash has wrong num of pairs. got={0}", result_hash_AST.Pairs.Count));
            }

            foreach (var item in expected) {
                if (result_hash_AST.Pairs.TryGetValue(item.Key, out HashPair result_pair))
                    testIntegerObject(result_pair.Value, item.Value);
                else
                    Assert.Fail("no result_pair for found for given expected key");
            }
        }

        [TestMethod]
        public void TestHashIndexExpressions() {
            var tests = new ConditonalTestData[] {
                                new ConditonalTestData{ input=@"{""foo"": 5}[""foo""]", expected=new Integer_AST(5)},
                                new ConditonalTestData{ input=@"{""foo"": 5}[""bar""]", expected=Evaluator.null_AST},//anonymous types could not handle the varying 'type' for expected field
                                
                                new ConditonalTestData{ input=@"let key = ""foo""; {""foo"": 5}[key]", expected=new Integer_AST(5)},//anonymous types could not handle the varying 'type' for expected field
                                new ConditonalTestData{ input=@"{}[""foo""]", expected=Evaluator.null_AST},//anonymous types could not handle the varying 'type' for expected field
                              
                                new ConditonalTestData{ input=@"{5: 5}[5]", expected=new Integer_AST(5)},
                                new ConditonalTestData{ input=@"{true: 5}[true]", expected=new Integer_AST(5)},
                                new ConditonalTestData{ input=@"{false: 5}[false]", expected=new Integer_AST(5)},
            };

            foreach (var test in tests) {
                var evaluated = testEval(test.input);
                switch (test.expected) {
                    case Integer_AST i:
                        testIntegerObject(evaluated, i);
                        break;
                    case Null_AST null_:
                        testNullObject(evaluated, null_);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
