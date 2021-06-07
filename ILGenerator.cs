using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using VinewoodCC.AST;

namespace VinewoodCC
{
    namespace ILGen
    {
        public class ILGenerator
        {
            public static Stack<QuadTuple> LoopBreakStack { get; set; }
            public static Stack<QuadTuple> LoopContinueStack { get; set; }
            public static List<QuadTuple> PostfixCache { get; set; }
            public static int TmpCounter { get; set; }
            private ASTNode Root { get; set; }
            public string OutputFile { get; set; }
            private List<QuadTuple> ILProgram { get; set; }
            private int LoadAST(string path)
            {
                OutputFile = path.Substring(0, path.LastIndexOf(".ast.json")) + ".vcil";
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
            private void GenerateIR(bool OutputQuadTuple)
            {
                ILProgram = new List<QuadTuple>();
                PostfixCache = new List<QuadTuple>();
                LoopBreakStack = new Stack<QuadTuple>();
                LoopContinueStack = new Stack<QuadTuple>();
                TmpCounter = 0;
                Root.ILGenerate(ILProgram, null);
                //Console.WriteLine("---------------IR Code---------------");
                //ILProgram.PrintToConsole();
                ILProgram.OutputQuadTuple(OutputFile);
            }
            public void Run(string arg, bool OutputQuadTuple)
            {
                var start = DateTime.Now;
                Console.WriteLine($"Reading \"{arg}\"...");
                if (LoadAST(arg) != 0) return;
                Console.WriteLine("Generating IR...");
                GenerateIR(OutputQuadTuple);
                var end = DateTime.Now;
                Console.WriteLine($"Done in {(end - start).TotalMilliseconds}ms.");
            }
            public void Run2(string arg, bool OutputQuadTuple)
            {
                OutputFile = arg.Substring(0, arg.LastIndexOf(".ast.json")) + ".vcil";
                Console.WriteLine("Generating IR...");
                GenerateIR(OutputQuadTuple);
            }
            public ILGenerator(ASTNode root)
            {
                Root = root;
            }
        }
    }
}
