using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace VinewoodCC
{
    public static class ILProgramExtension
    {
        public static void PrintToConsole(this LinkedList<QuadTuple> ILProgram)
        {
            foreach (var i in ILProgram)
            {
                Console.WriteLine("({0},{1},{2},{3})", i.Operator, i.RValueA.ID, i.RValueB.ID, i.LValue.ID);
            }
        }
    }
    public class ILGenerator
    {
        private ASTNode Root { get; set; }
        public string OutputFile { get; set; }
        private LinkedList<QuadTuple> ILProgram { get; set; }
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
        private void GenerateIR()
        {
            ILProgram = new LinkedList<QuadTuple>();
            Root.ILGenerate(ILProgram);
            ILProgram.PrintToConsole();
            try
            {
                var stream = new FileStream(OutputFile, FileMode.Create, FileAccess.Write);
                using (var writer = new StreamWriter(stream))
                {
                    var jsonIR = JsonConvert.SerializeObject(ILProgram, Formatting.Indented);
                    writer.Write(jsonIR);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine($"Could not write file: {OutputFile}");
            }
        }
        public void run(string arg)
        {
            var start = DateTime.Now;
            Console.WriteLine($"Reading \"{arg}\"...");
            if (LoadAST(arg) != 0) return;
            Console.WriteLine("Generating IR...");
            GenerateIR();
            var end = DateTime.Now;
            Console.WriteLine($"Done in {(end - start).TotalMilliseconds}ms.");
        }
    }
}