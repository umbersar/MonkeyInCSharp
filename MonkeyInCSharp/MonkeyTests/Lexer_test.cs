using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonkeyInCSharp;
namespace MonkeyTests {
    [TestClass]
    public class Lexer_test {
        //create this class to replace anonymous type as i wanted to refactor code extracting methods but anonymous type can't be passed around. That
        //makes sense
        struct Test_Token {
            public int tokenNumber;
            public TokenHelper.TokenType expectedType;
            public string expectedLiteral;
        }

        [TestMethod]
        public void TestNextToken_EOF() {
            string inputString = "";

            var tests = new[] { new { tokenNumber=0, expectedType = TokenHelper.TokenType.EOF, expectedLiteral = '\0'.ToString() }
                                };

            var lex = new Lexer(inputString);
            foreach (var test in tests) {
                var tok = lex.NextToken();
                if (tok.Type != test.expectedType) {
                    throw new System.Exception(string.Format("wrong tokentype. expected={0}, got={1}", test.expectedType, tok.Type));
                }

                if (tok.Literal != test.expectedLiteral) {
                    throw new System.Exception(string.Format("wrong literal. expected={0}, got={1}", test.expectedLiteral, tok.Literal));
                }
            }

        }

        [TestMethod]
        public void TestNextToken() {
            string inputString = "=+(){},;";

            var tests = new[] { new { tokenNumber=0, expectedType = TokenHelper.TokenType.ASSIGN, expectedLiteral = "=" },
                                new { tokenNumber=1, expectedType = TokenHelper.TokenType.PLUS, expectedLiteral = "+" },
                                new { tokenNumber=2, expectedType = TokenHelper.TokenType.LPAREN, expectedLiteral = "(" },
                                new { tokenNumber=3, expectedType = TokenHelper.TokenType.RPAREN, expectedLiteral = ")" },
                                new { tokenNumber=4, expectedType = TokenHelper.TokenType.LBRACE, expectedLiteral = "{" },
                                new { tokenNumber=5, expectedType = TokenHelper.TokenType.RBRACE, expectedLiteral = "}" },
                                new { tokenNumber=6, expectedType = TokenHelper.TokenType.COMMA, expectedLiteral = "," },
                                new { tokenNumber=7, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" }
                                };

            var lex = new Lexer(inputString);
            foreach (var test in tests) {
                var tok = lex.NextToken();
                if (tok.Type != test.expectedType) {
                    throw new System.Exception(string.Format("tokentype wrong. expected={0}, got={1}", test.expectedType, tok.Type));
                }

                if (tok.Literal != test.expectedLiteral) {
                    throw new System.Exception(string.Format("literal wrong. expected={0}, got={1}", test.expectedLiteral, tok.Literal));
                }
            }

        }

        [TestMethod]
        public void TestNextToken_keywords_funcs() {
            string inputString = @"let five = 5;
                                   let ten = 10;
                                   let add = fn(x, y){
                                    x + y;
                                   };
                                   let result = add(five, ten);
                                ";


            var tests = new[] { new Test_Token{tokenNumber=0, expectedType = TokenHelper.TokenType.LET, expectedLiteral = "let" },
                                new Test_Token{tokenNumber=1, expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "five" },
                                new Test_Token{ tokenNumber=2,expectedType = TokenHelper.TokenType.ASSIGN, expectedLiteral = "=" },
                                new Test_Token{ tokenNumber=3, expectedType = TokenHelper.TokenType.INT, expectedLiteral = "5" },
                                new Test_Token{ tokenNumber=4, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },

                                new Test_Token{ tokenNumber=5, expectedType = TokenHelper.TokenType.LET, expectedLiteral = "let" },
                                new Test_Token{ tokenNumber=6, expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "ten" },
                                new Test_Token{ tokenNumber=7, expectedType = TokenHelper.TokenType.ASSIGN, expectedLiteral = "=" },
                                new Test_Token{ tokenNumber=8, expectedType = TokenHelper.TokenType.INT, expectedLiteral = "10" },
                                new Test_Token{ tokenNumber=9, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },

                                new Test_Token{ tokenNumber=10, expectedType = TokenHelper.TokenType.LET, expectedLiteral = "let" },
                                new Test_Token{ tokenNumber=11, expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "add" },
                                new Test_Token{ tokenNumber=12, expectedType = TokenHelper.TokenType.ASSIGN, expectedLiteral = "=" },
                                new Test_Token{ tokenNumber=13, expectedType = TokenHelper.TokenType.FUNCTION, expectedLiteral = "fn" },
                                new Test_Token{ tokenNumber=14, expectedType = TokenHelper.TokenType.LPAREN, expectedLiteral = "(" },
                                new Test_Token{ tokenNumber=15, expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "x" },
                                new Test_Token{ tokenNumber=16, expectedType = TokenHelper.TokenType.COMMA, expectedLiteral = "," },
                                new Test_Token{ tokenNumber=17, expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "y" },
                                new Test_Token{ tokenNumber=18, expectedType = TokenHelper.TokenType.RPAREN, expectedLiteral = ")" },

                                new Test_Token{ tokenNumber=19, expectedType = TokenHelper.TokenType.LBRACE, expectedLiteral = "{" },
                                new Test_Token{ tokenNumber=20, expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "x" },
                                new Test_Token{ tokenNumber=21, expectedType = TokenHelper.TokenType.PLUS, expectedLiteral = "+" },
                                new Test_Token{ tokenNumber=22, expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "y" },
                                new Test_Token{ tokenNumber=23, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },
                                new Test_Token{ tokenNumber=24, expectedType = TokenHelper.TokenType.RBRACE, expectedLiteral = "}" },
                                new Test_Token{ tokenNumber=25, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },

                                new Test_Token{ tokenNumber=26, expectedType = TokenHelper.TokenType.LET, expectedLiteral = "let" },
                                new Test_Token{ tokenNumber=27, expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "result" },
                                new Test_Token{ tokenNumber=28, expectedType = TokenHelper.TokenType.ASSIGN, expectedLiteral = "=" },
                                new Test_Token{ tokenNumber=29, expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "add" },
                                new Test_Token{ tokenNumber=30, expectedType = TokenHelper.TokenType.LPAREN, expectedLiteral = "(" },
                                new Test_Token{ tokenNumber=31, expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "five" },
                                new Test_Token{ tokenNumber=32, expectedType = TokenHelper.TokenType.COMMA, expectedLiteral = "," },
                                new Test_Token{ tokenNumber=33, expectedType = TokenHelper.TokenType.IDENT, expectedLiteral = "ten" },
                                new Test_Token{ tokenNumber=34, expectedType = TokenHelper.TokenType.RPAREN, expectedLiteral = ")" },
                                new Test_Token{ tokenNumber=35, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" }

            };

            TestTokens(inputString, tests);
        }

        //tokenize gibberish statements mixed with valid statements. "!-/*5;" is gibberish but next statement is valid monkey statement
        [TestMethod]
        public void TestNextToken_TokenizeGibberish() {
            string inputString = @"!-/*5;
                                   5<10>5;
                                  ";

            var tests = new[] { new Test_Token{ tokenNumber=0, expectedType = TokenHelper.TokenType.BANG, expectedLiteral = "!" },
                                new Test_Token{ tokenNumber=1, expectedType = TokenHelper.TokenType.MINUS, expectedLiteral = "-" },
                                new Test_Token{ tokenNumber=2, expectedType = TokenHelper.TokenType.SLASH, expectedLiteral = "/" },
                                new Test_Token{ tokenNumber=3, expectedType = TokenHelper.TokenType.ASTERISK, expectedLiteral = "*" },
                                new Test_Token{ tokenNumber=4, expectedType = TokenHelper.TokenType.INT, expectedLiteral = "5" },
                                new Test_Token{ tokenNumber=5, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },

                                new Test_Token{ tokenNumber=6, expectedType = TokenHelper.TokenType.INT, expectedLiteral = "5" },
                                new Test_Token{ tokenNumber=7, expectedType = TokenHelper.TokenType.LT, expectedLiteral = "<" },
                                new Test_Token{ tokenNumber=8, expectedType = TokenHelper.TokenType.INT, expectedLiteral = "10" },
                                new Test_Token{ tokenNumber=9, expectedType = TokenHelper.TokenType.GT, expectedLiteral = ">" },
                                new Test_Token{ tokenNumber=10, expectedType = TokenHelper.TokenType.INT, expectedLiteral = "5" },
                                new Test_Token{ tokenNumber=11, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" }
                                };

            TestTokens(inputString, tests);
        }

        //add if statement and multi character operators 
        [TestMethod]
        public void TestNextToken_If_MutliChar_Operator() {
            string inputString = @" if (5 < 10) {
                                        return true;
                                    } else {
                                        return false;
                                    }

                                    10 == 10;
                                    10 != 9;
                                  ";

            var tests = new[] { new Test_Token{ tokenNumber=0, expectedType = TokenHelper.TokenType.IF, expectedLiteral = "if" },
                                new Test_Token{ tokenNumber=1, expectedType = TokenHelper.TokenType.LPAREN, expectedLiteral = "(" },
                                new Test_Token{ tokenNumber=2, expectedType = TokenHelper.TokenType.INT, expectedLiteral = "5" },
                                new Test_Token{ tokenNumber=3, expectedType = TokenHelper.TokenType.LT, expectedLiteral = "<" },
                                new Test_Token{ tokenNumber=4, expectedType = TokenHelper.TokenType.INT, expectedLiteral = "10" },
                                new Test_Token{ tokenNumber=5, expectedType = TokenHelper.TokenType.RPAREN, expectedLiteral = ")" },
                                new Test_Token{ tokenNumber=6, expectedType = TokenHelper.TokenType.LBRACE, expectedLiteral = "{" },

                                new Test_Token{ tokenNumber=7, expectedType = TokenHelper.TokenType.RETURN, expectedLiteral = "return" },
                                new Test_Token{ tokenNumber=8, expectedType = TokenHelper.TokenType.TRUE, expectedLiteral = "true" },
                                new Test_Token{ tokenNumber=9, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },

                                new Test_Token{ tokenNumber=10, expectedType = TokenHelper.TokenType.RBRACE, expectedLiteral = "}" },
                                new Test_Token{ tokenNumber=11, expectedType = TokenHelper.TokenType.ELSE, expectedLiteral = "else" },
                                new Test_Token{ tokenNumber=12, expectedType = TokenHelper.TokenType.LBRACE, expectedLiteral = "{" },

                                new Test_Token{ tokenNumber=13, expectedType = TokenHelper.TokenType.RETURN, expectedLiteral = "return" },
                                new Test_Token{ tokenNumber=14, expectedType = TokenHelper.TokenType.FALSE, expectedLiteral = "false" },
                                new Test_Token{ tokenNumber=15, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },

                                new Test_Token{ tokenNumber=16, expectedType = TokenHelper.TokenType.RBRACE, expectedLiteral = "}" },


                                new Test_Token{ tokenNumber=17, expectedType = TokenHelper.TokenType.INT, expectedLiteral = "10" },
                                new Test_Token{ tokenNumber=18, expectedType = TokenHelper.TokenType.EQ, expectedLiteral = "==" },
                                new Test_Token{ tokenNumber=19, expectedType = TokenHelper.TokenType.INT, expectedLiteral = "10" },
                                new Test_Token{ tokenNumber=20, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },

                                new Test_Token{ tokenNumber=21, expectedType = TokenHelper.TokenType.INT, expectedLiteral = "10" },
                                new Test_Token{ tokenNumber=22, expectedType = TokenHelper.TokenType.NOT_EQ, expectedLiteral = "!=" },
                                new Test_Token{ tokenNumber=23, expectedType = TokenHelper.TokenType.INT, expectedLiteral = "9" },
                                new Test_Token{ tokenNumber=24, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },
                                };

            TestTokens(inputString, tests);
        }

        [TestMethod]
        public void TestNextToken_Strings() {
            string inputString = @" ""foobar"";
                                    ""foo bar"";
                                  ";

            var tests = new[] { new Test_Token{ tokenNumber=0, expectedType = TokenHelper.TokenType.STRING, expectedLiteral = "foobar" },
                                new Test_Token{ tokenNumber=1, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },
                                new Test_Token{ tokenNumber=2, expectedType = TokenHelper.TokenType.STRING, expectedLiteral = "foo bar" },
                                new Test_Token{ tokenNumber=3, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },
                            };

            TestTokens(inputString, tests);
        }

        [TestMethod]
        public void TestNextToken_Array() {
            string inputString = @"[1, 2];";

            var tests = new[] { new Test_Token{ tokenNumber=0, expectedType = TokenHelper.TokenType.LBRACKET, expectedLiteral = "[" },
                                new Test_Token{ tokenNumber=1, expectedType = TokenHelper.TokenType.INT, expectedLiteral = "1" },
                                new Test_Token{ tokenNumber=2, expectedType = TokenHelper.TokenType.COMMA, expectedLiteral = "," },
                                new Test_Token{ tokenNumber=3, expectedType = TokenHelper.TokenType.INT, expectedLiteral = "2" },
                                new Test_Token{ tokenNumber=4, expectedType = TokenHelper.TokenType.RBRACKET, expectedLiteral = "]" },
                                new Test_Token{ tokenNumber=5, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },
                                new Test_Token{ tokenNumber=6, expectedType = TokenHelper.TokenType.EOF, expectedLiteral = "\0" },
                            };

            TestTokens(inputString, tests);
        }

        [TestMethod]
        public void TestNextToken_HashMapDictionary() {
            string inputString = @"{""foo"": ""bar""};";

            var tests = new[] { new Test_Token{ tokenNumber=0, expectedType = TokenHelper.TokenType.LBRACE, expectedLiteral = "{" },
                                new Test_Token{ tokenNumber=1, expectedType = TokenHelper.TokenType.STRING, expectedLiteral = @"foo" },
                                new Test_Token{ tokenNumber=2, expectedType = TokenHelper.TokenType.COLON, expectedLiteral = ":" },
                                new Test_Token{ tokenNumber=3, expectedType = TokenHelper.TokenType.STRING, expectedLiteral = @"bar" },
                                new Test_Token{ tokenNumber=4, expectedType = TokenHelper.TokenType.RBRACE, expectedLiteral = "}" },
                                new Test_Token{ tokenNumber=5, expectedType = TokenHelper.TokenType.SEMICOLON, expectedLiteral = ";" },
                                new Test_Token{ tokenNumber=6, expectedType = TokenHelper.TokenType.EOF, expectedLiteral = "\0" },
                            };

            TestTokens(inputString, tests);
        }

        private static void TestTokens(string inputString, Test_Token[] tests) {
            var lex = new Lexer(inputString);

            int index = -1;
            foreach (var test in tests) {
                index++;
                var tok = lex.NextToken();
                if (tok.Type != test.expectedType) {
                    throw new AssertFailedException(string.Format("token number {0} - wrong tokentype. expected={1}, got={2}", index, test.expectedType, tok.Type));
                }

                if (tok.Literal != test.expectedLiteral) {
                    throw new AssertFailedException(string.Format("token number {0} - wrong literal. expected={1}, got={2}", index, test.expectedLiteral, tok.Literal));
                }
            }
        }
    }
}

