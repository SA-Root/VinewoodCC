using System;

namespace VinewoodCC
{
    public class Semantica
    {
        private int LoadAST(string path)
        {
            return 0;
        }
        private void SemanticCheck()
        {

        }
        public void run(string arg)
        {
            var start = DateTime.Now;
            Console.WriteLine($"Reading \"{arg}\"...");
            if (LoadAST(arg) != 0) return;
            Console.WriteLine("Checking AST...");
            SemanticCheck();
            var end = DateTime.Now;
            Console.WriteLine($"Done in {(end - start).TotalMilliseconds}ms.");
        }
    }
}