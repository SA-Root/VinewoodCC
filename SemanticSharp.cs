using System;
using System.IO;
using System.Text.Json;
using VinewoodCC.AST;

namespace VinewoodCC
{
    namespace Semantic
    {
        public class Semantica
        {
            public ASTNode Root { get; set; }
            public static int HasError { get; set; }
            private int LoadAST(string path)
            {
                try
                {
                    var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    using var reader = new StreamReader(stream);
                    var jsonAST = reader.ReadToEnd();
                    Root = JsonSerializer.Deserialize<ASTNode>(jsonAST,SourceGenerationContext.Default.ASTNode);
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
                HasError = 0;
                Root.AOTCheck(null, null, null);
                //函数和变量不可同名
                //函数内不能定义函数
                //无函数头
                //无表达式内大括号
                //无重载
                //函数重定义不检查
            }
            public void Run(string arg)
            {
                var start = DateTime.Now;
                Console.WriteLine($"Reading \"{arg}\"...");
                if (LoadAST(arg) != 0) return;
                Console.WriteLine("Checking AST...");
                SemanticCheck();
                var end = DateTime.Now;
                Console.WriteLine($"Done in {(end - start).TotalMilliseconds}ms.");
            }
            public void Run2(string arg)
            {
                if (LoadAST(arg) != 0) return;
                Console.WriteLine("Checking AST...");
                SemanticCheck();
            }
        }
    }

}