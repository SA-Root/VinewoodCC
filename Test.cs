using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace VinewoodCC
{
    [SimpleJob(RunStrategy.ColdStart, targetCount: 5)]
    public class Test
    {
        [Benchmark]
        public void run()
        {
            var LS = new LexSharp.Program();
            LS.Run("E:/7_Dijkstra.c");
            var PS = new ParserSharp.Parser();
            PS.Run(LS.OutputFile);
        }
    }
}
