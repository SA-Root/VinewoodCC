using System;
using System.Collections.Generic;
using VinewoodCC.ILGen;

namespace VinewoodCC
{
    namespace NGen
    {
        class NativeCodeGenenrator
        {
            private List<QuadTuple> ILCode { get; set; }
            public NativeCodeGenenrator(List<QuadTuple> ilc)
            {
                ILCode = ilc;
            }
            public void Run()
            {

            }
        }
    }
}