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
                Console.Write("({0},", i.Operator);
                if (i.RValueA is not null) Console.Write(i.RValueA.ID);
                Console.Write(",");
                if (i.RValueB is not null) Console.Write(i.RValueB.ID);
                Console.Write(",");
                if (i.LValue is not null) Console.Write(i.LValue.ID);
                Console.Write(")\n");
            }
        }
        public static void ZipBack(this LinkedList<QuadTuple> ILProgram, Dictionary<string, int> ZipBackTable, int offset)
        {

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
            Root.ILGenerate(ILProgram, null);
            Console.WriteLine("---------------IR Code---------------");
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