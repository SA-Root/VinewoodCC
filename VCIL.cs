using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using VinewoodCC.AST;

namespace VinewoodCC
{
    namespace ILGen
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
            public static void InjectConstant(this QuadTuple qt, ASTConstant constant, int isRValue, ILIdentifier str = null)
            {
                if (constant is ASTStringConstant)
                {
                    if (isRValue == 0)
                    {
                        qt.RValueA = str;
                    }
                    else if (isRValue == 1)
                    {
                        qt.RValueB = str;
                    }
                    else
                    {
                        qt.LValue = str;
                    }
                }
                else if (constant is ASTIntegerConstant ic)
                {
                    if (isRValue == 0)
                    {
                        qt.RValueA = new ILIdentifier(ic.Value.ToString(), ILNameType.Constant, "int");
                    }
                    else if (isRValue == 1)
                    {
                        qt.RValueB = new ILIdentifier(ic.Value.ToString(), ILNameType.Constant, "int");
                    }
                    else
                    {
                        qt.LValue = new ILIdentifier(ic.Value.ToString(), ILNameType.Constant, "int");
                    }
                }
                else if (constant is ASTFloatConstant fp)
                {
                    if (isRValue == 0)
                    {
                        qt.RValueA = new ILIdentifier(fp.Value.ToString(), ILNameType.Constant, "float");
                    }
                    else if (isRValue == 1)
                    {
                        qt.RValueB = new ILIdentifier(fp.Value.ToString(), ILNameType.Constant, "float");
                    }
                    else
                    {
                        qt.LValue = new ILIdentifier(fp.Value.ToString(), ILNameType.Constant, "float");
                    }
                }
                else if (constant is ASTCharConstant cc)
                {
                    if (isRValue == 0)
                    {
                        qt.RValueA = new ILIdentifier(cc.Value.ToString(), ILNameType.Constant, "char");
                    }
                    else if (isRValue == 1)
                    {
                        qt.RValueB = new ILIdentifier(cc.Value.ToString(), ILNameType.Constant, "char");
                    }
                    else
                    {
                        qt.LValue = new ILIdentifier(cc.Value.ToString(), ILNameType.Constant, "char");
                    }
                }
            }
            public static QuadTuple Last(this List<QuadTuple> ILProgram)
            => ILProgram[^1];
            public static void PrintToConsole(this QuadTuple i)
            {
                Console.Write("({0},", i.Operator);
                if (i.RValueA is not null) Console.Write(i.RValueA.ID);
                Console.Write(",");
                if (i.RValueB is not null) Console.Write(i.RValueB.ID);
                Console.Write(",");
                if (i.LValue is not null) Console.Write(i.LValue.ID);
                Console.Write(")\n");
            }
            public static void PrintToConsole(this List<QuadTuple> ILProgram)
            {
                foreach (var i in ILProgram)
                {
                    i.PrintToConsole();
                }
            }
            public static void OutputQuadTuple(this List<QuadTuple> ILProgram, string path)
            {
                try
                {
                    var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
                    using var writer = new StreamWriter(stream);
                    foreach (var i in ILProgram)
                    {
                        writer.Write("({0},", i.Operator);
                        if (i.RValueA is not null) writer.Write(i.RValueA.ID);
                        writer.Write(",");
                        if (i.RValueB is not null) writer.Write(i.RValueB.ID);
                        writer.Write(",");
                        if (i.LValue is not null) writer.Write(i.LValue.ID);
                        writer.Write(")\n");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine($"Could not write file: {path}");
                }
            }
            public static void OutputJson(this List<QuadTuple> ILProgram, string path)
            {
                try
                {
                    var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
                    using var writer = new StreamWriter(stream);
                    var jsonIR = JsonSerializer.Serialize(ILProgram, typeof(List<QuadTuple>), SourceGenerationContext2.Default);
                    writer.Write(jsonIR);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine($"Could not write file: {path}");
                }
            }
        }
        public enum ILNameType
        {
            Var = 0,
            TmpVar = 1,
            Function = 2,
            Constant = 3,
            Array = 4
        }
        public enum ILOperator
        {
            ArrayDefine = -4,
            ArrayAssign = -3,//writeback addr
            Push = -2,//push arg for func call
            VarDefine = -1,//(local/global) var definition
            Assign = 0,//=
            DataBegin = 1,//.data
            DataEnd = 2,//.code
            ProcBegin = 3,//xx proc 
            ProcEnd = 4,//xx endp
            Call = 5,
            Jmp = 6,
            Jne = 7,
            Param = 9,// xx:type
            Return = 10,//ret
            JmpTarget = 11,//jmp label in asm file
            Add = 12,//+
            ArrayAccess = 13,//lvalue is address of the array element
            Subtract = 14,//-
            Multiply = 15,//*
            Division = 16,///
            Jg = 17,//>
            Jl = 18,//<
            Je = 19,//==
            Increase = 20,//++
            Decrease = 21,//--
            Jge = 22,//>=
            Jle = 23,//<=
            Jnz = 24,//!
            LoadAddress = 25,//&
            Module = 26//%
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

        [JsonSourceGenerationOptions(WriteIndented = true)]
        [JsonSerializable(typeof(List<QuadTuple>))]
        internal partial class SourceGenerationContext2 : JsonSerializerContext
        {
        }
    }
}