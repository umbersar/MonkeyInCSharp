using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MonkeyInCSharp.AST_Helper;

namespace MonkeyInCSharp {

    //Basic structure is <expression>[<expression>]. Examples:
    //[2, 3][0]    
    //[1, 2, 3, 4][2];
    //let myArray = [1, 2, 3, 4];
    //myArray[2];
    //myArray[2 + 1];
    //returnsArray()[1];
    public class ArrayIndexExpression : Expression {
        public Token Token { get; set; }//TokenType.LPAREN '[' token. 

        public Expression Left;//this would either be a IDENT (bound to a ArrayLiteral) or a ArrayLiteral or even a function call that results in an ArrayLiteral.
        public Expression Index;//this can also be any expression but it should result in an integer
        public override void expressionNode() {
            throw new NotImplementedException();
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("(");
            stringBuilder.Append(String.Join(",", this.Left));
            stringBuilder.Append("[");
            stringBuilder.Append(String.Join(",", this.Index));
            stringBuilder.Append("]");
            stringBuilder.Append(")");

            return stringBuilder.ToString();
        }
    }
}
