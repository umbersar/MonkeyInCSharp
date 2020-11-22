using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MonkeyInCSharp.AST_Helper;

namespace MonkeyInCSharp {

    //{<expression> : <expression>, <expression> : <expression>, ... }
    public class HashLiteral : Expression {
        public Token Token { get; set; }//TokenType.FUNCTION '{' token

        public Dictionary<Expression, Expression> Pairs { get; set; }

        public HashLiteral() {
            this.Pairs = new Dictionary<Expression, Expression>();
        }

        public override void expressionNode() {
            throw new NotImplementedException();
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            //string.Join(", ", Pairs). It will output in the form [A, 1], [B, 2], [C, 3], [D, 4] 
            return string.Join(",", Pairs.Select(x => x.Key + ":" + x.Value).ToArray());            
        }
    }
}
