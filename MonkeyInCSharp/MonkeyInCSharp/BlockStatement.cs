using System;
using System.Collections.Generic;
using System.Text;
using static MonkeyInCSharp.AST_Helper;

namespace MonkeyInCSharp {
    //BlockStatement is just a list of statements just like Program class. Difference being that Program is just a node(not a expression or statement)
    //whereas BlockStatement is Statement itself.
    public class BlockStatement : Statement {
        public Token Token { get; set; }// the TokenType.LBRACE token, i.e., {

        List<Statement> _statements;
        public List<Statement> Statements { get => _statements; set => _statements = value; }

        public BlockStatement() {
            this.Statements = new List<Statement>();
        }

        public override void statementNode() {
        }
        public override string TokenLiteral() {
            return this.Token.Literal;
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();
            
            //stringBuilder.Append("{");//this should have been used to enclose the block but most of the epression types like infixepxression
                                        //enclose the expressions in parenthesis. 

            foreach (var stmt in this.Statements) {
                stringBuilder.Append(stmt.ToString());
            }

            //stringBuilder.Append("}");//this should have been used to enclose the block

            return stringBuilder.ToString();
        }
    }
}
