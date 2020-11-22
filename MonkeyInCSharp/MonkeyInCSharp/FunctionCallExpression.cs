using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MonkeyInCSharp.AST_Helper;

namespace MonkeyInCSharp {

    // Examples:
    //add(2, 3)    add is an identifier
    //add(2 + 2, 3 * 3 * 3)     add is an identifier and arguments are expressions
    //fn(x, y) { x + y; }(2, 3) this is a FunctionExpression which is being called in-place
    public class FunctionCallExpression : Expression {//this class is called CallExpression in book
        public Token Token { get; set; }//TokenType.LPAREN '(' token. 

        public Expression Function;//Identifier or FunctionExpression. So the least common denominator is to treat it as an Expression
        public List<Expression> Arguments;
        public override void expressionNode() {
            throw new NotImplementedException();
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(this.Function.ToString());
            stringBuilder.Append("(");
            stringBuilder.Append(String.Join(",", this.Arguments));
            stringBuilder.Append(")");

            return stringBuilder.ToString();
        }
    }
}
