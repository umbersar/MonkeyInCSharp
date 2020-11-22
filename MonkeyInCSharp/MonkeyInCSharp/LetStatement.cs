using System;
using System.Collections.Generic;
using System.Text;
using static MonkeyInCSharp.AST_Helper;

namespace MonkeyInCSharp {
    public class LetStatement : Statement {
        public Token Token { get; set; }// the TokenType.LET token
        public Identifier Name { get; set; }
        public Expression Value { get; set; }

        public override void statementNode() {
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(this.TokenLiteral() + " ");
            stringBuilder.Append(this.Name.ToString());
            stringBuilder.Append(" = ");
            if(this.Value != null)//this assumes a LetStatement can be without a value. Does not make sense when "=" has been assumed to be present
                stringBuilder.Append(this.Value.ToString());
            stringBuilder.Append(";");

            return stringBuilder.ToString();
        }
    }
}
