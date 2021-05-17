using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using ParserSharp;

namespace VinewoodCC
{
    public class Semantica
    {
        private ASTNode Root { get; set; }
        private int LoadAST(string path)
        {
            try
            {
                var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                using (var reader = new StreamReader(stream))
                {
                    var jsonAST = reader.ReadToEnd();
                    Root = JsonConvert.DeserializeObject<ASTNode>(jsonAST);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine($"Could not read file: {path}");
            }
            return 0;
        }
        private void SemanticCheck()
        {
            Root.AOTCheck(null, null);
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