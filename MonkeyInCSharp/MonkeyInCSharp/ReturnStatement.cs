using System;
using System.Collections.Generic;
using System.Text;
using static MonkeyInCSharp.AST_Helper;

namespace MonkeyInCSharp {
    public class ReturnStatement : Statement {
        public Token Token { get; set; }//return TokenType.RETURN token
        public Expression ReturnValue { get; set; }

        public override void statementNode() {
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(this.TokenLiteral()+ " ");
            if (this.ReturnValue != null)//handles the case of ReturnStatement without an expression
                stringBuilder.Append(this.ReturnValue.ToString());
            stringBuilder.Append(";");

            return stringBuilder.ToString();
        }
    }
}
