using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace VinewoodCC
{
    public static class ILProgramExtension
    {
        public static QuadTuple Last(this List<QuadTuple> ILProgram)
        => ILProgram[ILProgram.Count - 1];
        public static void PrintToConsole(this List<QuadTuple> ILProgram)
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
        public static void ZipBack(this List<QuadTuple> ILProgram, Dictionary<string, int> ZipBackTable, int offset)
        {
            for (int i = offset; i < ILProgram.Count; ++i)
            {
                if (ILProgram[i].Operator == ILOperator.JmpTarget)
                {
                    var name = ILProgram[i].LValue.ID;
                    if (ZipBackTable.ContainsKey(name))
                    {
                        var j = ZipBackTable[name];
                        do
                        {
                            var next = int.Parse(ILProgram[j].LValue.ID);
                            ILProgram[j].LValue = ILProgram[i].LValue;
                            j = next;
                        } while (ILProgram[j].LValue.ID != "0");
                        ZipBackTable.Remove(name);
                    }
                }
            }
        }
    }
    public class ILGenerator
    {
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
        private void GenerateIR()
        {
            ILProgram = new List<QuadTuple>();
            TmpCounter = 0;
            Root.ILGenerate(ILProgram, null, null);
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