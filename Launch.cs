using System;
using BenchmarkDotNet.Running;

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
        }
        public static void Main(string[] args)
        {
            run(args[0]);
            //_ = BenchmarkRunner.Run<Test>();
        }
    }
}
