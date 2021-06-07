using System;
using System.IO;
using System.Collections.Generic;
using VinewoodCC.ILGen;

namespace VinewoodCC
{
    namespace NGen
    {
        class NativeCodeGenenrator
        {
            private bool isLocal { get; set; }
            private Dictionary<string, string> GSymbols { get; set; } = new Dictionary<string, string>();
            private Dictionary<string, string> LSymbols { get; set; } = new Dictionary<string, string>();
            private List<string> DataSegment { get; set; } = new List<string>();
            private List<string> CodeSegment { get; set; } = new List<string>();
            private List<string> ProcSegment { get; set; }
            private List<string> AssemblyCode { get; set; } = new List<string>();
            private List<QuadTuple> ILCode { get; set; }
            private string OutputFile { get; set; }
            public NativeCodeGenenrator(List<QuadTuple> ilc)
            {
                ILCode = ilc;
                isLocal = false;
            }
            private void ILArrayDefine(QuadTuple qt)
            {
                var tplt = "{0} {1} {2} DUP({3})";
                var tplt_local = "   local {0}[{1}]:{2}";
                var vtype = qt.LValue.ValueType;
                var len = int.Parse(qt.RValueA.ID);
                if (vtype == "int")
                {
                    if (isLocal)
                    {
                        LSymbols[qt.LValue.ID] = qt.LValue.ValueType;
                        ProcSegment.Add(string.Format(tplt_local, qt.LValue.ID, len, "dword"));
                    }
                    else
                    {
                        GSymbols[qt.LValue.ID] = qt.LValue.ValueType;
                        DataSegment.Add(string.Format(tplt, qt.LValue.ID, "dword", len, 0));
                    }
                }
            }
            private void ILArrayAssign(QuadTuple qt)
            {
                var tplt1 = "   mov eax,{0}";
                var offset = qt.RValueA.ID;
                if (qt.RValueA.ValueType == "addr")
                {
                    if (LSymbols.ContainsKey(offset))
                    {
                        if (LSymbols[offset] == "addr")
                        {
                            ProcSegment.Add(string.Format(tplt1, "dword ptr " + offset));
                        }
                    }
                    else
                    {
                        if (GSymbols[offset] == "addr")
                        {
                            ProcSegment.Add(string.Format(tplt1, "dword ptr " + offset));
                        }
                    }
                }
                else
                {
                    ProcSegment.Add(string.Format(tplt1, offset));
                }
                var tplt2 = "   mov {0},eax";
                offset = qt.LValue.ID;
                if (LSymbols.ContainsKey(offset))
                {
                    if (LSymbols[offset] == "addr")
                    {
                        ProcSegment.Add(string.Format(tplt2, "dword ptr " + offset));
                    }
                }
                else
                {
                    if (GSymbols[offset] == "addr")
                    {
                        ProcSegment.Add(string.Format(tplt2, "dword ptr " + offset));
                    }
                }
            }
            private void ILPush(QuadTuple qt)
            {
                var tplt = "   push {0}";
                ProcSegment.Add(string.Format(tplt, qt.LValue.ID));
            }
            private void ILVarDefine(QuadTuple qt)
            {
                var tplt = "{0} {1} {2}";
                var vtype = qt.LValue.ValueType;
                var name = qt.LValue.ID;
                if (vtype == "int")
                {
                    if (isLocal)
                    {
                        if (!LSymbols.ContainsKey(name))
                        {
                            var tplt_local = "   local {0}:{1}";
                            LSymbols[name] = vtype;
                            ProcSegment.Insert(0, string.Format(tplt_local, name, "dword"));
                        }
                        if (qt.RValueA != null)
                        {
                            if (qt.RValueA.ILNameType == ILNameType.Constant)
                            {
                                var tplt1 = "   mov {0},{1}";
                                ProcSegment.Add(string.Format(tplt1, name, qt.RValueA.ID));
                            }
                            else
                            {
                                var tplt1 = "   mov eax,{0}";
                                var tplt2 = "   mov {0},eax";
                                if (qt.RValueA.ValueType == "addr")
                                {
                                    ProcSegment.Add(string.Format(tplt1, "dword ptr " + qt.RValueA.ID));
                                }
                                else
                                {
                                    ProcSegment.Add(string.Format(tplt1, qt.RValueA.ID));
                                }
                                ProcSegment.Add(string.Format(tplt2, name));
                            }
                        }
                        else
                        {
                            ProcSegment.Add(string.Format("   mov {0},0", name));
                        }
                    }
                    else
                    {
                        GSymbols[name] = vtype;
                        if (qt.RValueA != null)
                        {
                            if (qt.RValueA.ILNameType == ILNameType.Constant)
                            {
                                var tplt1 = "   mov {0},{1}";
                                ProcSegment.Add(string.Format(tplt1, name, qt.RValueA.ID));
                            }
                            else
                            {
                                var tplt1 = "   mov eax,{0}";
                                var tplt2 = "   mov {0},eax";
                                if (qt.RValueA.ValueType == "addr")
                                {
                                    ProcSegment.Add(string.Format(tplt1, "dword ptr " + qt.RValueA.ID));
                                }
                                else
                                {
                                    ProcSegment.Add(string.Format(tplt1, qt.RValueA.ID));
                                }
                                ProcSegment.Add(string.Format(tplt2, name));
                            }
                        }
                        else
                        {
                            DataSegment.Add(string.Format(tplt, name, "dword", 0));
                        }
                    }
                }
                else if (vtype == "string")
                {
                    GSymbols[name] = "string";
                    var tplt1 = "{0} byte {1},0";
                    DataSegment.Add(string.Format(tplt1, name, qt.RValueA.ID));
                }
            }
            private void ILAssign(QuadTuple qt)
            {
                var tplt1 = "   mov eax,{0}";
                var tplt2 = "   mov {0},eax";

            }
            private void ILDataBegin(QuadTuple qt)
            {
                DataSegment.Add(".data");
            }
            private void ILDataEnd(QuadTuple qt)
            {
                DataSegment.Add(".code");
            }
            private void ILProcBegin(QuadTuple qt)
            {
                var tplt = "{0} proc stdcall";
                CodeSegment.Add(string.Format(tplt, qt.LValue.ID));
            }
            private void ILProcEnd(QuadTuple qt)
            {
                var tplt = "{0} endp";
                CodeSegment.Add(string.Format(tplt, qt.LValue.ID));
            }
            private void ILCall(QuadTuple qt)
            {

            }
            private void ILJmp(QuadTuple qt)
            {

            }
            private void ILJe(QuadTuple qt)
            {

            }
            private void ILJne(QuadTuple qt)
            {

            }
            private void ILParam(QuadTuple qt)
            {

            }
            private void ILReturn(QuadTuple qt)
            {

            }
            private void ILJmpTarget(QuadTuple qt)
            {

            }

            private void ILAdd(QuadTuple qt)
            {

            }
            private void ILArrayAccess(QuadTuple qt)
            {
                var tplt1 = "   mov eax,{0}";
                ProcSegment.Add(string.Format(tplt1, qt.RValueB.ID));
                var tplt2 = "   mov {0},[{1}+{2}*eax]";
                var arrName = qt.RValueA.ID;
                var target = qt.LValue.ID;
                if (!LSymbols.ContainsKey(target) && !GSymbols.ContainsKey(target))
                {
                    if (qt.LValue.ValueType == "int" || qt.LValue.ValueType == "addr")
                    {
                        LSymbols[target] = qt.LValue.ValueType;
                        ProcSegment.Insert(0, string.Format("   local {0}:dword", target));
                    }
                }
                if (LSymbols.ContainsKey(arrName))
                {
                    if (LSymbols[arrName] == "int" || LSymbols[arrName] == "addr")
                    {
                        ProcSegment.Add(string.Format(tplt2, target, arrName, 4));
                    }
                }
                else
                {
                    if (GSymbols[arrName] == "int" || LSymbols[arrName] == "addr")
                    {
                        ProcSegment.Add(string.Format(tplt2, target, arrName, 4));
                    }
                }
            }
            private void ILSubtract(QuadTuple qt)
            {

            }
            private void ILMultiply(QuadTuple qt)
            {

            }
            private void ILDivision(QuadTuple qt)
            {

            }
            private void ILGreater(QuadTuple qt)
            {

            }
            private void ILLess(QuadTuple qt)
            {

            }
            private void ILEqual(QuadTuple qt)
            {

            }
            private void ILIncrease(QuadTuple qt)
            {

            }
            private void ILDecrease(QuadTuple qt)
            {

            }
            private void ILGreaterEqual(QuadTuple qt)
            {

            }
            private void ILLessEqual(QuadTuple qt)
            {

            }
            private void ILNot(QuadTuple qt)
            {

            }
            private void ILLoadAddress(QuadTuple qt)
            {

            }
            private void ILPushAddr(QuadTuple qt)
            {

            }
            public void Run(string path)
            {
                Console.WriteLine("Generating Asssembly....");
                AssemblyCode.Add(".model flat,stdcall");
                AssemblyCode.Add("option casemap:none");
                AssemblyCode.Add("include msvcrt.inc");
                AssemblyCode.Add("includelib msvcrt.lib");
                foreach (var item in ILCode)
                {
                    switch (item.Operator)
                    {
                        case ILOperator.Add:
                            ILAdd(item);
                            break;
                        case ILOperator.ArrayAccess:
                            ILArrayAccess(item);
                            break;
                        case ILOperator.ArrayAssign:
                            ILArrayAssign(item);
                            break;
                        case ILOperator.ArrayDefine:
                            ILArrayDefine(item);
                            break;
                        case ILOperator.Assign:
                            ILAssign(item);
                            break;
                        case ILOperator.Call:
                            ILCall(item);
                            break;
                        case ILOperator.DataBegin:
                            ILDataBegin(item);
                            break;
                        case ILOperator.DataEnd:
                            ILDataEnd(item);
                            break;
                        case ILOperator.Decrease:
                            ILDecrease(item);
                            break;
                        case ILOperator.Division:
                            ILDivision(item);
                            break;
                        case ILOperator.Equal:
                            ILEqual(item);
                            break;
                        case ILOperator.Greater:
                            ILGreater(item);
                            break;
                        case ILOperator.GreaterEqual:
                            ILGreaterEqual(item);
                            break;
                        case ILOperator.Increase:
                            ILIncrease(item);
                            break;
                        case ILOperator.Je:
                            ILJe(item);
                            break;
                        case ILOperator.Jmp:
                            ILJmp(item);
                            break;
                        case ILOperator.JmpTarget:
                            ILJmpTarget(item);
                            break;
                        case ILOperator.Jne:
                            ILJne(item);
                            break;
                        case ILOperator.Less:
                            ILLess(item);
                            break;
                        case ILOperator.LessEqual:
                            ILLessEqual(item);
                            break;
                        case ILOperator.LoadAddress:
                            ILLoadAddress(item);
                            break;
                        case ILOperator.Multiply:
                            ILMultiply(item);
                            break;
                        case ILOperator.Not:
                            ILNot(item);
                            break;
                        case ILOperator.Param:
                            ILParam(item);
                            break;
                        case ILOperator.ProcBegin:
                            LSymbols = new Dictionary<string, string>();
                            isLocal = true;
                            ILProcBegin(item);
                            ProcSegment = new List<string>();
                            break;
                        case ILOperator.ProcEnd:
                            isLocal = false;
                            CodeSegment.AddRange(ProcSegment);
                            ILProcEnd(item);
                            break;
                        case ILOperator.Push:
                            ILPush(item);
                            break;
                        case ILOperator.PushAddr:
                            ILPushAddr(item);
                            break;
                        case ILOperator.Return:
                            ILReturn(item);
                            break;
                        case ILOperator.Subtract:
                            ILSubtract(item);
                            break;
                        case ILOperator.VarDefine:
                            ILVarDefine(item);
                            break;
                        default:
                            break;
                    }
                }
                AssemblyCode.AddRange(DataSegment);
                AssemblyCode.AddRange(CodeSegment);
                AssemblyCode.Add("end main");
                WriteToFile(path);
            }
            private void WriteToFile(string path)
            {
                OutputFile = path.Substring(0, path.LastIndexOf(".vcil")) + ".asm";
                try
                {
                    var stream = new FileStream(OutputFile, FileMode.Create, FileAccess.Write);
                    using (var writer = new StreamWriter(stream))
                    {
                        foreach (var i in AssemblyCode)
                        {
                            writer.WriteLine(i);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine($"Could not write file: {OutputFile}");
                }
            }
        }
    }
}