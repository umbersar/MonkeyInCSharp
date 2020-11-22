using System;
using System.Collections.Generic;
using System.Text;
//using static MonkeyInCSharp.AST_Helper;

namespace MonkeyInCSharp {

    public static class Parser_Helper {
        //instead of definning my own delegates here, i could have used readymade Action and Func delegate types. Func delegate is useful in this case as 
        //it has a retruntype
        //Func<AST_Helper.Expression>; which would be equivalent to prefixParserFn
        //Func<AST_Helper.Expression,AST_Helper.Expression>; which would be equivalent to infixParseFn
        //but when we make custom delegate types, we give them names and that makes the intention clearer
        //some info on delegates:https://www.pluralsight.com/guides/how-why-to-use-delegates-csharp

        public delegate AST_Helper.Expression prefixParserFn();
        public delegate AST_Helper.Expression infixParseFn(AST_Helper.Expression expression);

        //define operator predence. The operators here are not using the same names as equivalent operators in TokenType enum
        //for e.g., "==" is called EQ in TokenType enum and here it is called EQUALS. These just define precedence levels
        //and these levels are then associated with TokenTypes from TokenType enum in the dictionary below.
        public enum precedence {//todo:should have been named precedenceLevels(from lowest to highest)
            LOWEST,

            EQUALS, // == !=
            LESSGREATER, // > or <
            SUM, // + -
            PRODUCT, // * /
            PREFIX, // -X or !X
            CALL, // myFunction(X)
            INDEX // array[index]
        }

        //todo:should have been named operatorPrecedenceLevels
        public static Dictionary<TokenHelper.TokenType, precedence> precedences = new Dictionary<TokenHelper.TokenType, precedence> {
                                                        {TokenHelper.TokenType.EQ, precedence.EQUALS},
                                                        {TokenHelper.TokenType.NOT_EQ, precedence.EQUALS},

                                                        {TokenHelper.TokenType.LT, precedence.LESSGREATER},//< and > are at same precedence level
                                                        {TokenHelper.TokenType.GT, precedence.LESSGREATER},

                                                        {TokenHelper.TokenType.PLUS, precedence.SUM},//tells us that + and - are at same precedence level
                                                        {TokenHelper.TokenType.MINUS, precedence.SUM},

                                                        {TokenHelper.TokenType.ASTERISK, precedence.PRODUCT},
                                                        {TokenHelper.TokenType.SLASH, precedence.PRODUCT},

                                                        //2nd highest precedence. provides correct 'stickiness' to LPAREN to parse the functions parameters so 
                                                        //that they do not become infix operators when parseExpression is called
                                                        //todo: how CALL precedence for LPAREN not needed for correctly parsing the FunctionExpression arguments but is needed for
                                                        //FunctionCallEpxression parameters??
                                                        {TokenHelper.TokenType.LPAREN, precedence.CALL},
                                                        
                                                        //the highest precedence is awarded to array index operator
                                                        {TokenHelper.TokenType.LBRACKET, precedence.INDEX}

                                                };
    }

    public class Parser {
        Lexer l;
        Token curToken;
        Token peekToken;

        List<string> errors;

        //todo:prefixParseFns and infixParseFns are used in in parsing expression statements (one of the 3 statement types). But not sure about 
        //their usage. It seems, prefixParseFns does not have anything to do with prefix operators(??) or prefixExpression(the expressions which use
        //prefix operators like -5; or 10 + -5; or !foobar;). It seems that prefixParseFns contains prefix operators, IDENTIFIERS(like foobar) 
        //and LITERALs(like 5) with which an Expression(or an ExpressionStatement) can start and thus it includes prefix operators plus other 'stuff'.
        //prefixParseFns are being used to get the parsing correct and not to be confused with prefix operators or prefixExpression.
        Dictionary<TokenHelper.TokenType, Parser_Helper.prefixParserFn> prefixParseFns;
        Dictionary<TokenHelper.TokenType, Parser_Helper.infixParseFn> infixParseFns;

        public Parser(Lexer lexer) {
            this.l = lexer;
            errors = new List<string>();

            //the first expression in the expression statement is taken to be the prefix expression. And if there are more expressions to follow
            //this prefix expression is then joined to them as the leftExpr with rightExpr being the following expressions
            //
            this.prefixParseFns = new Dictionary<TokenHelper.TokenType, Parser_Helper.prefixParserFn>();
            this.registerPrefix(TokenHelper.TokenType.IDENT, this.parseIdentifier);//foobar
            this.registerPrefix(TokenHelper.TokenType.INT, this.parseIntegerLiteral);//5
            this.registerPrefix(TokenHelper.TokenType.BANG, this.parsePrefixExpression);//for expression like !5 or !foobar
            this.registerPrefix(TokenHelper.TokenType.MINUS, this.parsePrefixExpression);//for expression like -5 or -foobar
            this.registerPrefix(TokenHelper.TokenType.TRUE, this.parseBooleanLiteral);//for expression like True 
            this.registerPrefix(TokenHelper.TokenType.FALSE, this.parseBooleanLiteral);//for expression like False
            this.registerPrefix(TokenHelper.TokenType.LPAREN, this.parseGroupedExpression);//for expressions which specify explicit precedence rules using parenthesis
            this.registerPrefix(TokenHelper.TokenType.IF, this.parseIfExpression);//for expressions which specify explicit precedence rules using parenthesis
            this.registerPrefix(TokenHelper.TokenType.FUNCTION, this.parseFunctionExpression);//for expressions which specify explicit precedence rules using parenthesis
            this.registerPrefix(TokenHelper.TokenType.STRING, this.parseStringLiteral);//foobar
            this.registerPrefix(TokenHelper.TokenType.LBRACKET, this.parseArrayLiteral);//foobar
            this.registerPrefix(TokenHelper.TokenType.LBRACE, this.parseHashLiteral);

            this.infixParseFns = new Dictionary<TokenHelper.TokenType, Parser_Helper.infixParseFn>();
            this.registerInfix(TokenHelper.TokenType.PLUS, this.parseInfixExpression);
            this.registerInfix(TokenHelper.TokenType.MINUS, this.parseInfixExpression);
            this.registerInfix(TokenHelper.TokenType.SLASH, this.parseInfixExpression);
            this.registerInfix(TokenHelper.TokenType.ASTERISK, this.parseInfixExpression);
            this.registerInfix(TokenHelper.TokenType.EQ, this.parseInfixExpression);
            this.registerInfix(TokenHelper.TokenType.NOT_EQ, this.parseInfixExpression);
            this.registerInfix(TokenHelper.TokenType.LT, this.parseInfixExpression);
            this.registerInfix(TokenHelper.TokenType.GT, this.parseInfixExpression);

            //FunctionCallExpression could start with a identifier (name of the function) or the FunctionEXpression. see comments in FunctionCallExpression 
            //class implementation. TODO: we declare LPAREN as something that is valid in Infix position for a FunctionCallExpression but we do not want  
            //LPAREN to behave like a Infix operator as we have changed it precedence to highest for FunctionCallExpression and thus changes its' stickiness.
            //see precedences dictionary above 
            this.registerInfix(TokenHelper.TokenType.LPAREN, this.parseFunctionCallExpression);

            //for reasons similar to declaring LPAREN as valid in Infix position for a FunctionCallExpression, do same for LBRACKET. 
            this.registerInfix(TokenHelper.TokenType.LBRACKET, this.parseIndexExpression);//treat [ in myArray[0] as the infix operator, myArray as the left operand and 0 as the right operand.

            this.nextToken();
            this.nextToken();
        }

        private void nextToken() {
            this.curToken = this.peekToken;
            this.peekToken = this.l.NextToken();
        }

        public Program ParseProgram() {
            Program p = new Program();

            while (!this.curTokenIs(TokenHelper.TokenType.EOF)) {
                var stmt = this.parseStatement();
                if (stmt != null) {
                    p.Statements.Add(stmt);
                }
                this.nextToken();
            }

            return p;
        }

        //we only have 3 statement types. These statements are in turn composed of expressions.
        //so perhaps no need to handle all those token types in the switch statement.
        private AST_Helper.Statement parseStatement() {
            switch (this.curToken.Type) {
                //case TokenHelper.TokenType.ILLEGAL:
                //    break;
                //case TokenHelper.TokenType.EOF:
                //    break;
                //case TokenHelper.TokenType.IDENT:
                //    break;
                //case TokenHelper.TokenType.INT:
                //    break;
                //case TokenHelper.TokenType.ASSIGN:
                //    break;
                //case TokenHelper.TokenType.PLUS:
                //    break;
                //case TokenHelper.TokenType.MINUS:
                //    break;
                //case TokenHelper.TokenType.BANG:
                //    break;
                //case TokenHelper.TokenType.ASTERISK:
                //    break;
                //case TokenHelper.TokenType.SLASH:
                //    break;
                //case TokenHelper.TokenType.LT:
                //    break;
                //case TokenHelper.TokenType.GT:
                //    break;
                //case TokenHelper.TokenType.EQ:
                //    break;
                //case TokenHelper.TokenType.NOT_EQ:
                //    break;
                //case TokenHelper.TokenType.COMMA:
                //    break;
                //case TokenHelper.TokenType.SEMICOLON:
                //    break;
                //case TokenHelper.TokenType.LPAREN:
                //    break;
                //case TokenHelper.TokenType.RPAREN:
                //    break;
                //case TokenHelper.TokenType.LBRACE:
                //    break;
                //case TokenHelper.TokenType.RBRACE:
                //    break;
                //case TokenHelper.TokenType.FUNCTION:
                //    break;
                case TokenHelper.TokenType.LET:
                    return this.parseLetStatement();
                //case TokenHelper.TokenType.TRUE:
                //    break;
                //case TokenHelper.TokenType.FALSE:
                //    break;
                //case TokenHelper.TokenType.IF:
                //    break;
                //case TokenHelper.TokenType.ELSE:
                //    break;
                case TokenHelper.TokenType.RETURN:
                    return this.parseReturnStatement();
                default:
                    return this.parseExpressionStatement();
            }
        }

        //x+5-20; is an expression statement
        private AST_Helper.Statement parseExpressionStatement() {
            var stmt = new ExpressionStatement() { Token = this.curToken };

            stmt.Expression = this.parseExpression(Parser_Helper.precedence.LOWEST);//todo: why LOWEST precedence. Because we have just parsing the
            //expression statement and not sure about precedence. But we will recursively call this method back using precedence gleaned from the 
            //parsing context. For example, see parsePrefixExpression method below.

            //if the next token is not semicolon, we do not throw an error(makes it easier to run statement like 5+5 from the REPl without 
            //typing the semicolon). But if it is, we advance the cursor. 
            if (this.peekTokenIs(TokenHelper.TokenType.SEMICOLON)) {
                this.nextToken();
            }

            return stmt;
        }

        private void noPrefixParseFnError(TokenHelper.TokenType type) {
            string message = string.Format("no prefix parse function for {0} found", type);
            this.errors.Add(message);
        }

        //this implements pratt parsing(top down recursive parsing)
        //DaBeaz's course implemented this with loop instead of recursion (so although top down, it is not recursive). Look in "Solution Items/stuff" folder
        //for snapshots showing his approach. Not only is it not recursive(he uses loops), but he has also created a func for each precendence level.
        //his approach is more readable than what we have here.
        private AST_Helper.Expression parseExpression(Parser_Helper.precedence precedence) {

            if (!this.prefixParseFns.ContainsKey(this.curToken.Type)) {
                noPrefixParseFnError(this.curToken.Type);
                return null;
            }
            Parser_Helper.prefixParserFn prefixParserFn = this.prefixParseFns[this.curToken.Type];

            AST_Helper.Expression leftExpr = prefixParserFn();

            while (!this.peekTokenIs(TokenHelper.TokenType.SEMICOLON) && precedence < this.peekPrecedence()) {
                if (!this.infixParseFns.ContainsKey(this.peekToken.Type)) {
                    return leftExpr;
                }

                Parser_Helper.infixParseFn infixParseFn = this.infixParseFns[this.peekToken.Type];
                this.nextToken();
                leftExpr = infixParseFn(leftExpr);//construct a new leftExpr expression using current leftExpr and next expr as rightExpr
            }

            return leftExpr;
        }

        private AST_Helper.Expression parseIdentifier() {
            return new Identifier() { Token = this.curToken, Value = this.curToken.Literal };
        }

        private AST_Helper.Expression parseIntegerLiteral() {
            var literal = new IntegerLiteral() { Token = this.curToken };

            if (int.TryParse(this.curToken.Literal, out int result)) {
                literal.Value = result;
            } else {
                string message = string.Format("Could not parse {0} as integer", this.curToken.Literal);
                this.errors.Add(message);

                return null;
            }

            return literal;
        }

        private AST_Helper.Expression parseStringLiteral() {
            return new StringLiteral() { Token = this.curToken, Value = this.curToken.Literal };
        }

        private AST_Helper.Expression parseArrayLiteral() {
            return new ArrayLiteral() { Token = this.curToken, Elements = this.parseExpressionList(TokenHelper.TokenType.RBRACKET) };
        }

        private AST_Helper.Expression parseHashLiteral() {
            HashLiteral hashLiteral = new HashLiteral { Token = this.curToken };

            while (!this.peekTokenIs(TokenHelper.TokenType.RBRACE)) {
                this.nextToken();
                AST_Helper.Expression keyExpr = this.parseExpression(Parser_Helper.precedence.LOWEST);

                if (!this.expectPeek(TokenHelper.TokenType.COLON)) {
                    return null;
                }

                this.nextToken();
                AST_Helper.Expression valueExpr = this.parseExpression(Parser_Helper.precedence.LOWEST);

                hashLiteral.Pairs.Add(keyExpr, valueExpr);

                if (!this.peekTokenIs(TokenHelper.TokenType.RBRACE) && !this.expectPeek(TokenHelper.TokenType.COMMA)) {
                    return null;
                }
            }

            if (!this.expectPeek(TokenHelper.TokenType.RBRACE)) {
                return null;
            }

            return hashLiteral;
        }

        private AST_Helper.Expression parseBooleanLiteral() {
            //return new BooleanLiteral() { Token = this.curToken, Value = this.curTokenIs(TokenHelper.TokenType.TRUE) };//book uses this logic
            var literal = new BooleanLiteral() { Token = this.curToken };

            if (bool.TryParse(this.curToken.Literal, out bool result)) {
                literal.Value = result;
            } else {
                string message = string.Format("Could not parse {0} as bool", this.curToken.Literal);
                this.errors.Add(message);

                return null;
            }

            return literal;
        }

        //note that Grouped expression does not have a corresponding AST expression node. We have associated a 'prefixFunction' with LParen
        //as soon we enouncter that in a expression, we read the expression starting from LParen till RParen in the usual manner. Grouped Expression
        //is pushed down in the AST and sits at a lower level from non-grouped epxressions thus enforcing the precedence hierarchy.
        //For the other way around, when we serialize AST Tree, the ToString() method takes care for adding the parenthisis as it goes traversing the tree.
        private AST_Helper.Expression parseGroupedExpression() {
            this.nextToken();

            AST_Helper.Expression expression = this.parseExpression(Parser_Helper.precedence.LOWEST);

            if (!this.expectPeek(TokenHelper.TokenType.RPAREN)) {
                return null;
            }

            return expression;
        }


        private AST_Helper.Expression parseIndexExpression(AST_Helper.Expression leftExpr) {
            ArrayIndexExpression arrayIndexExpression = new ArrayIndexExpression { Token = this.curToken, Left = leftExpr };

            this.nextToken();
            arrayIndexExpression.Index = this.parseExpression(Parser_Helper.precedence.LOWEST);

            if (!this.expectPeek(TokenHelper.TokenType.RBRACKET)) {
                return null;
            }

            return arrayIndexExpression;
        }

        private AST_Helper.Expression parseFunctionExpression() {//func named as parseFunctionLiteral in the book
            FunctionExpression funcExpression = new FunctionExpression { Token = this.curToken };

            if (!this.expectPeek(TokenHelper.TokenType.LPAREN)) {
                return null;
            }

            funcExpression.Parameters = this.parseFunctionParameters();

            if (!this.expectPeek(TokenHelper.TokenType.LBRACE)) {
                return null;
            }


            funcExpression.Body = this.parseBlockStatement();

            return funcExpression;
        }

        private List<Identifier> parseFunctionParameters() {
            List<Identifier> identifiers = new List<Identifier>();

            if (this.peekTokenIs(TokenHelper.TokenType.RPAREN)) {
                this.nextToken();
                return identifiers;
            }

            this.nextToken();
            Identifier ident = new Identifier { Token = this.curToken, Value = this.curToken.Literal };

            identifiers.Add(ident);

            while (this.peekTokenIs(TokenHelper.TokenType.COMMA)) {
                this.nextToken();
                this.nextToken();

                ident = new Identifier { Token = this.curToken, Value = this.curToken.Literal };
                identifiers.Add(ident);
            }

            if (!this.expectPeek(TokenHelper.TokenType.RPAREN)) {
                return null;
            }

            return identifiers;
        }

        private AST_Helper.Expression parseIfExpression() {

            IfExpression ifExpression = new IfExpression { Token = this.curToken };

            if (!this.expectPeek(TokenHelper.TokenType.LPAREN)) {
                return null;
            }
            this.nextToken();

            //recursice call back to parseExpression. Control arrived here from parseExpression which we are now calling back
            ifExpression.Condition = this.parseExpression(Parser_Helper.precedence.LOWEST);//it is here we pass precedence using the current context

            if (!this.expectPeek(TokenHelper.TokenType.RPAREN)) {
                return null;
            }

            if (!this.expectPeek(TokenHelper.TokenType.LBRACE)) {
                return null;
            }

            ifExpression.Consequence = this.parseBlockStatement();

            if (this.peekTokenIs(TokenHelper.TokenType.ELSE)) {
                this.nextToken();

                if (!this.expectPeek(TokenHelper.TokenType.LBRACE)) {//expectPeek also advances the token if expected Token is found.
                    return null;
                }

                ifExpression.Alternative = this.parseBlockStatement();
            }

            return ifExpression;
        }

        //ParseProgram method implementation is quite similar to this as both BlockStatement class as well as Program class
        //are collection of Statements.
        private BlockStatement parseBlockStatement() {
            BlockStatement blockStmnt = new BlockStatement { Token = this.curToken };
            this.nextToken();

            while (!this.curTokenIs(TokenHelper.TokenType.RBRACE) && !this.curTokenIs(TokenHelper.TokenType.EOF)) {
                AST_Helper.Statement stmnt = this.parseStatement();
                if (stmnt != null) {
                    blockStmnt.Statements.Add(stmnt);
                }

                this.nextToken();
            }

            return blockStmnt;
        }

        //a prefix expression is something like -5; or !foobar;
        private AST_Helper.Expression parsePrefixExpression() {
            PrefixExpression prefixExpression = new PrefixExpression { Token = this.curToken, Operator = this.curToken.Literal };

            this.nextToken();

            //recursice call back to parseExpression. Control arrived here from parseExpression which we are now calling back
            prefixExpression.Right = this.parseExpression(Parser_Helper.precedence.PREFIX);//it is here we pass precedence using the current context

            return prefixExpression;
        }

        private AST_Helper.Expression parseInfixExpression(AST_Helper.Expression left) {
            InfixExpression infixExpression = new InfixExpression { Token = this.curToken, Operator = this.curToken.Literal, Left = left };

            Parser_Helper.precedence precedence = this.curPrecedence();//precedence level of current token
            this.nextToken();

            //recursice call back to parseExpression. Control arrived here from parseExpression which we are now calling back
            infixExpression.Right = this.parseExpression(precedence);//it is here we pass precedence using the current context

            return infixExpression;
        }

        //this is called parseCallExpression in book. 
        private AST_Helper.Expression parseFunctionCallExpression(AST_Helper.Expression function) {
            FunctionCallExpression functionCallExpression = new FunctionCallExpression { Token = this.curToken, Function = function };
            functionCallExpression.Arguments = this.parseExpressionList(TokenHelper.TokenType.RPAREN);

            return functionCallExpression;
        }

        //this is very similar to parseFunctionParameters. Difference being in parseFunctionParameters, we expect the parameters to be 
        //identifiers wherease here when we are calling/invoking that function the arguments passed to those parameters can be Expressions such as
        //(5, foobar, foobar+5)
        private List<AST_Helper.Expression> parseExpressionList(TokenHelper.TokenType endToken) {
            List<AST_Helper.Expression> args = new List<AST_Helper.Expression>();

            if (this.peekTokenIs(endToken)) {
                this.nextToken();
                return args;
            }

            this.nextToken();
            args.Add(this.parseExpression(Parser_Helper.precedence.LOWEST));

            while (this.peekTokenIs(TokenHelper.TokenType.COMMA)) {
                this.nextToken();
                this.nextToken();

                args.Add(this.parseExpression(Parser_Helper.precedence.LOWEST));
            }

            if (!this.expectPeek(endToken)) {
                return null;
            }

            return args;
        }

        private AST_Helper.Statement parseReturnStatement() {
            var stmt = new ReturnStatement() { Token = this.curToken };

            this.nextToken();

            #region MyRegion
            ////TODO: skip expressions until we encounter a semicolon but replace this with correct parsing logic later
            //while (!this.curTokenIs(TokenHelper.TokenType.SEMICOLON)) {
            //    this.nextToken();
            //} 
            #endregion

            stmt.ReturnValue = this.parseExpression(Parser_Helper.precedence.LOWEST);

            if (this.peekTokenIs(TokenHelper.TokenType.SEMICOLON)) {
                this.nextToken();

            }

            return stmt;
        }

        private LetStatement parseLetStatement() {
            var stmt = new LetStatement() { Token = this.curToken };

            if (!this.expectPeek(TokenHelper.TokenType.IDENT)) {
                return null;
            }

            stmt.Name = new Identifier() { Token = this.curToken, Value = this.curToken.Literal };

            if (!this.expectPeek(TokenHelper.TokenType.ASSIGN)) {
                return null;
            }

            #region MyRegion
            ////TODO: skip expressions until we encounter a semicolon but replace this with correct parsing logic later
            //while (!this.curTokenIs(TokenHelper.TokenType.SEMICOLON)) {
            //    this.nextToken();
            //} 
            #endregion

            this.nextToken();

            stmt.Value = this.parseExpression(Parser_Helper.precedence.LOWEST);

            if (this.peekTokenIs(TokenHelper.TokenType.SEMICOLON)) {
                this.nextToken();

            }

            return stmt;
        }

        Parser_Helper.precedence peekPrecedence() {
            if (Parser_Helper.precedences.ContainsKey(this.peekToken.Type))
                return Parser_Helper.precedences[this.peekToken.Type];
            else
                return Parser_Helper.precedence.LOWEST;
        }

        Parser_Helper.precedence curPrecedence() {
            if (Parser_Helper.precedences.ContainsKey(this.curToken.Type))
                return Parser_Helper.precedences[this.curToken.Type];
            else
                return Parser_Helper.precedence.LOWEST;
        }

        private bool expectPeek(TokenHelper.TokenType t) {//if the expected token is present, this also advances the cursor to next token
            if (this.peekTokenIs(t)) {                    //and this should be reflected in the func name.
                this.nextToken();
                return true;
            } else {
                this.peekError(t);
                return false;
            }
        }

        private bool peekTokenIs(TokenHelper.TokenType t) {
            return this.peekToken.Type == t;
        }

        private bool curTokenIs(TokenHelper.TokenType t) {
            return this.curToken.Type == t;
        }

        public List<string> Errors() {
            return this.errors;
        }

        void peekError(TokenHelper.TokenType t) {
            string message = string.Format("expected next token to be {0}, got {1} instead", t, this.peekToken.Type);
            this.errors.Add(message);
        }

        void registerPrefix(TokenHelper.TokenType t, Parser_Helper.prefixParserFn fn) {
            this.prefixParseFns[t] = fn;
        }

        void registerInfix(TokenHelper.TokenType t, Parser_Helper.infixParseFn fn) {
            this.infixParseFns[t] = fn;
        }
    }
}
