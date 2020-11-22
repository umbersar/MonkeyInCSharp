using System;
using System.Collections.Generic;
using System.Text;
using static MonkeyInCSharp.AST_Helper;

namespace MonkeyInCSharp {
    //a prefix expression, also called unary expression, is something like: -5 or !foobar
    //similarly infix expression, also called binary expression, is something like 5 + 5 or foobar + foobar
    //Todo: 5 + -5; now in this case, how is the -5 expression resolved??
    public class PrefixExpression : AST_Helper.Expression {
        public Token Token { get; set; }
        public string Operator { get; set; }
        public Expression Right { get; set; }

        public override void expressionNode() {
            throw new NotImplementedException();
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("(");
            stringBuilder.Append(this.Operator);
            stringBuilder.Append(this.Right.ToString());
            stringBuilder.Append(")");

            return stringBuilder.ToString();
        }
    }
}
