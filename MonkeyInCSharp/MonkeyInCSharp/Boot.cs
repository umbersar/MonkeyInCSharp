using System;

namespace MonkeyInCSharp
{
    class Boot
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello {0}! This is the Monkey programming language!\n", Environment.UserName);
            Console.WriteLine("Feel free to type in commands\n");

            REPL.Start(Console.In, Console.Out);
        }
    }
}
