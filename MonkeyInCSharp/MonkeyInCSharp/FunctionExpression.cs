using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MonkeyInCSharp.AST_Helper;

namespace MonkeyInCSharp {

    //Book calls this class "FunctionLiteral" but imo this should be called FunctionExpression. Literals to my understanding
    //are constant values (5 or true or "Canberra"). Examples:
    //fn(x, y) { return x + y; }
    //let myFunction = fn(x, y) { return x + y; }
    //fn() {return fn(x, y) { return x > y; }; }
    //myFunc(x, y, fn(x, y) { return x > y; });
    public class FunctionExpression : Expression {
        public Token Token { get; set; }//TokenType.FUNCTION 'fn' token
        public List<Identifier> Parameters { get; set; }
        public BlockStatement Body;

        public FunctionExpression() {
           this.Parameters = new List<Identifier>();
        }

        public override void expressionNode() {
            throw new NotImplementedException();
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(this.TokenLiteral());
            stringBuilder.Append("(");
            stringBuilder.Append(String.Join(",", this.Parameters));
            stringBuilder.Append(")");
            stringBuilder.Append(this.Body.ToString());

            return stringBuilder.ToString();
        }
    }
}
