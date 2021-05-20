using System;
using BenchmarkDotNet.Running;
using System.Collections.Generic;

namespace VinewoodCC
{
    public class Program
    {
        static void run(string path)
        {
            var LS = new LexSharp.Program();
            LS.Run(path);
            var PS = new ParserSharp.Parser();
            PS.Run(LS.OutputFile);
            var SC = new Semantica();
            SC.run(PS.OutputFile);
            if (Semantica.HasError != 0) return;
            var IRG = new ILGenerator();
            IRG.run(PS.OutputFile);
        }
        public static void Main(string[] args)
        {
            run(args[0]);
            //_ = BenchmarkRunner.Run<Test>();
        }
    }
}
