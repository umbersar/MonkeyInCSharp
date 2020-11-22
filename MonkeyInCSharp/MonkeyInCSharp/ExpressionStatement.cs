using System;
using System.Collections.Generic;
using System.Text;
using static MonkeyInCSharp.AST_Helper;

namespace MonkeyInCSharp {
    //let x= 12; this is a let statement
    //x+10; since monkey allows expressions like these, we define a expressionstatement type. 
    //i think a expression computes a value but if we set that value to some var, then that becomes a statement. But monkey is exception
    //because even when we did not set (x+10) value to a var, it is still considered a valid statement.
    //todo:if we disallowed statements like these, then is guess we would have a type which derives from Expression abstract class
    //instead of Statement abstract class.
    public class ExpressionStatement : Statement {
        //first token of the expression. So if the expression is x+5, it will store x. if the expression is 5+5, it will store 5.
        //if the expression statement consists of just one literal, x, then Token will store x. But what will be stored in expression in that case??
        //Or does it mean that Expression always stores the complete Expression but Token just stores first Token.
        public Token Token { get; set; }
        public Expression Expression { get; set; }

        public override void statementNode() {
        }

        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            if (this.Expression != null) {
                return this.Expression.ToString();
            } else
                return "";
        }
    }
}
