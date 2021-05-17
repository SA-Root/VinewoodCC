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
            var jsonAST = "";
            try
            {
                var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                using (var reader = new StreamReader(stream))
                {
                    jsonAST = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine($"Could not read file: {path}");
            }
            Root = JsonConvert.DeserializeObject<ASTNode>(jsonAST);
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