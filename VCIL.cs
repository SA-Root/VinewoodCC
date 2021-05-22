using System;
using System.Collections.Generic;

namespace VinewoodCC
{
    public static class ILProgramExtension
    {
        public static void MergePostfix(this List<QuadTuple> ILProgram)
        {
            if (ILGenerator.PostfixCache.Count > 0)
            {
                ILProgram.AddRange(ILGenerator.PostfixCache);
                ILGenerator.PostfixCache.Clear();
            }
        }
        public static void InjectConstant(this QuadTuple qt, ASTConstant constant, bool isRValue)
        {
            if (constant is ASTStringConstant sc)
            {
                if (isRValue)
                {
                    qt.RValueA = new ILIdentifier(sc.Value, ILNameType.Constant, "string");
                }
                else
                {
                    qt.LValue = new ILIdentifier(sc.Value, ILNameType.Constant, "string");
                }
            }
            else if (constant is ASTIntegerConstant ic)
            {
                if (isRValue)
                {
                    qt.RValueA = new ILIdentifier(ic.Value.ToString(), ILNameType.Constant, "int");
                }
                else
                {
                    qt.LValue = new ILIdentifier(ic.Value.ToString(), ILNameType.Constant, "int");
                }
            }
            else if (constant is ASTFloatConstant fp)
            {
                if (isRValue)
                {
                    qt.RValueA = new ILIdentifier(fp.Value.ToString(), ILNameType.Constant, "float");
                }
                else
                {
                    qt.LValue = new ILIdentifier(fp.Value.ToString(), ILNameType.Constant, "float");
                }
            }
            else if (constant is ASTCharConstant cc)
            {
                if (isRValue)
                {
                    qt.RValueA = new ILIdentifier(cc.Value.ToString(), ILNameType.Constant, "char");
                }
                else
                {
                    qt.LValue = new ILIdentifier(cc.Value.ToString(), ILNameType.Constant, "char");
                }
            }
        }
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
    }
    public enum ILNameType
    {
        Var = 0,
        TmpVar = 1,
        Function = 2,
        Constant = 3
    }
    public enum ILOperator
    {
        ArrayAssign = -3,//writeback to array elements
        Push = -2,//push arg for func call
        VarDefine = -1,//(local/global) var definition
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
        ArrayAccess = 13,//lvalue is address of the array element
        Subtract = 14,//-
        Multiply = 15,//*
        Division = 16,///
        Greater = 17,//>
        Less = 18,//<
        Equal = 19,//==
        Increase = 20,//++
        Decrease = 21,//--
        GreaterEqual = 22,//>=
        LessEqual = 23,//<=
        Not = 24//!
    }
    public class ILIdentifier
    {
        public string ID { get; set; }
        public ILNameType ILNameType { get; set; }
        //addr: target is an address
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