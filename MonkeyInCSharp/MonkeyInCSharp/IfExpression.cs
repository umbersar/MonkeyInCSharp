using System;
using System.Collections.Generic;
using System.Text;
using static MonkeyInCSharp.AST_Helper;

namespace MonkeyInCSharp {
    //note that it is an expression, not a statement. So it will return a value
    public class IfExpression : Expression {
        public Token Token { get; set; }
        public Expression Condition { get; set; }
        public BlockStatement Consequence { get; set; }
        public BlockStatement Alternative { get; set; }

        public override void expressionNode() {
            throw new NotImplementedException();
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("if");
            stringBuilder.Append(this.Condition.ToString());
            stringBuilder.Append(" ");
            stringBuilder.Append(this.Consequence.ToString());

            if (this.Alternative != null) {
                stringBuilder.Append("else");
                stringBuilder.Append(this.Alternative.ToString());
            }

            return stringBuilder.ToString();
        }
    }
}
