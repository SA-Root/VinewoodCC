using System;
using System.IO;
using Newtonsoft.Json;

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
            Root.AOTCheck(null, null, null);
            //函数和变量不可同名
            //函数内不能定义函数
            //无函数头
            //无表达式内大括号
            //无重载
            //函数重定义不检查
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