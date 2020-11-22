using System;
using System.Collections.Generic;
using System.Text;

using static MonkeyInCSharp.AST_Helper;

namespace MonkeyInCSharp {
    public class Program : Node {
        List<Statement> _statements;

        public List<Statement> Statements { get => _statements; set => _statements = value; }

        public Program() {
            this.Statements = new List<Statement>();
        }

        public override string TokenLiteral() {
            if (this.Statements.Count > 0) {
                return this.Statements[0].TokenLiteral();
            } else {
                return "";
            }
        }

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var stmt in this.Statements) {
                stringBuilder.Append(stmt.ToString());
            }

            return stringBuilder.ToString();
        }
    }
}
