using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MonkeyInCSharp.AST_Helper;

namespace MonkeyInCSharp {

    // Examples:
    //[2, 3]    
    //[2, "test", 3 + 3, fn(x) { x }, add(2, 2)] 
    //Array elements can be of any type as long as they are expressions. It has the form similar to function call arguments (which are expressions as well). The only difference being
    //in this case we are using RBRACKET and LBRACKET tokens for enclosing them.
    public class ArrayLiteral: Expression {
        public Token Token { get; set; }//TokenType.LPAREN '[' token. 

        public List<Expression> Elements;
        public override void expressionNode() {
            throw new NotImplementedException();
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("[");
            stringBuilder.Append(String.Join(",", this.Elements));
            stringBuilder.Append("]");

            return stringBuilder.ToString();
        }
    }
}
