using System.Collections.Generic;
using VinewoodCC.Semantic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace VinewoodCC
{
    namespace AST
    {
        [JsonDerivedType(typeof(ASTCompilationUnit), "Program")]
        [JsonDerivedType(typeof(ASTFunctionDefine), "FunctionDefine")]
        [JsonDerivedType(typeof(ASTDeclaration), "Declaration")]
        [JsonDerivedType(typeof(ASTToken), "Token")]
        [JsonDerivedType(typeof(ASTTypename), "Typename")]
        [JsonDerivedType(typeof(ASTParamsDeclarator), "ParamsDeclarator")]
        [JsonDerivedType(typeof(ASTInitList), "InitList")]
        [JsonDerivedType(typeof(ASTIdentifier), "Identifier")]
        [JsonDerivedType(typeof(ASTArrayAccess), "ArrayAccess")]
        [JsonDerivedType(typeof(ASTBinaryExpression), "BinaryExpression")]
        [JsonDerivedType(typeof(ASTFunctionCall), "FunctionCall")]
        [JsonDerivedType(typeof(ASTPostfixExpression), "PostfixExpression")]
        [JsonDerivedType(typeof(ASTUnaryExpression), "UnaryExpression")]
        [JsonDerivedType(typeof(ASTCharConstant), "CharConstant")]
        [JsonDerivedType(typeof(ASTFloatConstant), "FloatConstant")]
        [JsonDerivedType(typeof(ASTStringConstant), "StringConstant")]
        [JsonDerivedType(typeof(ASTIntegerConstant), "IntegerConstant")]
        [JsonDerivedType(typeof(ASTBreakStatement), "BreakStatement")]
        [JsonDerivedType(typeof(ASTCompoundStatement), "CompoundStatement")]
        [JsonDerivedType(typeof(ASTContinueStatement), "ContinueStatement")]
        [JsonDerivedType(typeof(ASTExpressionStatement), "ExpressionStatement")]
        [JsonDerivedType(typeof(ASTIterationDeclaredStatement), "IterationDeclaredStatement")]
        [JsonDerivedType(typeof(ASTIterationStatement), "IterationStatement")]
        [JsonDerivedType(typeof(ASTReturnStatement), "ReturnStatement")]
        [JsonDerivedType(typeof(ASTSelectionStatement), "SelectionStatement")]
        [JsonDerivedType(typeof(ASTArrayDeclarator), "ArrayDeclarator")]
        [JsonDerivedType(typeof(ASTVariableDeclarator), "VariableDeclarator")]
        [JsonDerivedType(typeof(ASTFunctionDeclarator), "FunctionDeclarator")]
        public partial class ASTNode
        {
            public string Type { get; set; }
            public ASTNode(string type)
            {
                Type = type;
            }
        }

        [JsonSourceGenerationOptions(WriteIndented = true)]
        [JsonSerializable(typeof(ASTNode))]
        internal partial class SourceGenerationContext : JsonSerializerContext
        {
        }

        public partial class ASTCompilationUnit : ASTNode
        {
            public List<ASTNode> Items { get; set; }
            [JsonIgnore]
            public Dictionary<string, STItem> GlobalSymbolTable { get; set; }
            public ASTCompilationUnit() : base("Program")
            {
                Items = new List<ASTNode>();
            }
            public ASTCompilationUnit(List<ASTNode> items) : base("Program")
            {
                Items = items;
            }
        }
        [JsonDerivedType(typeof(ASTIdentifier), "Identifier")]
        [JsonDerivedType(typeof(ASTArrayAccess), "ArrayAccess")]
        [JsonDerivedType(typeof(ASTBinaryExpression), "BinaryExpression")]
        [JsonDerivedType(typeof(ASTFunctionCall), "FunctionCall")]
        [JsonDerivedType(typeof(ASTPostfixExpression), "PostfixExpression")]
        [JsonDerivedType(typeof(ASTUnaryExpression), "UnaryExpression")]
        [JsonDerivedType(typeof(ASTCharConstant), "CharConstant")]
        [JsonDerivedType(typeof(ASTFloatConstant), "FloatConstant")]
        [JsonDerivedType(typeof(ASTStringConstant), "StringConstant")]
        [JsonDerivedType(typeof(ASTIntegerConstant), "IntegerConstant")]
        public abstract partial class ASTExpression : ASTNode
        {
            public ASTExpression(string type) : base(type)
            {

            }
        }
        public abstract partial class ASTConstant : ASTExpression
        {
            public ASTConstant(string type) : base(type)
            {

            }
        }
        [JsonDerivedType(typeof(ASTBreakStatement), "BreakStatement")]
        [JsonDerivedType(typeof(ASTCompoundStatement), "CompoundStatement")]
        [JsonDerivedType(typeof(ASTContinueStatement), "ContinueStatement")]
        [JsonDerivedType(typeof(ASTExpressionStatement), "ExpressionStatement")]
        [JsonDerivedType(typeof(ASTIterationDeclaredStatement), "IterationDeclaredStatement")]
        [JsonDerivedType(typeof(ASTIterationStatement), "IterationStatement")]
        [JsonDerivedType(typeof(ASTReturnStatement), "ReturnStatement")]
        [JsonDerivedType(typeof(ASTSelectionStatement), "SelectionStatement")]
        public abstract partial class ASTStatement : ASTNode
        {
            public ASTStatement(string type) : base(type)
            {

            }
        }
        public partial class ASTFunctionDefine : ASTNode
        {
            public List<ASTToken> Specifiers { get; set; }
            public ASTDeclarator Declarator { get; set; }
            public ASTCompoundStatement Body { get; set; }
            [JsonIgnore]
            private Dictionary<string, STItem> LocalSymbolTable { get; set; }
            public ASTFunctionDefine() : base("FunctionDefine")
            {
                Specifiers = new List<ASTToken>();
            }
            public ASTFunctionDefine(List<ASTToken> specList, ASTDeclarator declarator, ASTCompoundStatement bodyStatement) : base("FunctionDefine")
            {
                Specifiers = specList;
                Declarator = declarator;
                Body = bodyStatement;
            }
        }
        public partial class ASTDeclaration : ASTNode
        {
            public List<ASTToken> Specifiers { get; set; }
            public List<ASTInitList> InitLists { get; set; }
            public ASTDeclaration() : base("Declaration")
            {
                Specifiers = new List<ASTToken>();
                InitLists = new List<ASTInitList>();
            }
            public ASTDeclaration(List<ASTToken> specList, List<ASTInitList> initList) : base("Declaration")
            {
                Specifiers = specList;
                InitLists = initList;
            }
        }
        public partial class ASTToken : ASTNode
        {
            public string Value { get; set; }
            public int TokenID { get; set; }
            public ASTToken() : base("Token")
            {

            }
            public ASTToken(string value, int tid) : base("Token")
            {
                Value = value;
                TokenID = tid;
            }
        }
        public partial class ASTTypename : ASTNode
        {
            public List<ASTToken> Specfiers { get; set; }
            public ASTDeclarator Declarator { get; set; }
            public ASTTypename() : base("Typename")
            {
                Specfiers = new List<ASTToken>();
            }
            public ASTTypename(List<ASTToken> specList, ASTDeclarator declarator) : base("Typename")
            {
                Specfiers = specList;
                Declarator = declarator;
            }
        }
        [JsonDerivedType(typeof(ASTArrayDeclarator), "ArrayDeclarator")]
        [JsonDerivedType(typeof(ASTVariableDeclarator), "VariableDeclarator")]
        [JsonDerivedType(typeof(ASTFunctionDeclarator), "FunctionDeclarator")]
        abstract public partial class ASTDeclarator : ASTNode
        {
            public ASTDeclarator(string type) : base(type)
            {

            }
        }
        public partial class ASTParamsDeclarator : ASTNode
        {
            public List<ASTToken> Specfiers { get; set; }
            public ASTDeclarator Declarator { get; set; }
            public ASTParamsDeclarator() : base("ParamsDeclarator")
            {
                Specfiers = new List<ASTToken>();
            }
            public ASTParamsDeclarator(List<ASTToken> specList, ASTDeclarator declarator) : base("ParamsDeclarator")
            {
                Specfiers = specList;
                Declarator = declarator;
            }
        }
        public partial class ASTInitList : ASTNode
        {
            public ASTDeclarator Declarator { get; set; }
            public List<ASTExpression> Expressions { get; set; }
            public ASTInitList() : base("InitList")
            {

            }
            public ASTInitList(ASTDeclarator d, List<ASTExpression> e) : base("InitList")
            {
                Declarator = d;
                Expressions = e;
            }
        }
        public partial class ASTIdentifier : ASTExpression
        {
            public string Value { get; set; }
            public int TokenID { get; set; }
            public ASTIdentifier() : base("Identifier")
            {

            }
            public ASTIdentifier(string value, int tid) : base("Identifier")
            {
                Value = value;
                TokenID = tid;
            }
        }
        public partial class ASTArrayAccess : ASTExpression
        {
            public ASTExpression ArrayName { get; set; }
            public List<ASTExpression> Elements { get; set; }
            public ASTArrayAccess() : base("ArrayAccess")
            {
                Elements = new List<ASTExpression>();
            }
            public ASTArrayAccess(ASTExpression arrayname, List<ASTExpression> elements) : base("ArrayAccess")
            {
                ArrayName = arrayname;
                Elements = elements;
            }
        }
        public partial class ASTBinaryExpression : ASTExpression
        {
            public ASTToken Operator { get; set; }
            public ASTExpression Expr1 { get; set; }
            public ASTExpression Expr2 { get; set; }
            public ASTBinaryExpression() : base("BinaryExpression")
            {

            }
            public ASTBinaryExpression(ASTToken op, ASTExpression e1, ASTExpression e2) : base("BinaryExpression")
            {
                Operator = op;
                Expr1 = e1;
                Expr2 = e2;
            }
        }
        public partial class ASTCharConstant : ASTConstant
        {
            public string Value { get; set; }
            public int TokenID { get; set; }
            public ASTCharConstant() : base("CharConstant")
            {

            }
            public ASTCharConstant(string value, int tid) : base("CharConstant")
            {
                Value = value;
                TokenID = tid;
            }
        }
        public partial class ASTFloatConstant : ASTConstant
        {
            public double Value { get; set; }
            public int TokenID { get; set; }
            public ASTFloatConstant() : base("FloatConstant")
            {

            }
            public ASTFloatConstant(double value, int tid) : base("FloatConstant")
            {
                Value = value;
                TokenID = tid;
            }
        }
        public partial class ASTFunctionCall : ASTExpression
        {
            public ASTExpression FunctionName { get; set; }
            public List<ASTNode> ArgList { get; set; }
            public ASTFunctionCall() : base("FunctionCall")
            {

            }
            public ASTFunctionCall(ASTExpression name, List<ASTNode> args) : base("FunctionCall")
            {
                FunctionName = name;
                ArgList = args;
            }
        }
        public partial class ASTIntegerConstant : ASTConstant
        {
            public int Value { get; set; }
            public int TokenID { get; set; }
            public ASTIntegerConstant() : base("IntegerConstant")
            {

            }
            public ASTIntegerConstant(int value, int tid) : base("IntegerConstant")
            {
                Value = value;
                TokenID = tid;
            }
        }
        public partial class ASTPostfixExpression : ASTExpression
        {
            public ASTExpression Expression { get; set; }
            public ASTToken Operator { get; set; }
            public ASTPostfixExpression() : base("PostfixExpression")
            {

            }
            public ASTPostfixExpression(ASTExpression expr, ASTToken op) : base("PostfixExpression")
            {
                Expression = expr;
                Operator = op;
            }
        }
        public partial class ASTStringConstant : ASTConstant
        {
            public string Value { get; set; }
            public int TokenID { get; set; }
            public ASTStringConstant() : base("StringConstant")
            {

            }
            public ASTStringConstant(string value, int tid) : base("StringConstant")
            {
                Value = value;
                TokenID = tid;
            }
        }
        public partial class ASTUnaryExpression : ASTExpression
        {
            public ASTToken Operator { get; set; }
            public ASTExpression Expression { get; set; }
            public ASTUnaryExpression() : base("UnaryExpression")
            {

            }
            public ASTUnaryExpression(ASTToken op, ASTExpression expr) : base("UnaryExpression")
            {
                Expression = expr;
                Operator = op;
            }
        }
        public partial class ASTBreakStatement : ASTStatement
        {
            public ASTBreakStatement() : base("BreakStatement")
            {

            }
        }
        public partial class ASTCompoundStatement : ASTStatement
        {
            public List<ASTNode> BlockItems { get; set; }
            public ASTCompoundStatement() : base("CompoundStatement")
            {
                BlockItems = new List<ASTNode>();
            }
            public ASTCompoundStatement(List<ASTNode> items) : base("CompoundStatement")
            {
                BlockItems = items;
            }
        }
        public partial class ASTContinueStatement : ASTStatement
        {
            public ASTContinueStatement() : base("ContinueStatement")
            {

            }
        }
        public partial class ASTExpressionStatement : ASTStatement
        {
            public List<ASTNode> Expressions { get; set; }
            public ASTExpressionStatement() : base("ExpressionStatement")
            {

            }
            public ASTExpressionStatement(List<ASTNode> exprs) : base("ExpressionStatement")
            {
                Expressions = exprs;
            }
        }
        public partial class ASTIterationDeclaredStatement : ASTStatement
        {
            public ASTDeclaration Initilize { get; set; }
            public List<ASTNode> Condition { get; set; }
            public List<ASTNode> Step { get; set; }
            public ASTStatement Stat { get; set; }
            public ASTIterationDeclaredStatement() : base("IterationDeclaredStatement")
            {

            }
            public ASTIterationDeclaredStatement(ASTDeclaration init,
                        List<ASTNode> cond,
                        List<ASTNode> step,
                        ASTStatement stat) : base("IterationDeclaredStatement")
            {
                Initilize = init;
                Condition = cond;
                Step = step;
                Stat = stat;
            }
        }
        public partial class ASTIterationStatement : ASTStatement
        {
            public List<ASTNode> Initilize { get; set; }
            public List<ASTNode> Condition { get; set; }
            public List<ASTNode> Step { get; set; }
            public ASTStatement Stat { get; set; }
            public ASTIterationStatement() : base("IterationStatement")
            {

            }
            public ASTIterationStatement(List<ASTNode> init,
                        List<ASTNode> cond,
                        List<ASTNode> step,
                        ASTStatement stat) : base("IterationStatement")
            {
                Initilize = init;
                Condition = cond;
                Step = step;
                Stat = stat;
            }
        }
        public partial class ASTReturnStatement : ASTStatement
        {
            public List<ASTNode> Expression { get; set; }
            public ASTReturnStatement() : base("ReturnStatement")
            {

            }
            public ASTReturnStatement(List<ASTNode> expr) : base("ReturnStatement")
            {
                Expression = expr;
            }
        }
        public partial class ASTSelectionStatement : ASTStatement
        {
            public List<ASTNode> Condition { get; set; }
            public ASTStatement Then { get; set; }
            public ASTStatement Otherwise { get; set; }
            public ASTSelectionStatement() : base("SelectionStatement")
            {

            }
            public ASTSelectionStatement(List<ASTNode> cond,
                        ASTStatement then,
                        ASTStatement otherwise) : base("SelectionStatement")
            {
                Condition = cond;
                Then = then;
                Otherwise = otherwise;
            }
        }
        public partial class ASTArrayDeclarator : ASTDeclarator
        {
            public ASTDeclarator Declarator { get; set; }
            public ASTExpression Expression { get; set; }
            public ASTArrayDeclarator() : base("ArrayDeclarator")
            {

            }
            public ASTArrayDeclarator(ASTDeclarator declarator, ASTExpression expressions) : base("ArrayDeclarator")
            {
                Declarator = declarator;
                Expression = expressions;
            }
        }
        public partial class ASTVariableDeclarator : ASTDeclarator
        {
            public ASTIdentifier Identifier { get; set; }
            public ASTVariableDeclarator() : base("VariableDeclarator")
            {

            }
            public ASTVariableDeclarator(ASTIdentifier declarator) : base("VariableDeclarator")
            {
                Identifier = declarator;
            }
        }
        public partial class ASTFunctionDeclarator : ASTDeclarator
        {
            public ASTDeclarator Declarator { get; set; }
            public List<ASTParamsDeclarator> Parameters { get; set; }
            public ASTFunctionDeclarator() : base("FunctionDeclarator")
            {

            }
            public ASTFunctionDeclarator(ASTDeclarator declarator,
                        List<ASTParamsDeclarator> paramsDecl) : base("FunctionDeclarator")
            {
                Declarator = declarator;
                Parameters = paramsDecl;
            }
        }
    }

}
