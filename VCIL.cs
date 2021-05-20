using System;

namespace VinewoodCC
{
    public enum ILNameType
    {
        Var = 0,
        TmpVar = 1,
        Function = 2,
        StringConstant = 3,
        CharConstant = 4,
        IntegerConstant = 5,
        FPConstant = 6
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
        Jne = 8,
        Param = 9
    }
    public struct ILIdentifier
    {
        public string ID { get; set; }
        public ILNameType ILNameType { get; set; }
    }
    public class QuadTuple
    {
        public ILOperator Operator { get; set; }
        public ILIdentifier RValueA { get; set; }
        public ILIdentifier RValueB { get; set; }
        public ILIdentifier LValue { get; set; }
    }
}