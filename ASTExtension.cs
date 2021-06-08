using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VinewoodCC.ILGen;
using VinewoodCC.Semantic;

namespace VinewoodCC
{
    namespace AST
    {
        public partial class ASTNode
        {
            //extra args:
            //0: List<STLoopItem>
            //1: string vtype/enum isInLoop
            public virtual int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                return 0;
            }
            public virtual void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {

            }
        }
        public partial class ASTCompilationUnit : ASTNode
        {
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                GlobalSymbolTable = new Dictionary<string, STItem>();
                earg = new AOTCheckExtraArg();
                foreach (var i in Items)
                {
                    i.AOTCheck(GlobalSymbolTable, null, earg);
                }
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                ILProgram.Add(new QuadTuple(ILOperator.DataBegin, null, null, null));
                ILProgram.Add(new QuadTuple(ILOperator.DataEnd, null, null, null));
                foreach (var i in Items)
                {
                    i.ILGenerate(ILProgram, "global");
                }
            }
        }
        public partial class ASTFunctionDefine : ASTNode
        {
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                LocalSymbolTable = new Dictionary<string, STItem>();
                var funcDef = new STFunctionItem()
                {
                    ArgTypes = new List<string>(),
                    LST = LocalSymbolTable,
                    //get return type
                    RetType = Specifiers[0].Value,
                    //get func name
                    Identifier = ((Declarator as ASTFunctionDeclarator)?.Declarator as ASTVariableDeclarator)?.Identifier.Value
                };
                //get params
                var param = (Declarator as ASTFunctionDeclarator)?.Parameters;
                if (param is not null)
                {
                    foreach (var p in param)
                    {
                        var arg1 = new STVariableItem((p.Declarator as ASTVariableDeclarator)?.Identifier.Value, p.Specfiers[0].Value);
                        //redefine
                        if (LocalSymbolTable.ContainsKey(arg1.Identifier))
                        {
                            Console.WriteLine(SemanticErrors.VCE0001, arg1.Identifier);
                            Semantica.HasError = 1;
                        }
                        //new
                        else
                        {
                            LocalSymbolTable[arg1.Identifier] = arg1;
                            funcDef.ArgTypes.Add(arg1.ValueType);
                        }
                    }
                }
                //not defined
                if (!GST.ContainsKey(funcDef.Identifier))
                {
                    GST.Add(funcDef.Identifier, funcDef);
                }
                //defined
                else
                {
                    (GST[funcDef.Identifier] as STFunctionItem).LST = LocalSymbolTable;
                    (GST[funcDef.Identifier] as STFunctionItem).ArgTypes = funcDef.ArgTypes;
                }
                //in-function check
                foreach (var i in Body.BlockItems)
                {
                    i.AOTCheck(GST, LocalSymbolTable, earg);
                }
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                var funcName = ((Declarator as ASTFunctionDeclarator).Declarator as ASTVariableDeclarator).Identifier.Value;
                var fHead = new ILIdentifier(funcName, ILNameType.Function, null);
                ILProgram.Add(new QuadTuple(ILOperator.ProcBegin, null, null, fHead));
                //params
                var param = (Declarator as ASTFunctionDeclarator)?.Parameters;
                if (param is not null)
                {
                    foreach (var i in param)
                    {
                        var arg = (i.Declarator as ASTVariableDeclarator).Identifier.Value;
                        var Typename = i.Specfiers[0].Value;
                        ILProgram.Add(new QuadTuple(ILOperator.Param, null, null,
                            new ILIdentifier(arg, ILNameType.Var, Typename)));
                    }
                }
                var fStart = ILProgram.Count - 1;
                //body
                foreach (var i in Body.BlockItems)
                {
                    i.ILGenerate(ILProgram, "in-func");
                }
                ILProgram.Add(new QuadTuple(ILOperator.ProcEnd, null, null, fHead));
            }
        }
        public partial class ASTDeclaration : ASTNode
        {
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                earg.VType = Specifiers[0].Value;
                earg.MultiDimArray = new Stack<int>();
                foreach (var i in InitLists)
                {
                    i.Declarator.AOTCheck(GST, LST, earg);
                    if (i.Expressions.Count > 0)
                    {
                        i.Expressions[0].AOTCheck(GST, LST, earg);
                    }
                }
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                var DeclType = Specifiers[0].Value;
                foreach (var i in InitLists)
                {
                    i.Declarator.ILGenerate(ILProgram, DeclType);
                    if (DeclarationType == "global")
                    {
                        var last = ILProgram.Last();
                        ILProgram.Remove(ILProgram.Last());
                        ILProgram.Insert(1, last);
                        if (i.Expressions.Count > 0)
                        {
                            ILProgram[1].InjectConstant(i.Expressions[0] as ASTConstant, 0);
                        }
                    }
                    else
                    {
                        if (i.Expressions.Count > 0)
                        {
                            var newvar = ILProgram.Last().LValue;
                            if (i.Expressions[0] is ASTConstant cst)
                            {
                                ILProgram.Last().InjectConstant(i.Expressions[0] as ASTConstant, 0);
                            }
                            else
                            {
                                i.Expressions[0].ILGenerate(ILProgram, null);
                                var qt = new QuadTuple(ILOperator.Assign, ILProgram.Last().LValue, null, newvar);
                                ILProgram.Add(qt);
                            }
                        }
                    }
                }
            }
        }
        public partial class ASTIdentifier : ASTExpression
        {
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                var id = Value;
                if (LST.ContainsKey(id))
                {
                    //the name is not a var or array
                    if ((LST[id] is not STVariableItem) && (LST[id] is not STArrayItem))
                    {
                        Console.WriteLine(SemanticErrors.VCE0006, id);
                        Semantica.HasError = 1;
                    }
                }
                else
                {
                    if (GST.ContainsKey(id))
                    {
                        //the name is not a var or array
                        if ((GST[id] is not STVariableItem) && (GST[id] is not STArrayItem))
                        {
                            Console.WriteLine(SemanticErrors.VCE0006, id);
                            Semantica.HasError = 1;
                        }
                    }
                    else
                    {
                        bool defined = false;
                        foreach (var i in earg.Loops)
                        {
                            if (i.LPT.ContainsKey(id))
                            {
                                defined = true;
                                break;
                            }
                        }
                        if (!defined)
                        {
                            Console.WriteLine(SemanticErrors.VCE0002, id);
                            Semantica.HasError = 1;
                        }
                    }
                }
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                var tmpcopy = new QuadTuple(ILOperator.Assign,
                    new ILIdentifier(Value, ILNameType.Var, null), null,
                    new ILIdentifier("@Tmp" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, null));
                ++ILGenerator.TmpCounter;
                ILProgram.Add(tmpcopy);
            }
        }
        public partial class ASTArrayAccess : ASTExpression
        {
            [JsonIgnore]
            private string VType { get; set; }
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                var id = (ArrayName as ASTIdentifier).Value;
                //local array
                if (LST is not null && LST.ContainsKey(id))
                {
                    VType = (LST[id] as STArrayItem).ValueType;
                }
                else
                {
                    //global array
                    if (GST.ContainsKey(id))
                    {
                        VType = (GST[id] as STArrayItem).ValueType;
                    }
                    //not defined
                    else
                    {
                        bool defined = false;
                        foreach (var i in earg.Loops)
                        {
                            if (i.LPT.ContainsKey(id))
                            {
                                defined = true;
                                VType = (i.LPT[id] as STArrayItem).ValueType;
                                break;
                            }
                        }
                        if (!defined)
                        {
                            Console.WriteLine(SemanticErrors.VCE0002, id);
                            Semantica.HasError = 1;
                        }
                    }
                }
                foreach (var i in Elements)
                {
                    i.AOTCheck(GST, LST, earg);
                }
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                var ArrName = (ArrayName as ASTIdentifier).Value;
                var dim1 = new ILIdentifier();
                if (Elements[0] is ASTIntegerConstant ic)
                {
                    dim1.ILNameType = ILNameType.Constant;
                    dim1.ValueType = "int";
                    dim1.ID = ic.Value.ToString();
                }
                else if (Elements[0] is ASTIdentifier id)
                {
                    dim1.ILNameType = ILNameType.Var;
                    dim1.ID = id.Value;
                }
                else
                {
                    Elements[0].ILGenerate(ILProgram, null);
                    dim1 = ILProgram.Last().LValue;
                }
                //2-D array
                if (Elements.Count > 1)
                {
                    var dim2 = new ILIdentifier();

                    if (Elements[1] is ASTIntegerConstant ic2)
                    {
                        dim2.ILNameType = ILNameType.Constant;
                        dim2.ValueType = "int";
                        dim2.ID = ic2.Value.ToString();
                    }
                    else if (Elements[1] is ASTIdentifier id2)
                    {
                        dim2.ILNameType = ILNameType.Var;
                        dim2.ID = id2.Value;
                    }
                    else
                    {
                        Elements[1].ILGenerate(ILProgram, null);
                        dim2 = ILProgram.Last().LValue;
                    }
                    ILProgram.Add(new QuadTuple(ILOperator.Add, dim1, dim2,
                        new ILIdentifier("@Tmp" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, "int")));
                    ++ILGenerator.TmpCounter;
                    ILProgram.Add(new QuadTuple(ILOperator.ArrayAccess,
                        new ILIdentifier(ArrName, ILNameType.Var, VType), ILProgram.Last().LValue,
                        new ILIdentifier("@Tmp" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, "addr")));
                    ++ILGenerator.TmpCounter;
                }
                else
                {
                    ILProgram.Add(new QuadTuple(ILOperator.ArrayAccess,
                        new ILIdentifier(ArrName, ILNameType.Var, VType), dim1,
                        new ILIdentifier("@Tmp" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, "addr")));
                    ++ILGenerator.TmpCounter;
                }
            }
        }
        public partial class ASTBinaryExpression : ASTExpression
        {
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                Expr1.AOTCheck(GST, LST, earg);
                Expr2.AOTCheck(GST, LST, earg);
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                var op = Operator.Value;
                var expr = new QuadTuple();
                if (op == "+=" || op == "-=" || op == "*=" || op == "/=")
                {
                    expr.Operator = ILOperator.Assign;
                    expr.LValue = new ILIdentifier("@Tmp" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, null);
                    if (Expr1 is ASTIdentifier id)
                    {
                        expr.RValueB = new ILIdentifier(id.Value, ILNameType.Var, null);
                    }
                    else if (Expr1 is ASTArrayAccess aa)
                    {
                        Expr1.ILGenerate(ILProgram, null);
                        expr.RValueB = ILProgram.Last().LValue;
                    }
                    if (Expr2 is ASTIdentifier id2)
                    {
                        var rvalue = id2.Value;
                        expr.RValueA = new ILIdentifier(rvalue, ILNameType.Var, null);
                    }
                    else if (Expr2 is ASTConstant cc)
                    {
                        expr.InjectConstant(cc, 0);
                    }
                    else
                    {
                        Expr2.ILGenerate(ILProgram, null);
                        expr.RValueA = ILProgram.Last().LValue;
                    }
                    ILProgram.Add(expr);
                    if (Expr1 is ASTIdentifier)
                    {
                        ILProgram.Add(new QuadTuple(ILOperator.Assign, ILProgram.Last().LValue, null, expr.RValueB));
                    }
                    else if (Expr1 is ASTArrayAccess)
                    {
                        ILProgram.Add(new QuadTuple(ILOperator.ArrayAssign, ILProgram.Last().LValue, null, expr.RValueB));
                    }
                    ILProgram.MergePostfix();
                }
                else if (op == "=")
                {
                    if (Expr2 is ASTIdentifier id2)
                    {
                        var rvalue = id2.Value;
                        expr.RValueA = new ILIdentifier(rvalue, ILNameType.Var, null);
                    }
                    else if (Expr2 is ASTConstant cc)
                    {
                        expr.InjectConstant(cc, 0);
                    }
                    else
                    {
                        Expr2.ILGenerate(ILProgram, null);
                        expr.RValueA = ILProgram.Last().LValue;
                    }
                    if (Expr1 is ASTIdentifier id)
                    {
                        expr.Operator = ILOperator.Assign;
                        expr.LValue = new ILIdentifier(id.Value, ILNameType.Var, null);
                    }
                    else if (Expr1 is ASTArrayAccess aa)
                    {
                        Expr1.ILGenerate(ILProgram, null);
                        ILProgram.Last().Operator = ILOperator.LoadAddress;
                        ILProgram.Last().LValue.ValueType = "addr";
                        ILProgram.MergePostfix();
                        expr.Operator = ILOperator.Assign;
                        expr.LValue = ILProgram.Last().LValue;
                    }
                    ILProgram.Add(expr);
                    ILProgram.MergePostfix();
                }
                else
                {
                    if (op == "+")
                    {
                        expr.Operator = ILOperator.Add;
                    }
                    else if (op == "-")
                    {
                        expr.Operator = ILOperator.Subtract;
                    }
                    else if (op == "*")
                    {
                        expr.Operator = ILOperator.Multiply;
                    }
                    else if (op == "/")
                    {
                        expr.Operator = ILOperator.Division;
                    }
                    else if (op == "%")
                    {
                        expr.Operator = ILOperator.Module;
                    }
                    else if (op == ">")
                    {
                        expr.Operator = ILOperator.Jle;
                    }
                    else if (op == "<")
                    {
                        expr.Operator = ILOperator.Jge;
                    }
                    else if (op == "==")
                    {
                        expr.Operator = ILOperator.Jne;
                    }
                    else if (op == ">=")
                    {
                        expr.Operator = ILOperator.Jl;
                    }
                    else if (op == "<=")
                    {
                        expr.Operator = ILOperator.Jg;
                    }
                    if (Expr2 is ASTIdentifier id2)
                    {
                        var rvalue = id2.Value;
                        expr.RValueB = new ILIdentifier(rvalue, ILNameType.Var, null);
                    }
                    else if (Expr2 is ASTConstant cc)
                    {
                        expr.InjectConstant(cc, 1);
                    }
                    else
                    {
                        Expr2.ILGenerate(ILProgram, null);
                        expr.RValueB = ILProgram.Last().LValue;
                    }
                    if (Expr1 is ASTIdentifier id)
                    {
                        expr.RValueA = new ILIdentifier(id.Value, ILNameType.Var, null);
                    }
                    else if (Expr1 is ASTArrayAccess aa)
                    {
                        Expr1.ILGenerate(ILProgram, null);
                        expr.RValueA = ILProgram.Last().LValue;
                    }
                    else if (Expr1 is ASTConstant cc)
                    {
                        expr.InjectConstant(cc, 0);
                    }
                    expr.LValue = new ILIdentifier("@Tmp" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, null);
                    ++ILGenerator.TmpCounter;
                    ILProgram.Add(expr);
                }
            }
        }
        public partial class ASTFunctionCall : ASTExpression
        {
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                var fname = (FunctionName as ASTIdentifier).Value;
                if (fname != "printf" && fname != "scanf")
                {
                    //func not defined
                    if (!GST.ContainsKey(fname))
                    {
                        Console.WriteLine(SemanticErrors.VCE0004, fname);
                        Semantica.HasError = 1;
                    }
                    else
                    {
                        //the name is not a function
                        if (GST[fname] is not STFunctionItem)
                        {
                            Console.WriteLine(SemanticErrors.VCE0005, fname);
                            Semantica.HasError = 1;
                        }
                    }
                }
                foreach (var i in ArgList)
                {
                    i.AOTCheck(GST, LST, earg);
                }
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                foreach (var i in ArgList)
                {
                    if (i is ASTIdentifier id)
                    {
                        ILProgram.Add(new QuadTuple(ILOperator.Push, null, null,
                            new ILIdentifier(id.Value, ILNameType.Var, null)));
                    }
                    else if (i is ASTConstant c)
                    {
                        var cst = new QuadTuple(ILOperator.Push, null, null, null);
                        if (c is ASTStringConstant sc)
                        {
                            ILProgram.Insert(1, new QuadTuple(ILOperator.VarDefine,
                                new ILIdentifier(sc.Value, ILNameType.Constant, "string"), null,
                                new ILIdentifier("@Tmp" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, "string")));
                            ++ILGenerator.TmpCounter;
                            cst.InjectConstant(sc, 2, ILProgram[1].LValue);
                        }
                        else
                        {
                            cst.InjectConstant(c, 2);
                        }
                        ILProgram.Add(cst);
                    }
                    else
                    {
                        i.ILGenerate(ILProgram, null);
                        ILProgram.Add(new QuadTuple(ILOperator.Push, null, null,
                                ILProgram.Last().LValue));
                    }
                }
                var fname = (FunctionName as ASTIdentifier).Value;
                ILProgram.Add(new QuadTuple(ILOperator.Call, new ILIdentifier(fname, ILNameType.Function, null), null,
                    new ILIdentifier("@Callback" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, null)));
                ++ILGenerator.TmpCounter;
                ILProgram.MergePostfix();
            }
        }
        public partial class ASTPostfixExpression : ASTExpression
        {
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                if (Expression is ASTIdentifier ie)
                {
                    ie.AOTCheck(GST, LST, earg);
                }
                else if (Expression is ASTArrayAccess aa)
                {
                    aa.AOTCheck(GST, LST, earg);
                }
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                var ocopy = new ILIdentifier("@Tmp" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, null);
                ++ILGenerator.TmpCounter;
                if (Operator.Value == "--")
                {
                    if (Expression is ASTArrayAccess aa)
                    {
                        aa.ILGenerate(ILProgram, null);
                        ILGenerator.PostfixCache.Add(new QuadTuple(ILOperator.Decrease, null, null, ILProgram.Last().LValue));
                    }
                    else if (Expression is ASTIdentifier id)
                    {
                        var original = new ILIdentifier(id.Value, ILNameType.Var, null);
                        ILProgram.Add(new QuadTuple(ILOperator.Assign, original, null, ocopy));
                        ILGenerator.PostfixCache.Add(new QuadTuple(ILOperator.Decrease, null, null, original));
                    }
                }
                else if (Operator.Value == "++")
                {
                    if (Expression is ASTArrayAccess aa)
                    {
                        aa.ILGenerate(ILProgram, null);
                        ILGenerator.PostfixCache.Add(new QuadTuple(ILOperator.Increase, null, null, ILProgram.Last().LValue));
                    }
                    else if (Expression is ASTIdentifier id)
                    {
                        var original = new ILIdentifier(id.Value, ILNameType.Var, null);
                        ILProgram.Add(new QuadTuple(ILOperator.Assign, original, null, ocopy));
                        ILGenerator.PostfixCache.Add(new QuadTuple(ILOperator.Increase, null, null, original));
                    }
                }
            }
        }
        public partial class ASTUnaryExpression : ASTExpression
        {
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                if (Expression is ASTIdentifier ie)
                {
                    ie.AOTCheck(GST, LST, earg);
                }
                else if (Expression is ASTArrayAccess aa)
                {
                    aa.AOTCheck(GST, LST, earg);
                }
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                var ocopy = new ILIdentifier("@Tmp" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, null);
                ++ILGenerator.TmpCounter;
                if (Operator.Value == "!")
                {
                    if (Expression is ASTArrayAccess aa)
                    {
                        aa.ILGenerate(ILProgram, null);
                        ILProgram.Add(new QuadTuple(ILOperator.Jnz, ILProgram.Last().LValue, null,
                            new ILIdentifier("@Tmp" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, null)));
                        ++ILGenerator.TmpCounter;
                    }
                    else if (Expression is ASTIdentifier id)
                    {
                        var original = new ILIdentifier(id.Value, ILNameType.Var, null);
                        ILProgram.Add(new QuadTuple(ILOperator.Jnz, original, null,
                            new ILIdentifier("@Tmp" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, null)));
                        ++ILGenerator.TmpCounter;
                    }
                }
                else if (Operator.Value == "--")
                {
                    if (Expression is ASTArrayAccess aa)
                    {
                        aa.ILGenerate(ILProgram, null);
                        ILProgram.Add(new QuadTuple(ILOperator.Decrease, null, null, ILProgram.Last().LValue));
                    }
                    else if (Expression is ASTIdentifier id)
                    {
                        var original = new ILIdentifier(id.Value, ILNameType.Var, null);
                        ILProgram.Add(new QuadTuple(ILOperator.Decrease, null, null, original));
                    }
                }
                else if (Operator.Value == "++")
                {
                    if (Expression is ASTArrayAccess aa)
                    {
                        aa.ILGenerate(ILProgram, null);
                        ILProgram.Add(new QuadTuple(ILOperator.Increase, null, null, ILProgram.Last().LValue));
                    }
                    else if (Expression is ASTIdentifier id)
                    {
                        var original = new ILIdentifier(id.Value, ILNameType.Var, null);
                        ILProgram.Add(new QuadTuple(ILOperator.Increase, null, null, original));
                    }
                }
                else if (Operator.Value == "&")
                {
                    if (Expression is ASTArrayAccess aa)
                    {
                        aa.ILGenerate(ILProgram, null);
                        ILProgram.Last().Operator = ILOperator.LoadAddress;
                        ILProgram.MergePostfix();
                    }
                    else if (Expression is ASTIdentifier id)
                    {
                        var original = new ILIdentifier(id.Value, ILNameType.Var, null);
                        ILProgram.Add(new QuadTuple(ILOperator.LoadAddress, original, null,
                            new ILIdentifier("@Tmp" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, "addr")));
                        ++ILGenerator.TmpCounter;
                        ILProgram.MergePostfix();
                    }
                }
            }
        }
        public partial class ASTBreakStatement : ASTStatement
        {
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                if (!earg.isInLoop)
                {
                    Console.WriteLine(SemanticErrors.VCE0003);
                    Semantica.HasError = 1;
                }
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                ILProgram.Add(new QuadTuple(ILOperator.Jmp, null, null, ILGenerator.LoopBreakStack.Peek().LValue));
            }
        }
        public partial class ASTContinueStatement : ASTStatement
        {
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                if (!earg.isInLoop)
                {
                    Console.WriteLine(SemanticErrors.VCE0007);
                    Semantica.HasError = 1;
                }
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                ILProgram.Add(new QuadTuple(ILOperator.Jmp, null, null, ILGenerator.LoopContinueStack.Peek().LValue));
            }
        }
        public partial class ASTReturnStatement : ASTStatement
        {
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                if (Expression is not null)
                {
                    if (Expression[0] is ASTIdentifier id)
                    {
                        ILProgram.Add(new QuadTuple(ILOperator.Return, null, null,
                            new ILIdentifier(id.Value, ILNameType.Var, null)));
                    }
                    else if (Expression[0] is ASTConstant cst)
                    {
                        var ret = new QuadTuple(ILOperator.Return, null, null,
                            null);
                        ret.InjectConstant(cst, 2);
                        ILProgram.Add(ret);
                    }
                    else
                    {
                        Expression[0].ILGenerate(ILProgram, null);
                        ILProgram.Add(new QuadTuple(ILOperator.Return, null, null, ILProgram.Last().LValue));
                    }
                }
                else
                {
                    ILProgram.Add(new QuadTuple(ILOperator.Return, null, null, null));
                }
            }
        }
        public partial class ASTExpressionStatement : ASTStatement
        {
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                foreach (var i in Expressions)
                {
                    i.AOTCheck(GST, LST, earg);
                }
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                Expressions[0].ILGenerate(ILProgram, DeclarationType);
            }
        }
        public partial class ASTIterationDeclaredStatement : ASTStatement
        {
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                var lpST = new STLoopItem()
                {
                    LPT = new Dictionary<string, STItem>()
                };
                earg.Loops.AddLast(lpST);
                Initilize?.AOTCheck(GST, LST, earg);
                Condition?[0].AOTCheck(GST, LST, earg);
                Step?[0].AOTCheck(GST, LST, earg);
                earg.isInLoop = true;
                foreach (var i in (Stat as ASTCompoundStatement)?.BlockItems)
                {
                    i.AOTCheck(GST, LST, earg);
                }
                if (earg.Loops.Count <= 1) earg.isInLoop = false;
                earg.Loops.RemoveLast();
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                var jmpCond = new QuadTuple(ILOperator.JmpTarget, null, null,
                    new ILIdentifier("@Cond" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, null));
                ++ILGenerator.TmpCounter;
                var jmpEnd = new QuadTuple(ILOperator.JmpTarget, null, null,
                    new ILIdentifier("@Cond" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, null));
                ILGenerator.LoopBreakStack.Push(jmpEnd);
                ++ILGenerator.TmpCounter;
                var jmpStep = new QuadTuple(ILOperator.JmpTarget, null, null,
                    new ILIdentifier("@Cond" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, null));
                ++ILGenerator.TmpCounter;
                ILGenerator.LoopContinueStack.Push(jmpStep);
                if (Initilize is not null)
                {
                    Initilize.ILGenerate(ILProgram, "loop");
                }
                ILProgram.Add(new QuadTuple(ILOperator.Jmp, null, null, jmpCond.LValue));
                ILProgram.Add(jmpStep);
                if (Step is not null)
                {
                    Step[0].ILGenerate(ILProgram, null);
                    ILProgram.MergePostfix();
                }
                ILProgram.Add(jmpCond);
                if (Condition is not null)
                {
                    if (Condition[0] is ASTBinaryExpression be)
                    {
                        be.ILGenerate(ILProgram, null);
                        ILProgram.Last().LValue = jmpEnd.LValue;
                    }
                    else
                    {
                        Condition[0].ILGenerate(ILProgram, null);
                        ILProgram.Last().LValue = jmpEnd.LValue;
                    }
                }
                var body = (Stat as ASTCompoundStatement).BlockItems;
                foreach (var i in body)
                {
                    i.ILGenerate(ILProgram, "loop");
                }
                ILProgram.Add(new QuadTuple(ILOperator.Jmp, null, null, jmpStep.LValue));
                ILProgram.Add(jmpEnd);
                ILGenerator.LoopBreakStack.Pop();
                ILGenerator.LoopContinueStack.Pop();
            }
        }
        public partial class ASTIterationStatement : ASTStatement
        {
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                var lpST = new STLoopItem()
                {
                    LPT = new Dictionary<string, STItem>()
                };
                earg.Loops.AddLast(lpST);
                Initilize?[0].AOTCheck(GST, LST, earg);
                Condition?[0].AOTCheck(GST, LST, earg);
                Step?[0].AOTCheck(GST, LST, earg);
                earg.isInLoop = true;
                foreach (var i in (Stat as ASTCompoundStatement)?.BlockItems)
                {
                    i.AOTCheck(GST, LST, earg);
                }
                if (earg.Loops.Count <= 1) earg.isInLoop = false;
                earg.Loops.RemoveLast();
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                var jmpCond = new QuadTuple(ILOperator.JmpTarget, null, null,
                    new ILIdentifier("@Cond" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, null));
                ++ILGenerator.TmpCounter;
                var jmpEnd = new QuadTuple(ILOperator.JmpTarget, null, null,
                    new ILIdentifier("@Cond" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, null));
                ILGenerator.LoopBreakStack.Push(jmpEnd);
                ++ILGenerator.TmpCounter;
                var jmpStep = new QuadTuple(ILOperator.JmpTarget, null, null,
                    new ILIdentifier("@Cond" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, null));
                ++ILGenerator.TmpCounter;
                ILGenerator.LoopContinueStack.Push(jmpStep);
                if (Initilize is not null)
                {
                    Initilize[0].ILGenerate(ILProgram, "loop");
                }
                ILProgram.Add(new QuadTuple(ILOperator.Jmp, null, null, jmpCond.LValue));
                ILProgram.Add(jmpStep);
                if (Step is not null)
                {
                    Step[0].ILGenerate(ILProgram, null);
                    ILProgram.MergePostfix();
                }
                ILProgram.Add(jmpCond);
                if (Condition is not null)
                {
                    if (Condition[0] is ASTBinaryExpression be)
                    {
                        be.ILGenerate(ILProgram, null);
                        ILProgram.Last().LValue = jmpEnd.LValue;
                    }
                    else
                    {
                        Condition[0].ILGenerate(ILProgram, null);
                        ILProgram.Last().LValue = jmpEnd.LValue;
                    }
                }
                var body = (Stat as ASTCompoundStatement).BlockItems;
                foreach (var i in body)
                {
                    i.ILGenerate(ILProgram, "loop");
                }
                ILProgram.Add(new QuadTuple(ILOperator.Jmp, null, null, jmpStep.LValue));
                ILProgram.Add(jmpEnd);
                ILGenerator.LoopBreakStack.Pop();
                ILGenerator.LoopContinueStack.Pop();
            }
        }
        public partial class ASTSelectionStatement : ASTStatement
        {
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                Condition[0].AOTCheck(GST, LST, earg);
                Then?.AOTCheck(GST, LST, earg);
                Otherwise?.AOTCheck(GST, LST, earg);
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                var jmpEnd = new QuadTuple(ILOperator.JmpTarget, null, null, new ILIdentifier("Select" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, null));
                ++ILGenerator.TmpCounter;
                var jmpOther = new QuadTuple(ILOperator.JmpTarget, null, null, new ILIdentifier("Select" + ILGenerator.TmpCounter.ToString(), ILNameType.TmpVar, null));
                ++ILGenerator.TmpCounter;
                if (Condition is not null)
                {
                    if (Condition[0] is ASTBinaryExpression be)
                    {
                        be.ILGenerate(ILProgram, null);
                        ILProgram.Last().LValue = jmpOther.LValue;
                    }
                    else
                    {
                        if (Condition[0] is ASTIdentifier id)
                        {
                            ILProgram.Add(new QuadTuple(ILOperator.Je,
                                new ILIdentifier("0", ILNameType.Constant, "int"),
                                new ILIdentifier(id.Value, ILNameType.Var, null), jmpEnd.LValue));
                        }
                        else
                        {
                            Condition[0].ILGenerate(ILProgram, null);
                            ILProgram.Add(new QuadTuple(ILOperator.Je,
                                new ILIdentifier("0", ILNameType.Constant, "int"), ILProgram.Last().LValue, jmpEnd.LValue));
                        }
                    }
                }
                Then?.ILGenerate(ILProgram, "selection");
                ILProgram.Add(new QuadTuple(ILOperator.Jmp, null, null, jmpEnd.LValue));
                ILProgram.Add(jmpOther);
                Otherwise?.ILGenerate(ILProgram, "selection");
                ILProgram.Add(jmpEnd);
            }
        }
        public partial class ASTCompoundStatement : ASTStatement
        {
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                foreach (var i in BlockItems)
                {
                    i.AOTCheck(GST, LST, earg);
                }
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                foreach (var i in BlockItems)
                {
                    i.ILGenerate(ILProgram, null);
                    ILProgram.MergePostfix();
                }
            }
        }
        public partial class ASTArrayDeclarator : ASTDeclarator
        {
            [JsonIgnore]
            private string VType { get; set; }
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                VType = earg.VType;
                if (Declarator is ASTArrayDeclarator aDecl)
                {
                    earg.MultiDimArray = new Stack<int>();
                    earg.MultiDimArray.Push((Expression as ASTIntegerConstant).Value);
                    aDecl.AOTCheck(GST, LST, earg);
                }
                else
                {
                    var arrName = (Declarator as ASTVariableDeclarator).Identifier.Value;
                    earg.MultiDimArray.Push((Expression as ASTIntegerConstant).Value);
                    if (earg.Loops.Count > 0)
                    {
                        if (earg.Loops.Last.Value.LPT.ContainsKey(arrName))
                        {
                            Console.WriteLine(SemanticErrors.VCE0001, arrName);
                            Semantica.HasError = 1;
                        }
                        else
                        {
                            var dims = new List<int>();
                            while (earg.MultiDimArray.Count > 0)
                            {
                                dims.Add(earg.MultiDimArray.Pop());
                            }
                            earg.Loops.Last.Value.LPT.Add(arrName, new STArrayItem(arrName, dims, earg.VType));
                        }
                    }
                    else
                    {
                        if (LST is not null)
                        {
                            if (LST.ContainsKey(arrName))
                            {
                                Console.WriteLine(SemanticErrors.VCE0001, arrName);
                                Semantica.HasError = 1;
                            }
                            else
                            {
                                var dims = new List<int>();
                                while (earg.MultiDimArray.Count > 0)
                                {
                                    dims.Add(earg.MultiDimArray.Pop());
                                }
                                LST.Add(arrName, new STArrayItem(arrName, dims, earg.VType));
                            }
                        }
                        else
                        {
                            if (GST.ContainsKey(arrName))
                            {
                                Console.WriteLine(SemanticErrors.VCE0001, arrName);
                                Semantica.HasError = 1;
                            }
                            else
                            {
                                var dims = new List<int>();
                                while (earg.MultiDimArray.Count > 0)
                                {
                                    dims.Add(earg.MultiDimArray.Pop());
                                }
                                GST.Add(arrName, new STArrayItem(arrName, dims, earg.VType));
                            }
                        }
                    }
                }
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                var len = (Expression as ASTIntegerConstant).Value;
                var name = "";
                if (Declarator is ASTArrayDeclarator ad)
                {
                    len *= (ad.Expression as ASTIntegerConstant).Value;
                    name = (ad.Declarator as ASTVariableDeclarator).Identifier.Value;
                }
                else
                {
                    name = (Declarator as ASTVariableDeclarator).Identifier.Value;
                }
                if (DeclarationType == "global")
                {
                    ILProgram.Insert(1, new QuadTuple(ILOperator.ArrayDefine,
                        new ILIdentifier(len.ToString(), ILNameType.Constant, "int"), null,
                        new ILIdentifier(name, ILNameType.Array, VType)));
                }
                else
                {
                    ILProgram.Add(new QuadTuple(ILOperator.ArrayDefine,
                        new ILIdentifier(len.ToString(), ILNameType.Constant, "int"), null,
                        new ILIdentifier(name, ILNameType.Array, VType)));
                }
            }
        }
        public partial class ASTVariableDeclarator : ASTDeclarator
        {
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                var id = Identifier.Value;
                if (earg.Loops.Count > 0)
                {
                    if (earg.Loops.Last.Value.LPT.ContainsKey(id))
                    {
                        Console.WriteLine(SemanticErrors.VCE0001, id);
                        Semantica.HasError = 1;
                    }
                    else
                    {
                        earg.Loops.Last.Value.LPT.Add(id, new STVariableItem(id, earg.VType));
                    }
                }
                else
                {
                    if (LST is not null)
                    {
                        if (LST.ContainsKey(id))
                        {
                            Console.WriteLine(SemanticErrors.VCE0001, id);
                            Semantica.HasError = 1;
                        }
                        else
                        {
                            LST.Add(id, new STVariableItem(id, earg.VType));
                        }
                    }
                    else
                    {
                        if (GST.ContainsKey(id))
                        {
                            Console.WriteLine(SemanticErrors.VCE0001, id);
                            Semantica.HasError = 1;
                        }
                        else
                        {
                            GST.Add(id, new STVariableItem(id, earg.VType));
                        }
                    }
                }
                return 0;
            }
            public override void ILGenerate(List<QuadTuple> ILProgram, string DeclarationType)
            {
                var name = Identifier.Value;
                var qt = new QuadTuple(ILOperator.VarDefine, null, null,
                    new ILIdentifier(name, ILNameType.Var, DeclarationType));
                if (DeclarationType == "global")
                {
                    ILProgram.Insert(1, qt);
                }
                else
                {
                    ILProgram.Add(qt);
                }
            }
        }
        public partial class ASTFunctionDeclarator : ASTDeclarator
        {
            //global func define
            public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
            {
                var fName = (Declarator as ASTVariableDeclarator).Identifier.Value;
                var fDef = new STFunctionItem(fName, earg.VType, null, null);
                return 0;
            }
        }
    }

}