using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using JsonSubTypes;

namespace VinewoodCC
{
    namespace AST
    {
        [JsonConverter(typeof(JsonSubtypes), "type")]
        [JsonSubtypes.KnownSubType(typeof(ASTCompilationUnit), "Program")]
        [JsonSubtypes.KnownSubType(typeof(ASTFunctionDefine), "FunctionDefine")]
        [JsonSubtypes.KnownSubType(typeof(ASTDeclaration), "Declaration")]
        [JsonSubtypes.KnownSubType(typeof(ASTToken), "Token")]
        [JsonSubtypes.KnownSubType(typeof(ASTTypename), "Typename")]
        [JsonSubtypes.KnownSubType(typeof(ASTParamsDeclarator), "ParamsDeclarator")]
        [JsonSubtypes.KnownSubType(typeof(ASTInitList), "InitList")]
        [JsonSubtypes.KnownSubType(typeof(ASTIdentifier), "Identifier")]
        [JsonSubtypes.KnownSubType(typeof(ASTArrayAccess), "ArrayAccess")]
        [JsonSubtypes.KnownSubType(typeof(ASTBinaryExpression), "BinaryExpression")]
        [JsonSubtypes.KnownSubType(typeof(ASTFunctionCall), "FunctionCall")]
        [JsonSubtypes.KnownSubType(typeof(ASTPostfixExpression), "PostfixExpression")]
        [JsonSubtypes.KnownSubType(typeof(ASTUnaryExpression), "UnaryExpression")]
        [JsonSubtypes.KnownSubType(typeof(ASTCharConstant), "CharConstant")]
        [JsonSubtypes.KnownSubType(typeof(ASTFloatConstant), "FloatConstant")]
        [JsonSubtypes.KnownSubType(typeof(ASTStringConstant), "StringConstant")]
        [JsonSubtypes.KnownSubType(typeof(ASTIntegerConstant), "IntegerConstant")]
        [JsonSubtypes.KnownSubType(typeof(ASTBreakStatement), "BreakStatement")]
        [JsonSubtypes.KnownSubType(typeof(ASTCompoundStatement), "CompoundStatement")]
        [JsonSubtypes.KnownSubType(typeof(ASTContinueStatement), "ContinueStatement")]
        [JsonSubtypes.KnownSubType(typeof(ASTExpressionStatement), "ExpressionStatement")]
        [JsonSubtypes.KnownSubType(typeof(ASTIterationDeclaredStatement), "IterationDeclaredStatement")]
        [JsonSubtypes.KnownSubType(typeof(ASTIterationStatement), "IterationStatement")]
        [JsonSubtypes.KnownSubType(typeof(ASTReturnStatement), "ReturnStatement")]
        [JsonSubtypes.KnownSubType(typeof(ASTSelectionStatement), "SelectionStatement")]
        [JsonSubtypes.KnownSubType(typeof(ASTArrayDeclarator), "ArrayDeclarator")]
        [JsonSubtypes.KnownSubType(typeof(ASTVariableDeclarator), "VariableDeclarator")]
        [JsonSubtypes.KnownSubType(typeof(ASTFunctionDeclarator), "FunctionDeclarator")]
        public partial class ASTNode
        {
            [JsonProperty(Order = 1, PropertyName = "type")]
            public string Type { get; set; }
            public ASTNode(string type)
            {
                Type = type;
            }
        }
        public partial class ASTCompilationUnit : ASTNode
        {
            [JsonProperty(Order = 2, PropertyName = "items")]
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
        [JsonConverter(typeof(JsonSubtypes), "type")]
        [JsonSubtypes.KnownSubType(typeof(ASTIdentifier), "Identifier")]
        [JsonSubtypes.KnownSubType(typeof(ASTArrayAccess), "ArrayAccess")]
        [JsonSubtypes.KnownSubType(typeof(ASTBinaryExpression), "BinaryExpression")]
        [JsonSubtypes.KnownSubType(typeof(ASTFunctionCall), "FunctionCall")]
        [JsonSubtypes.KnownSubType(typeof(ASTPostfixExpression), "PostfixExpression")]
        [JsonSubtypes.KnownSubType(typeof(ASTUnaryExpression), "UnaryExpression")]
        [JsonSubtypes.KnownSubType(typeof(ASTCharConstant), "CharConstant")]
        [JsonSubtypes.KnownSubType(typeof(ASTFloatConstant), "FloatConstant")]
        [JsonSubtypes.KnownSubType(typeof(ASTStringConstant), "StringConstant")]
        [JsonSubtypes.KnownSubType(typeof(ASTIntegerConstant), "IntegerConstant")]
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
        [JsonConverter(typeof(JsonSubtypes), "type")]
        [JsonSubtypes.KnownSubType(typeof(ASTBreakStatement), "BreakStatement")]
        [JsonSubtypes.KnownSubType(typeof(ASTCompoundStatement), "CompoundStatement")]
        [JsonSubtypes.KnownSubType(typeof(ASTContinueStatement), "ContinueStatement")]
        [JsonSubtypes.KnownSubType(typeof(ASTExpressionStatement), "ExpressionStatement")]
        [JsonSubtypes.KnownSubType(typeof(ASTIterationDeclaredStatement), "IterationDeclaredStatement")]
        [JsonSubtypes.KnownSubType(typeof(ASTIterationStatement), "IterationStatement")]
        [JsonSubtypes.KnownSubType(typeof(ASTReturnStatement), "ReturnStatement")]
        [JsonSubtypes.KnownSubType(typeof(ASTSelectionStatement), "SelectionStatement")]
        public abstract partial class ASTStatement : ASTNode
        {
            public ASTStatement(string type) : base(type)
            {

            }
        }
        public partial class ASTFunctionDefine : ASTNode
        {
            [JsonProperty(Order = 2, PropertyName = "specifiers")]
            public List<ASTToken> Specifiers { get; set; }
            [JsonProperty(Order = 3, PropertyName = "declarator")]
            public ASTDeclarator Declarator { get; set; }
            [JsonProperty(Order = 4, PropertyName = "body")]
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
            [JsonProperty(Order = 2, PropertyName = "specifiers")]
            public List<ASTToken> Specifiers { get; set; }
            [JsonProperty(Order = 3, PropertyName = "initLists")]
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
            [JsonProperty(Order = 2, PropertyName = "value")]
            public string Value { get; set; }
            [JsonProperty(Order = 3, PropertyName = "tokenId")]
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
            [JsonProperty(Order = 2, PropertyName = "specfiers")]
            public List<ASTToken> Specfiers { get; set; }
            [JsonProperty(Order = 3, PropertyName = "declarator")]
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
        [JsonConverter(typeof(JsonSubtypes), "type")]
        [JsonSubtypes.KnownSubType(typeof(ASTArrayDeclarator), "ArrayDeclarator")]
        [JsonSubtypes.KnownSubType(typeof(ASTVariableDeclarator), "VariableDeclarator")]
        [JsonSubtypes.KnownSubType(typeof(ASTFunctionDeclarator), "FunctionDeclarator")]
        abstract public partial class ASTDeclarator : ASTNode
        {
            public ASTDeclarator(string type) : base(type)
            {

            }
        }
        public partial class ASTParamsDeclarator : ASTNode
        {
            [JsonProperty(Order = 2, PropertyName = "specfiers")]
            public List<ASTToken> Specfiers { get; set; }
            [JsonProperty(Order = 3, PropertyName = "declarator")]
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
            [JsonProperty(Order = 2, PropertyName = "declarator")]
            public ASTDeclarator Declarator { get; set; }
            [JsonProperty(Order = 3, PropertyName = "exprs")]
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
            [JsonProperty(Order = 2, PropertyName = "value")]
            public string Value { get; set; }
            [JsonProperty(Order = 3, PropertyName = "tokenId")]
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
            [JsonProperty(Order = 2, PropertyName = "arrayName")]
            public ASTExpression ArrayName { get; set; }
            [JsonProperty(Order = 3, PropertyName = "elements")]
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
            [JsonProperty(Order = 2, PropertyName = "op")]
            public ASTToken Operator { get; set; }
            [JsonProperty(Order = 3, PropertyName = "expr1")]
            public ASTExpression Expr1 { get; set; }
            [JsonProperty(Order = 4, PropertyName = "expr2")]
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
            [JsonProperty(Order = 2, PropertyName = "value")]
            public string Value { get; set; }
            [JsonProperty(Order = 3, PropertyName = "tokenId")]
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
            [JsonProperty(Order = 2, PropertyName = "value")]
            public double Value { get; set; }
            [JsonProperty(Order = 3, PropertyName = "tokenId")]
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
            [JsonProperty(Order = 2, PropertyName = "funcname")]
            public ASTExpression FunctionName { get; set; }
            [JsonProperty(Order = 3, PropertyName = "argList")]
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
            [JsonProperty(Order = 2, PropertyName = "value")]
            public int Value { get; set; }
            [JsonProperty(Order = 3, PropertyName = "tokenId")]
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
            [JsonProperty(Order = 2, PropertyName = "expr")]
            public ASTExpression Expression { get; set; }
            [JsonProperty(Order = 3, PropertyName = "op")]
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
            [JsonProperty(Order = 2, PropertyName = "value")]
            public string Value { get; set; }
            [JsonProperty(Order = 3, PropertyName = "tokenId")]
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
            [JsonProperty(Order = 2, PropertyName = "op")]
            public ASTToken Operator { get; set; }
            [JsonProperty(Order = 3, PropertyName = "expr")]
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
            [JsonProperty(Order = 2, PropertyName = "blockItems")]
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
            [JsonProperty(Order = 2, PropertyName = "exprs")]
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
            [JsonProperty(Order = 2, PropertyName = "init")]
            public ASTDeclaration Initilize { get; set; }
            [JsonProperty(Order = 3, PropertyName = "cond")]
            public List<ASTNode> Condition { get; set; }
            [JsonProperty(Order = 4, PropertyName = "step")]
            public List<ASTNode> Step { get; set; }
            [JsonProperty(Order = 5, PropertyName = "stat")]
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
            [JsonProperty(Order = 2, PropertyName = "init")]
            public List<ASTNode> Initilize { get; set; }
            [JsonProperty(Order = 3, PropertyName = "cond")]
            public List<ASTNode> Condition { get; set; }
            [JsonProperty(Order = 4, PropertyName = "step")]
            public List<ASTNode> Step { get; set; }
            [JsonProperty(Order = 5, PropertyName = "stat")]
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
            [JsonProperty(Order = 2, PropertyName = "expr")]
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
            [JsonProperty(Order = 2, PropertyName = "cond")]
            public List<ASTNode> Condition { get; set; }
            [JsonProperty(Order = 3, PropertyName = "then")]
            public ASTStatement Then { get; set; }
            [JsonProperty(Order = 4, PropertyName = "otherwise")]
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
            [JsonProperty(Order = 2, PropertyName = "declarator")]
            public ASTDeclarator Declarator { get; set; }
            [JsonProperty(Order = 3, PropertyName = "expr")]
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
            [JsonProperty(Order = 2, PropertyName = "identifier")]
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
            [JsonProperty(Order = 2, PropertyName = "declarator")]
            public ASTDeclarator Declarator { get; set; }
            [JsonProperty(Order = 3, PropertyName = "params")]
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