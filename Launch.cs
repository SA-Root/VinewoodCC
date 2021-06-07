using System;
using System.Collections.Generic;
using VinewoodCC.ILGen;
using VinewoodCC.Semantic;
using VinewoodCC.ParserSharp;
using VinewoodCC.NGen;

namespace VinewoodCC
{
    public class Program
    {
        static void run(string path)
        {
            var start = DateTime.Now;
            var LS = new LexSharp.Lexer();
            LS.Run2(path);
            var PS = new Parser();
            PS.Run2(LS.OutputFile);
            var SC = new Semantica();
            SC.Run2(PS.OutputFile);
            if (Semantica.HasError == 0)
            {
                var IRG = new ILGenerator(SC.Root);
                IRG.Run2(PS.OutputFile);
                var NCG = new NativeCodeGenenrator(IRG.ILProgram);
                NCG.Run();
            }
            var end = DateTime.Now;
            Console.WriteLine($"Done in {(end - start).TotalMilliseconds}ms.");
        }
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("[ERROR]No input file.");
                return;
            }
            run(args[0]);
            //_ = BenchmarkRunner.Run<Test>();
        }
    }
}
