using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyInCSharp {
    public class BooleanLiteral : AST_Helper.Expression {
        public Token Token { get; set; }

        //Token encapsulates the Token type and string literal. Whereas Value here represents parsed value depending on token type. So Value field 
        //of Boolean class would of type int and will hold the actual value
        public bool Value { get; set; }

        public override void expressionNode() {

        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            return this.Value.ToString().ToLower();//Calling ToString on true changes it to "True". The leading char becomes uppercase.
                                        //calling ToLower() is not the right fix but better than not doing anything
        }
    }
}
