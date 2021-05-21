using System;

namespace VinewoodCC
{
    public enum ILNameType
    {
        Var = 0,
        TmpVar = 1,
        Function = 2,
        Constant = 3
    }
    public enum ILOperator
    {
        Assign = 0,//=
        DataBegin = 1,//.data
        DataEnd = 2,//.code
        ProcBegin = 3,//xx proc 
        ProcEnd = 4,//xx endp
        Call = 5,
        Jmp = 6,
        Je = 7,
        Jne = 8,
        Param = 9,// xx:type
        Return = 10,//ret
        JmpTarget = 11,//jmp label in asm file
        Add = 12,//+
        ArrayAccess = 13//tmp=a[i][j]
    }
    public class ILIdentifier
    {
        public string ID { get; set; }
        public ILNameType ILNameType { get; set; }
        public string ValueType { get; set; }
        public ILIdentifier(string id, ILNameType nt, string vt)
        {
            ID = id;
            ILNameType = nt;
            ValueType = vt;
        }
        public ILIdentifier() { }
    }
    public class QuadTuple
    {
        public ILOperator Operator { get; set; }
        public ILIdentifier RValueA { get; set; }
        public ILIdentifier RValueB { get; set; }
        public ILIdentifier LValue { get; set; }
        public QuadTuple(ILOperator op, ILIdentifier rv1, ILIdentifier rv2, ILIdentifier lv)
        {
            Operator = op;
            RValueA = rv1;
            RValueB = rv2;
            LValue = lv;
        }
        public QuadTuple() { }
    }
}