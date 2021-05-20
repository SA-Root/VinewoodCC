using System;
using System.Collections.Generic;

namespace VinewoodCC
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
        public virtual void ILGenerate(LinkedList<QuadTuple> ILProgram, Dictionary<string, int> ZipBackTable)
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
        public override void ILGenerate(LinkedList<QuadTuple> ILProgram, Dictionary<string, int> ZipBackTable)
        {
            ILProgram.AddLast(new QuadTuple(ILOperator.DataBegin, null, null, null));
            ILProgram.AddLast(new QuadTuple(ILOperator.DataEnd, null, null, null));
            foreach (var i in Items)
            {
                i.ILGenerate(ILProgram, null);
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
        public override void ILGenerate(LinkedList<QuadTuple> ILProgram, Dictionary<string, int> ZipBackTable)
        {
            ZipBackTable = new Dictionary<string, int>();
            var funcName = ((Declarator as ASTFunctionDeclarator).Declarator as ASTVariableDeclarator).Identifier.Value;
            var fHead = new ILIdentifier(funcName, ILNameType.Function, null);
            ILProgram.AddLast(new QuadTuple(ILOperator.ProcBegin, null, null, fHead));
            //params

            var fStart = ILProgram.Count - 1;
            //body

            ILProgram.ZipBack(ZipBackTable, fStart);
            ILProgram.AddLast(new QuadTuple(ILOperator.ProcEnd, null, null, fHead));
        }
    }
    public partial class ASTDeclaration : ASTNode
    {
        public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
        {
            earg.VType = Specifiers[0].Value;
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
    }
    public partial class ASTArrayAccess : ASTExpression
    {
        public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
        {
            var id = (ArrayName as ASTIdentifier).Value;
            //local array
            if (LST != null && LST.ContainsKey(id))
            {

            }
            else
            {
                //global array
                if (GST.ContainsKey(id))
                {

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
    }
    public partial class ASTBinaryExpression : ASTExpression
    {
        public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
        {
            Expr1.AOTCheck(GST, LST, earg);
            Expr2.AOTCheck(GST, LST, earg);
            return 0;
        }
    }
    public partial class ASTFunctionCall : ASTExpression
    {
        public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
        {
            var fname = (FunctionName as ASTIdentifier).Value;
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
            return 0;
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
    }
    public partial class ASTArrayDeclarator : ASTDeclarator
    {
        public override int AOTCheck(Dictionary<string, STItem> GST, Dictionary<string, STItem> LST, AOTCheckExtraArg earg)
        {
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
                        earg.Loops.Last.Value.LPT.Add(arrName, new STArrayItem(arrName, dims));
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
                            LST.Add(arrName, new STArrayItem(arrName, dims));
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
                            GST.Add(arrName, new STArrayItem(arrName, dims));
                            Semantica.HasError = 1;
                        }
                    }
                }
            }
            return 0;
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