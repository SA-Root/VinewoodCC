using System;
using System.Collections.Generic;
using VinewoodCC.ILGen;

namespace VinewoodCC
{
    namespace NGen
    {
        class NativeCodeGenenrator
        {
            private List<string> DataSegment { get; set; } = new List<string>();
            private List<string> CodeSegment { get; set; } = new List<string>();
            private List<string> AssemblyCode { get; set; } = new List<string>();
            private List<QuadTuple> ILCode { get; set; }
            public NativeCodeGenenrator(List<QuadTuple> ilc)
            {
                ILCode = ilc;
            }
            private void ILArrayDefine(QuadTuple qt)
            {

            }
            private void ILArrayAssign(QuadTuple qt)
            {

            }
            private void ILPush(QuadTuple qt)
            {

            }
            private void ILVarDefine(QuadTuple qt)
            {

            }
            private void ILAssign(QuadTuple qt)
            {

            }
            private void ILDataBegin(QuadTuple qt)
            {

            }
            private void ILDataEnd(QuadTuple qt)
            {

            }
            private void ILProcBegin(QuadTuple qt)
            {

            }
            private void ILProcEnd(QuadTuple qt)
            {

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
            public void Run()
            {
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
                            ILProcBegin(item);
                            break;
                        case ILOperator.ProcEnd:
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
            }
        }
    }
}