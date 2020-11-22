using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MonkeyInCSharp {
    public class Token {
        public TokenHelper.TokenType Type { get; private set; }
        public string Literal { get; private set; }

        public Token(TokenHelper.TokenType type, string literal) { Type = type; Literal = literal; }

        public override string ToString() {
            return "TokenType=" + Type + " : Literal:" + Literal;
        }

    }
}
