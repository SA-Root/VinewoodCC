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
            private List<string> PushQueue { get; set; } = null;
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
                var vtype = qt.LValue.ValueType;
                var len = int.Parse(qt.RValueA.ID);
                if (vtype == "int")
                {
                    GSymbols[qt.LValue.ID] = qt.LValue.ValueType;
                    DataSegment.Insert(1, string.Format(tplt, qt.LValue.ID, "dword", len, 0));
                }
            }
            private void ILArrayAssign(QuadTuple qt)
            {
                var tplt1 = "   lea eax,{0}";
                var offset = qt.RValueA.ID;
                ProcSegment.Add(string.Format(tplt1, offset));
                var tplt2 = "   mov ebx,{0}";
                offset = qt.RValueB.ID;
                ProcSegment.Add(string.Format(tplt2, offset));
                ProcSegment.Add("   imul ebx,ebx,4");
                ProcSegment.Add("   add eax,ebx");
                var tplt3 = "   mov {0},eax";
                ProcSegment.Add(string.Format(tplt3, qt.LValue.ID));
            }
            private void ILPush(QuadTuple qt)
            {
                var tplt = ",{0}";
                if (PushQueue is null)
                {
                    PushQueue = new List<string>();
                }
                if (qt.LValue.ValueType == "string")
                {
                    PushQueue.Add(string.Format(tplt, "offset " + qt.LValue.ID));
                }
                else
                {
                    PushQueue.Add(string.Format(tplt, qt.LValue.ID));
                }
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
                        if (qt.RValueA is not null)
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
                        if (qt.RValueA is not null)
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
                    var str = qt.RValueA.ID;
                    if (str.Length >= 4 && str[str.Length - 3] == '\\' && str[str.Length - 2] == 'n')
                    {
                        str = str.Remove(str.Length - 3, 2);
                        if (str.Length <= 4)
                        {
                            DataSegment.Add(string.Format(tplt1, name, "0ah"));
                        }
                        else
                        {
                            DataSegment.Add(string.Format(tplt1, name, str + ",0ah"));
                        }
                    }
                    else
                    {
                        DataSegment.Add(string.Format(tplt1, name, str));
                    }
                }
            }
            private void ILAssign(QuadTuple qt)
            {
                var tplt1 = "   mov eax,{0}";
                if (qt.RValueA.ValueType == "addr")
                {
                    ProcSegment.Add(string.Format(tplt1, "dword ptr " + qt.RValueA.ID));
                }
                else
                {
                    ProcSegment.Add(string.Format(tplt1, qt.RValueA.ID));
                }
                var target = qt.LValue.ID;
                if (!LSymbols.ContainsKey(target) && !GSymbols.ContainsKey(target))
                {
                    ProcSegment.Insert(0, string.Format("   local {0}:dword", target));
                    LSymbols[target] = "int";
                }
                if (qt.LValue.ValueType == "addr")
                {
                    var tplt3 = "   mov ebx,{0}";
                    ProcSegment.Add(string.Format(tplt3, qt.LValue.ID));
                    ProcSegment.Add("   mov [ebx],eax");
                }
                else
                {
                    var tplt2 = "   mov {0},eax";
                    ProcSegment.Add(string.Format(tplt2, target));
                }
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
                var tplt = "{0} proc stdcall ";
                CodeSegment.Add(string.Format(tplt, qt.LValue.ID));
            }
            private void ILProcEnd(QuadTuple qt)
            {
                var tplt = "{0} endp";
                CodeSegment.Add(string.Format(tplt, qt.LValue.ID));
            }
            private void ILCall(QuadTuple qt)
            {
                var tplt = "   invoke {0}";
                if (qt.RValueA.ID == "scanf" || qt.RValueA.ID == "printf")
                {
                    ProcSegment.Add(string.Format(tplt, "crt_" + qt.RValueA.ID));
                }
                else
                {
                    ProcSegment.Add(string.Format(tplt, qt.RValueA.ID));
                }
                if (PushQueue is not null)
                {
                    foreach (var i in PushQueue)
                    {
                        ProcSegment[ProcSegment.Count - 1] += i;
                    }
                    PushQueue = null;
                }
            }
            private void ILJmp(QuadTuple qt)
            {
                ProcSegment.Add(string.Format("   jmp {0}", qt.LValue.ID));
            }
            private void ILJe(QuadTuple qt)
            {
                var tplt1 = "   cmp {0},{1}";
                ProcSegment.Add(string.Format(tplt1, qt.RValueA.ID, qt.RValueB.ID));
                var tplt2 = "   je {0}";
                ProcSegment.Add(string.Format(tplt2, qt.LValue.ID));
            }
            private void ILJne(QuadTuple qt)
            {
                var tplt1 = "   cmp {0},{1}";
                ProcSegment.Add(string.Format(tplt1, qt.RValueA.ID, qt.RValueB.ID));
                var tplt2 = "   jne {0}";
                ProcSegment.Add(string.Format(tplt2, qt.LValue.ID));
            }
            private void ILParam(QuadTuple qt)
            {
                var last = CodeSegment[CodeSegment.Count - 1];
                if (last[last.Length - 1] == ' ')
                {
                    CodeSegment[CodeSegment.Count - 1] += string.Format("{0}:dword", qt.LValue.ID);
                }
                else
                {
                    CodeSegment[CodeSegment.Count - 1] += string.Format(",{0}:dword", qt.LValue.ID);
                }
            }
            private void ILReturn(QuadTuple qt)
            {
                if (qt.LValue is not null)
                {
                    var tplt1 = "   mov eax,{0}";
                    ProcSegment.Add(string.Format(tplt1, qt.LValue.ID));
                }
                var tplt2 = "   ret";
                ProcSegment.Add(tplt2);
            }
            private void ILJmpTarget(QuadTuple qt)
            {
                ProcSegment.Add(string.Format("{0}:", qt.LValue.ID));
            }
            private void ILSimpleBinary(QuadTuple qt, string op)
            {
                var tplt1 = "   mov eax,{0}";
                if (qt.RValueA.ValueType == "addr")
                {
                    ProcSegment.Add(string.Format(tplt1, "dword ptr " + qt.RValueA.ID));
                }
                else
                {
                    ProcSegment.Add(string.Format(tplt1, qt.RValueA.ID));
                }
                var tplt2 = "   {0} eax,{1}";
                if (qt.RValueB.ValueType == "addr")
                {
                    ProcSegment.Add(string.Format(tplt2, op, "dword ptr " + qt.RValueB.ID));
                }
                else
                {
                    ProcSegment.Add(string.Format(tplt2, op, qt.RValueB.ID));
                }
                var tplt3 = "   mov {0},eax";
                var target = qt.LValue.ID;
                if (!LSymbols.ContainsKey(target) && !GSymbols.ContainsKey(target))
                {
                    ProcSegment.Insert(0, string.Format("   local {0}:dword", target));
                    LSymbols[target] = "int";
                }
                ProcSegment.Add(string.Format(tplt3, target));
            }
            //calc &  store the addr of the element
            private void ILArrayAccess(QuadTuple qt)
            {
                var tplt1 = "   mov eax,{0}";
                ProcSegment.Add(string.Format(tplt1, qt.RValueB.ID));
                if (qt.LValue.ValueType == "int" || qt.LValue.ValueType == "addr")
                {
                    ProcSegment.Add(string.Format("   imul eax,eax,{0}", 4));
                }
                var arrName = qt.RValueA.ID;
                ProcSegment.Add(string.Format("   lea ebx,{0}", arrName));
                ProcSegment.Add("   mov eax,[eax][ebx]");
                var tplt2 = "   mov {0},eax";
                var target = qt.LValue.ID;
                if (!LSymbols.ContainsKey(target) && !GSymbols.ContainsKey(target))
                {
                    if (qt.LValue.ValueType == "int" || qt.LValue.ValueType == "addr")
                    {
                        LSymbols[target] = "int";
                        ProcSegment.Insert(0, string.Format("   local {0}:dword", target));
                    }
                }
                ProcSegment.Add(string.Format(tplt2, target));
            }
            private void ILMultiply(QuadTuple qt)
            {
                ProcSegment.Add("   xor edx,edx");
                var tplt1 = "   mov eax,{0}";
                if (qt.RValueA.ValueType == "addr")
                {
                    ProcSegment.Add(string.Format(tplt1, "dword ptr " + qt.RValueA.ID));
                }
                else
                {
                    ProcSegment.Add(string.Format(tplt1, qt.RValueA.ID));
                }
                var tplt2 = "   imul {1}";
                if (qt.RValueB.ValueType == "addr")
                {
                    ProcSegment.Add(string.Format(tplt2, "dword ptr " + qt.RValueB.ID));
                }
                else
                {
                    ProcSegment.Add(string.Format(tplt2, qt.RValueB.ID));
                }
                var tplt3 = "   mov {0},eax";
                var target = qt.LValue.ID;
                if (!LSymbols.ContainsKey(target) && !GSymbols.ContainsKey(target))
                {
                    ProcSegment.Insert(0, string.Format("   local {0}:dword", target));
                    LSymbols[target] = "int";
                }
                ProcSegment.Add(string.Format(tplt3, target));
            }
            private void ILDivMod(QuadTuple qt, string op)
            {
                ProcSegment.Add("   xor edx,edx");
                var tplt1 = "   mov eax,{0}";
                if (qt.RValueA.ValueType == "addr")
                {
                    ProcSegment.Add(string.Format(tplt1, "dword ptr " + qt.RValueA.ID));
                }
                else
                {
                    ProcSegment.Add(string.Format(tplt1, qt.RValueA.ID));
                }
                var tplt2 = "   mov ebx,{1}";
                if (qt.RValueB.ValueType == "addr")
                {
                    ProcSegment.Add(string.Format(tplt2, "dword ptr " + qt.RValueB.ID));
                }
                else
                {
                    ProcSegment.Add(string.Format(tplt2, qt.RValueB.ID));
                }
                ProcSegment.Add("   idiv ebx");
                var tplt3 = "   mov {0},{1}";
                var target = qt.LValue.ID;
                if (!LSymbols.ContainsKey(target) && !GSymbols.ContainsKey(target))
                {
                    ProcSegment.Insert(0, string.Format("   local {0}:dword", target));
                    LSymbols[target] = "int";
                }
                ProcSegment.Add(string.Format(tplt3, target, op));
            }
            private void ILJc(QuadTuple qt, string op)
            {
                var tplt1 = "   mov eax,{0}";
                if (qt.RValueA.ValueType == "addr")
                {
                    ProcSegment.Add(string.Format(tplt1, "dword ptr " + qt.RValueA.ID));
                }
                else
                {
                    ProcSegment.Add(string.Format(tplt1, qt.RValueA.ID));
                }
                var tplt3 = "   mov ebx,{0}";
                if (qt.RValueA.ValueType == "addr")
                {
                    ProcSegment.Add(string.Format(tplt3, "dword ptr " + qt.RValueB.ID));
                }
                else
                {
                    ProcSegment.Add(string.Format(tplt3, qt.RValueB.ID));
                }
                ProcSegment.Add("   cmp eax,ebx");
                var tplt2 = "   {0} {1}";
                ProcSegment.Add(string.Format(tplt2, op, qt.LValue.ID));
            }
            private void ILIncrease(QuadTuple qt)
            {
                var tplt = "   inc {0}";
                if (qt.LValue.ValueType == "addr")
                {
                    ProcSegment.Add(string.Format(tplt, "dword ptr " + qt.LValue.ID));
                }
                else
                {
                    ProcSegment.Add(string.Format(tplt, qt.LValue.ID));
                }
            }
            private void ILDecrease(QuadTuple qt)
            {
                var tplt = "   dec {0}";
                if (qt.LValue.ValueType == "addr")
                {
                    ProcSegment.Add(string.Format(tplt, "dword ptr " + qt.LValue.ID));
                }
                else
                {
                    ProcSegment.Add(string.Format(tplt, qt.LValue.ID));
                }
            }
            private void ILLoadAddress(QuadTuple qt)
            {
                var tplt1 = "   lea eax,{0}";
                ProcSegment.Add(string.Format(tplt1, qt.RValueA.ID));
                if (qt.RValueB is not null)
                {
                    var tplt3 = "   mov ebx,{0}";
                    ProcSegment.Add(string.Format(tplt3, qt.RValueB.ID));
                    ProcSegment.Add("   imul ebx,ebx,4");
                    ProcSegment.Add("   add eax,ebx");
                }
                var tplt2 = "   mov {0},eax";
                var target = qt.LValue.ID;
                if (!LSymbols.ContainsKey(target) && !GSymbols.ContainsKey(target))
                {
                    LSymbols[target] = "addr";
                    ProcSegment.Insert(0, string.Format("   local {0}:dword", target));
                }
                ProcSegment.Add(string.Format(tplt2, target));
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
                        case ILOperator.Module:
                            ILDivMod(item, "edx");
                            break;
                        case ILOperator.Add:
                            ILSimpleBinary(item, "add");
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
                            ILDivMod(item, "eax");
                            break;
                        case ILOperator.Je:
                            ILJc(item, "je");
                            break;
                        case ILOperator.Jne:
                            ILJc(item, "jne");
                            break;
                        case ILOperator.Jg:
                            ILJc(item, "jg");
                            break;
                        case ILOperator.Jge:
                            ILJc(item, "jge");
                            break;
                        case ILOperator.Increase:
                            ILIncrease(item);
                            break;
                        case ILOperator.Jmp:
                            ILJmp(item);
                            break;
                        case ILOperator.JmpTarget:
                            ILJmpTarget(item);
                            break;
                        case ILOperator.Jl:
                            ILJc(item, "jl");
                            break;
                        case ILOperator.Jle:
                            ILJc(item, "jle");
                            break;
                        case ILOperator.LoadAddress:
                            ILLoadAddress(item);
                            break;
                        case ILOperator.Multiply:
                            ILMultiply(item);
                            break;
                        case ILOperator.Jnz:
                            ILJc(item, "jnz");
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
                        case ILOperator.Return:
                            ILReturn(item);
                            break;
                        case ILOperator.Subtract:
                            ILSimpleBinary(item, "sub");
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