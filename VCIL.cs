using System;

namespace VinewoodCC
{
    public enum ILNameType
    {
        Var = 0,
        TmpVar = 1,
        Function = 2
    }
    public enum ILOperator
    {
        Assign = 0,//=
        DataBegin = 1,
        DataEnd = 2,
        ProcBegin = 3,
        ProcEnd = 4,
        Call = 5,
        Jmp = 6,
        Je = 7,
        Jne = 8
    }
    public struct ILIdentifier
    {
        public string ID { get; set; }
        public ILNameType ILNameType { get; set; }
    }
    public class QuadTuple
    {
        ILOperator Operator { get; set; }
        ILIdentifier RValueA { get; set; }
        ILIdentifier RValueB { get; set; }
        ILIdentifier LValue { get; set; }
    }
}