using System;
using System.Collections.Generic;

namespace VinewoodCC
{
    public static class ILProgramExtension
    {
        public static void PrintToConsole(this List<QuadTuple> ILProgram)
        {
            foreach (var i in ILProgram)
            {

            }
        }
        public static QuadTuple LastItem(this List<QuadTuple> ILProgram)
        => ILProgram[ILProgram.Count - 1];
    }
    public class ILGenerator
    {
        public void run(string path)
        {

        }
    }
}