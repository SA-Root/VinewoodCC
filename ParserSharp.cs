using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ParserSharp
{
    public class ScannerTokens
    {
        public string Value { get; set; }
        public string Type { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }
    public class Parser
    {
        public string OutputFile { get; set; }
        private List<ScannerTokens> Tokens { get; set; }
        private int TokenIndex { get; set; }
        private ASTNode Root { get; set; }
        private int LoadTokens(string path)
        {
            Tokens = new List<ScannerTokens>();
            TokenIndex = 0;
            try
            {
                var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                using (var reader = new StreamReader(stream))
                {
                    //[@26,77:78='<=',<'<='>,5:14]
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var ValueLeftQuote = line.IndexOf('\'');
                        var ValueRightQuote = line.LastIndexOf("',<");
                        var TypeLeftBracket = ValueRightQuote + 2;//x
                        var TypeRightBracket = line.LastIndexOf('>');
                        if (TypeLeftBracket + 2 == TypeRightBracket)
                        {
                            TypeLeftBracket -= 2;
                        }
                        var LineLeft = TypeRightBracket + 2;
                        var LineRight = line.LastIndexOf(':') - 1;
                        var ColumnLeft = LineRight + 2;
                        var ColumnRight = line.Length - 2;
                        var token = new ScannerTokens
                        {
                            Value = line.Substring(ValueLeftQuote + 1, ValueRightQuote - ValueLeftQuote - 1),
                            Type = line.Substring(TypeLeftBracket + 1, TypeRightBracket - TypeLeftBracket - 1),
                            Line = int.Parse(line.Substring(LineLeft, LineRight - LineLeft + 1)),
                            Column = int.Parse(line.Substring(ColumnLeft, ColumnRight - ColumnLeft + 1))
                        };
                        Tokens.Add(token);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine($"Could not load file: {path}");
                return 1;
            }
            return 0;
        }
        private void ParseTokens(string path)
        {
            Root = Program();
            Console.WriteLine("Generating JSON file...");
            OutputFile = path.Substring(0, path.LastIndexOf(".tokens")) + ".ast.json";
            var jsonString = JsonConvert.SerializeObject(Root, Formatting.Indented);
            Console.WriteLine($"Writing to \"{OutputFile}\"...");
            try
            {
                var stream = new FileStream(OutputFile, FileMode.Create, FileAccess.Write);
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(jsonString);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine($"Could not write file: {OutputFile}");
            }
        }
        /// <summary>
        /// Match token type
        /// </summary>
        /// <param name="type">Token type</param>
        /// <returns>true for a match</returns>
        private bool MatchToken(string type)
        {
            if (TokenIndex < Tokens.Count)
            {
                if (Tokens[TokenIndex].Type == type)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }
        //PROGRAM -> S
        private ASTNode Program()
        {
            var root = new ASTCompilationUnit();
            var items = S();
            if (items != null)
            {
                root.Items = items;
            }
            return root;
        }
        //S -> DEC_FUNC S|ε
        private List<ASTNode> S()
        {
            var ret = new List<ASTNode>();
            if (MatchToken("EOF"))
            {
                return null;
            }
            ret.Add(DEC_FUNC());
            var SList = S();
            if (SList != null)
            {
                ret.AddRange(SList);
            }
            return ret;
        }
        //DEC_FUNC -> TYPENAME Identifier VAR_FUNC
        private ASTNode DEC_FUNC()
        {
            var typename = TYPENAME();
            if (MatchToken("Identifier"))
            {
                var identifier = new ASTIdentifier
                {
                    TokenID = TokenIndex,
                    Value = Tokens[TokenIndex].Value
                };
                ++TokenIndex;
                var var_func = VAR_FUNC();
                //Global var define
                if (var_func is ASTDeclaration VarDef)
                {
                    VarDef.Specifiers.Add(typename);
                    //var def at least one
                    if (VarDef.InitLists.Count >= 1)
                    {
                        //first is var
                        if (VarDef.InitLists[0].Declarator is ASTVariableDeclarator VarDecl)
                        {
                            VarDecl.Identifier = identifier;
                        }
                        //first is array
                        else if (VarDef.InitLists[0].Declarator is ASTArrayDeclarator ArrDecl)
                        {
                            if (ArrDecl.Declarator is ASTVariableDeclarator adl)
                            {
                                adl.Identifier = identifier;
                            }
                            else if (ArrDecl.Declarator is ASTArrayDeclarator aad)
                            {
                                (aad.Declarator as ASTVariableDeclarator).Identifier = identifier;
                            }
                        }
                        return VarDef;
                    }
                    else//InitList missing
                    {
                        throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
                    }
                }
                //Function impl
                else if (var_func is ASTFunctionDefine FuncDef)
                {
                    FuncDef.Specifiers.Add(typename);
                    (FuncDef.Declarator as ASTFunctionDeclarator).Declarator = new ASTVariableDeclarator
                    {
                        Identifier = identifier
                    };
                    return FuncDef;
                }
                //Function decl
                else if (var_func is ASTFunctionDeclarator FuncDecl)
                {
                    FuncDecl.Declarator = new ASTVariableDeclarator
                    {
                        Identifier = identifier
                    };
                    return new ASTDeclaration
                    {
                        Specifiers = new List<ASTToken>
                        {
                            typename
                        },
                        InitLists = new List<ASTInitList>
                        {
                            new ASTInitList
                            {
                                Declarator = FuncDecl,
                                Expressions = new List<ASTExpression>()
                            }
                        }
                    };
                }
            }
            else//missing identifier
            {
                throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
            }
            return null;
        }
        //VAR_FUNC -> ( PARAM_LIST ) ; | ( PARAM_LIST ) FUNC_BODY|VAR_DECL
        private ASTNode VAR_FUNC()
        {
            //Function
            if (MatchToken("'('"))
            {
                ++TokenIndex;
                var ret = new ASTFunctionDefine
                {
                    Declarator = new ASTFunctionDeclarator
                    {
                        Parameters = PARAM_LIST()
                    }
                };
                MatchToken("')'");
                ++TokenIndex;
                //func decl
                if (MatchToken("';'"))
                {
                    ++TokenIndex;
                    return ret.Declarator;
                }
                //func impl
                else
                {
                    ret.Body = FUNC_BODY();
                    return ret;
                }
            }
            //Var
            else
            {
                if (MatchToken("','"))
                {
                    var ret = new ASTDeclaration
                    {
                        InitLists = new List<ASTInitList>()
                    };
                    ret.InitLists.Add(new ASTInitList
                    {
                        Declarator = new ASTVariableDeclarator(),
                        Expressions = new List<ASTExpression>()
                    });
                    ret.InitLists.AddRange(VAR_DECL());
                    return ret;
                }
                else
                {
                    return new ASTDeclaration
                    {
                        InitLists = VAR_DECL()
                    };
                }
            }
        }
        //VAR_DECL -> ;|,Identifier ST_VAR_DECL VAR_DECL|ST_VAR_DECL VAR_DECL
        private List<ASTInitList> VAR_DECL()
        {
            //next var decl
            if (MatchToken("','"))
            {
                ++TokenIndex;
                var ret = new List<ASTInitList>()
                {
                };
                //next var's identifier
                if (MatchToken("Identifier"))
                {
                    var identifier = new ASTIdentifier
                    {
                        Value = Tokens[TokenIndex].Value,
                        TokenID = TokenIndex
                    };
                    ++TokenIndex;
                    //int a,b;
                    if (MatchToken("';'"))
                    {
                        ++TokenIndex;
                        ret.Add(new ASTInitList
                        {
                            Declarator = new ASTVariableDeclarator
                            {
                                Identifier = identifier
                            },
                            Expressions = new List<ASTExpression>()
                        });
                    }
                    //int a,b,...
                    else
                    {
                        if (MatchToken("','"))
                        {
                            ret.Add(new ASTInitList
                            {
                                Declarator = new ASTVariableDeclarator
                                {
                                    Identifier = identifier
                                },
                                Expressions = new List<ASTExpression>()
                            });
                        }
                        else
                        {
                            var var2 = ST_VAR_DECL();
                            //var
                            if (var2.Declarator is ASTVariableDeclarator vd)
                            {
                                vd.Identifier = identifier;
                            }
                            //var array
                            else if (var2.Declarator is ASTArrayDeclarator ad)
                            {
                                if (ad.Declarator is ASTVariableDeclarator adl)
                                {
                                    adl.Identifier = identifier;
                                }
                                else if (ad.Declarator is ASTArrayDeclarator aad)
                                {
                                    (aad.Declarator as ASTVariableDeclarator).Identifier = identifier;
                                }
                            }
                            ret.Add(var2);
                        }
                        var var3s = VAR_DECL();
                        if (var3s != null)
                        {
                            ret.AddRange(var3s);
                        }
                    }
                    return ret;
                }
                else//missing identifier
                {
                    throw new VariableDeclarationMissingIdentifierException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
                }
            }
            //end of var decl
            else if (MatchToken("';'"))
            {
                ++TokenIndex;
                return null;
            }
            //first var with (array mark) init value
            else
            {
                var ret = new List<ASTInitList>{
                    //var1, no expr
                    ST_VAR_DECL()
                };
                var var2s = VAR_DECL();
                if (var2s != null)
                {
                    ret.AddRange(var2s);
                }
                return ret;
            }
        }
        //ST_VAR_DECL -> ε | = CONST_EXPR | = FUNC_CALL | ARRAY_MARK ARRAY_DECL_ASSIGN
        private ASTInitList ST_VAR_DECL()
        {
            //var assign
            if (MatchToken("'='"))
            {
                ++TokenIndex;
                var ret = new ASTInitList
                {
                    Declarator = new ASTVariableDeclarator(),
                    Expressions = new List<ASTExpression>()
                };
                var v = NORM_EXPR();
                if (v is ASTExpression e)
                {
                    ret.Expressions.Add(e);
                }
                else if (v is ASTExpressionStatement es)
                {
                    ret.Expressions.Add(es.Expressions[0] as ASTExpression);
                }
                return ret;
            }
            //array mark
            else if (MatchToken("'['"))
            {
                return new ASTInitList
                {
                    Declarator = ARRAY_MARK(),
                    Expressions = ARRAY_DECL_ASSIGN()
                };
            }
            // //,
            // else if (MatchToken("','"))
            // {
            //     ++TokenIndex;
            //     return new ASTInitList
            //     {
            //         Declarator = new ASTVariableDeclarator(),
            //         Expressions = new List<ASTExpression>()
            //     };
            // }
            //var with no assign
            else
            {
                return null;
            }
        }
        //ARRAY_MARK -> [INT_CONST/Identifier] ARRAY_MARK| ε
        private ASTArrayDeclarator ARRAY_MARK()
        {
            if (MatchToken("'['"))
            {
                ++TokenIndex;
                var ret = new ASTArrayDeclarator
                {
                    Declarator = new ASTVariableDeclarator()
                };
                var exp = NORM_EXPR();
                if (exp is ASTExpression e)
                {
                    ret.Expression = e;
                }
                else if (exp is ASTExpressionStatement es)
                {
                    ret.Expression = es.Expressions[0] as ASTExpression;
                }
                if (MatchToken("']'"))
                {
                    ++TokenIndex;
                }
                //missing ']'
                else
                {
                    throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
                }
                var ret2 = ARRAY_MARK();
                //multi-dimension array
                if (ret2 != null)
                {
                    ret2.Declarator = ret;
                    return ret2;
                }
                else
                {
                    return ret;
                }
            }
            //end/error
            else
            {
                return null;
            }
        }
        //ARRAY_DECL_ASSIGN -> ε|= ARRAY_INIT_VALUE
        private List<ASTExpression> ARRAY_DECL_ASSIGN()
        {
            if (MatchToken("'='"))
            {
                ++TokenIndex;
                return ARRAY_INIT_VALUE();
            }
            //int a[5];
            //int a[5],
            else if (MatchToken("','") || MatchToken("';'"))
            {
                return new List<ASTExpression>();
            }
            //missing '='
            else
            {
                throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
            }
        }
        //ARRAY_INIT_VALUE -> { CONST_EXPR EXT_CONST }
        private List<ASTExpression> ARRAY_INIT_VALUE()
        {
            if (MatchToken("'{'"))
            {
                ++TokenIndex;
                var ret = new List<ASTExpression>
                {
                    CONST_EXPR()
                };
                var ext = EXT_CONST();
                if (ext != null)
                {
                    ret.AddRange(ext);
                }
                if (MatchToken("'}'"))
                {
                    ++TokenIndex;
                    return ret;
                }
                //missing '}'
                else
                {
                    throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
                }
            }
            //missing '{'
            else
            {
                throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
            }
        }
        //EXT_CONST -> , CONST_EXPR EXT_CONST | ε
        private List<ASTExpression> EXT_CONST()
        {
            //next const
            if (MatchToken("','"))
            {
                ++TokenIndex;
                var ret = new List<ASTExpression>
                {
                    CONST_EXPR()
                };
                var ext = EXT_CONST();
                if (ext != null)
                {
                    ret.AddRange(ext);
                }
                return ret;
            }
            //end
            else if (MatchToken("'}'"))
            {
                return null;
            }
            //missing ','
            else
            {
                throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
            }
        }
        //CONST_EXPR -> INT_CONST | FLOAT_CONST | CHAR_CONST | STRING_CONST
        private ASTExpression CONST_EXPR()
        {
            if (MatchToken("IntegerConstant"))
            {
                return INT_CONST();
            }
            else if (MatchToken("FloatConstant"))
            {
                return FLOAT_CONST();
            }
            else if (MatchToken("CharConstant"))
            {
                return CHAR_CONST();
            }
            else if (MatchToken("StringLiteral"))
            {
                return STRING_CONST();
            }
            //unsupported
            else
            {
                throw new UnsupportedSyntaticException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
            }
        }
        //INT_CONST -> [+/-] integer
        private ASTIntegerConstant INT_CONST()
        {
            bool isNeg = false;
            if (MatchToken("'+'"))
            {
                ++TokenIndex;
            }
            else if (MatchToken("'-'"))
            {
                ++TokenIndex;
                isNeg = true;
            }
            var ret = new ASTIntegerConstant
            {
                TokenID = TokenIndex
            };
            if (isNeg)
            {
                ret.Value = -int.Parse(Tokens[TokenIndex].Value);
            }
            else
            {
                ret.Value = int.Parse(Tokens[TokenIndex].Value);
            }
            ++TokenIndex;
            return ret;
        }
        //FLOAT_CONST
        private ASTFloatConstant FLOAT_CONST()
        {
            var ret = new ASTFloatConstant
            {
                TokenID = TokenIndex,
                Value = double.Parse(Tokens[TokenIndex].Value)
            };
            ++TokenIndex;
            return ret;
        }
        //CHAR_CONST
        private ASTCharConstant CHAR_CONST()
        {
            var ret = new ASTCharConstant
            {
                TokenID = TokenIndex,
                Value = Tokens[TokenIndex].Value
            };
            ++TokenIndex;
            return ret;
        }
        //STRING_CONST
        private ASTStringConstant STRING_CONST()
        {
            var ret = new ASTStringConstant
            {
                TokenID = TokenIndex,
                Value = Tokens[TokenIndex].Value
            };
            ++TokenIndex;
            return ret;
        }
        //PARAM_LIST -> ARG_DECL EXT_ARGS | ε
        private List<ASTParamsDeclarator> PARAM_LIST()
        {
            //no args
            if (MatchToken("')'"))
            {
                return new List<ASTParamsDeclarator>();
            }
            else
            {
                var ret = new List<ASTParamsDeclarator>
                {
                    ARG_DECL()
                };
                var ext = EXT_ARGS();
                if (ext != null)
                {
                    ret.AddRange(ext);
                }
                return ret;
            }
        }
        //ARG_DECL -> TYPENAME Identifier
        private ASTParamsDeclarator ARG_DECL()
        {
            var ret = new ASTParamsDeclarator
            {
                Specfiers = new List<ASTToken>
                {
                    TYPENAME()
                }
            };
            if (MatchToken("Identifier"))
            {
                ret.Declarator = new ASTVariableDeclarator
                {
                    Identifier = new ASTIdentifier
                    {
                        TokenID = TokenIndex,
                        Value = Tokens[TokenIndex].Value
                    }
                };
                ++TokenIndex;
                //array arg
                if (MatchToken("'['"))
                {
                    throw new UnsupportedSyntaticException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
                }
                return ret;
            }
            //missing identifier
            else
            {
                throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
            }
        }
        //EXT_ARGS -> , ARG_DECL EXT_ARGS | ε
        private List<ASTParamsDeclarator> EXT_ARGS()
        {
            if (MatchToken("','"))
            {
                ++TokenIndex;
                var ret = new List<ASTParamsDeclarator>
                {
                    ARG_DECL()
                };
                var ext = EXT_ARGS();
                if (ext != null)
                {
                    ret.AddRange(ext);
                }
                return ret;
            }
            //end
            else if (MatchToken("')'"))
            {
                return null;
            }
            //missing ','
            else
            {
                throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
            }
        }
        //FUNC_BODY -> { IN_FUNC_EXPRS }
        private ASTCompoundStatement FUNC_BODY()
        {
            if (MatchToken("'{'"))
            {
                ++TokenIndex;
                var ret = new ASTCompoundStatement
                {
                    BlockItems = IN_FUNC_EXPRS()
                };
                if (MatchToken("'}'"))
                {
                    ++TokenIndex;
                    return ret;
                }
                //missing '}'
                else
                {
                    throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
                }
            }
            //missing '{'
            else
            {
                throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
            }
        }
        //IN_FUNC_EXPRS -> EXPRESSION IN_FUNC_EXPRS | ε
        private List<ASTNode> IN_FUNC_EXPRS()
        {
            //end
            if (MatchToken("'}'"))
            {
                return new List<ASTNode>();
            }
            else
            {
                var ret = new List<ASTNode>();
                var expr = EXPRESSION();
                if (expr != null)
                {
                    ret.Add(expr);
                }
                var ext = IN_FUNC_EXPRS();
                ret.AddRange(ext);
                return ret;
            }
        }
        //EXPRESSION -> VAR_DEF | RETURN_EXPR | NORM_EXPR ; | IF_EXPR | FOR_EXPR
        private ASTNode EXPRESSION()
        {
            //VAR_DEF
            if (MatchToken("'double'")
            || MatchToken("'float'")
            || MatchToken("'int'")
            || MatchToken("'char'")
            || MatchToken("'bool'")
            || MatchToken("'long'")
            || MatchToken("'void'"))
            {
                return VAR_DEF();
            }
            //RETURN_EXPR
            else if (MatchToken("'return'"))
            {
                return RETURN_EXPR();
            }
            //NORM_EXPR
            else if (MatchToken("Identifier"))
            {
                var ret = NORM_EXPR();
                if (MatchToken("';'"))
                {
                    ++TokenIndex;
                }
                return ret;
            }
            //IF_EXPR
            else if (MatchToken("'if'"))
            {
                return IF_EXPR();
            }
            //FOR_EXPR
            else if (MatchToken("'for'"))
            {
                return FOR_EXPR();
            }
            else if (MatchToken("'break'"))
            {
                return BREAK_EXPR();
            }
            //;
            else if (MatchToken("';'"))
            {
                ++TokenIndex;
                return null;
            }
            //)
            else if (MatchToken("')'"))
            {
                return null;
            }
            else
            {
                throw new UnsupportedSyntaticException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
            }
        }
        //BREAK_EXPR
        private ASTBreakStatement BREAK_EXPR()
        {
            ++TokenIndex;
            if (MatchToken("';'"))
            {
                ++TokenIndex;
                return new ASTBreakStatement();
            }
            else
            {
                throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
            }
        }
        //VAR_DEF -> TYPENAME Identifier VAR_DECL
        private ASTDeclaration VAR_DEF()
        {
            var ret = new ASTDeclaration
            {
                Specifiers = new List<ASTToken>
                {
                    TYPENAME()
                }
            };
            if (MatchToken("Identifier"))
            {
                var identifier = new ASTIdentifier
                {
                    Value = Tokens[TokenIndex].Value,
                    TokenID = TokenIndex
                };
                ++TokenIndex;
                //int a;
                if (MatchToken("';'"))
                {
                    ++TokenIndex;
                    ret.InitLists = new List<ASTInitList>
                    {
                        new ASTInitList
                        {
                            Declarator = new ASTVariableDeclarator
                            {
                                Identifier = identifier
                            },
                            Expressions=new List<ASTExpression>()
                        }
                    };
                }
                else
                {
                    if (MatchToken("','"))
                    {
                        ret.InitLists = new List<ASTInitList>
                        {
                            new ASTInitList
                            {
                                Declarator = new ASTVariableDeclarator
                                {
                                    Identifier = identifier
                                },
                                Expressions=new List<ASTExpression>()
                            }
                        };
                        var decl = VAR_DECL();
                        ret.InitLists.AddRange(decl);
                    }
                    else
                    {
                        var decl = VAR_DECL();
                        if (decl[0].Declarator is ASTVariableDeclarator VarDecl)
                        {
                            VarDecl.Identifier = identifier;
                        }
                        //first is array
                        else if (decl[0].Declarator is ASTArrayDeclarator ArrDecl)
                        {
                            if (ArrDecl.Declarator is ASTVariableDeclarator adl)
                            {
                                adl.Identifier = identifier;
                            }
                            else if (ArrDecl.Declarator is ASTArrayDeclarator aad)
                            {
                                (aad.Declarator as ASTVariableDeclarator).Identifier = identifier;
                            }
                        }
                        ret.InitLists.AddRange(decl);
                    }
                }
                return ret;
            }
            else
            {
                throw new VariableDeclarationMissingIdentifierException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
            }
        }
        //RETURN_EXPR -> Return NORM_EXPR ;
        private ASTReturnStatement RETURN_EXPR()
        {
            ++TokenIndex;
            if (MatchToken("';'"))
            {
                ++TokenIndex;
                return new ASTReturnStatement
                {
                    Expression = null
                };
            }
            var ret = new ASTReturnStatement
            {
                Expression = (NORM_EXPR() as ASTExpressionStatement).Expressions
            };
            if (MatchToken("';'"))
            {
                ++TokenIndex;
            }
            return ret;
        }
        //NORM_EXPR -> PS_OPERATOR Identifier EXT_Identifier | Identifier EXT_Identifier NORM_EXPR_R | CONST NORM_EXPR_R
        private ASTNode NORM_EXPR()
        {
            //(expr)
            if (MatchToken("'('"))
            {
                ++TokenIndex;
                var left = NORM_EXPR();
                if (MatchToken("')'"))
                {
                    ++TokenIndex;
                    return left;
                }
                //missing ')'
                else
                {
                    throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
                }
            }
            //no prefix
            else if (MatchToken("Identifier"))
            {
                var ret = new ASTExpressionStatement
                {
                    Expressions = new List<ASTNode>()
                };
                var identifier = new ASTIdentifier
                {
                    Value = Tokens[TokenIndex].Value,
                    TokenID = TokenIndex
                };
                ++TokenIndex;
                var ext = EXT_Identifier();
                if (ext is ASTArrayAccess aa)
                {
                    if (aa.ArrayName is ASTArrayAccess aaa)
                    {
                        (aa.ArrayName as ASTArrayAccess).ArrayName = identifier;
                    }
                    else
                    {
                        aa.ArrayName = identifier;
                    }
                    if (MatchToken("'*'")
                    || MatchToken("'/'")
                    || MatchToken("'%'"))
                    {
                        var rett = NORM_EXPR_R();
                        ((rett as ASTBinaryExpression).Expr1 as ASTBinaryExpression).Expr1 = aa;
                        ret.Expressions.Add(rett);
                    }
                    else
                    {
                        var rett = NORM_EXPR_R();
                        //x[1]=...;
                        if (rett is ASTBinaryExpression be)
                        {
                            be.Expr1 = aa;
                            ret.Expressions.Add(be);
                        }
                        //a[1]++;
                        else if (rett is ASTPostfixExpression pe)
                        {
                            pe.Expression = aa;
                            ret.Expressions.Add(pe);
                        }
                        else
                        {
                            ret.Expressions.Add(aa);
                        }
                    }
                }
                else
                {
                    if (MatchToken("'*'")
                    || MatchToken("'/'")
                    || MatchToken("'%'"))
                    {
                        var rett = NORM_EXPR_R();
                        ((rett as ASTBinaryExpression).Expr1 as ASTBinaryExpression).Expr1 = identifier;
                        ret.Expressions.Add(rett);
                    }
                    else
                    {
                        var rett = NORM_EXPR_R();
                        //x=...
                        if (rett is ASTBinaryExpression be)
                        {
                            be.Expr1 = identifier;
                            ret.Expressions.Add(be);
                        }
                        //a()
                        else if (rett is ASTFunctionCall fc)
                        {
                            fc.FunctionName = identifier;
                            ret.Expressions.Add(fc);
                        }
                        //a++
                        else if (rett is ASTPostfixExpression pe)
                        {
                            pe.Expression = identifier;
                            ret.Expressions.Add(pe);
                        }
                        else
                        {
                            ret.Expressions.Add(identifier);
                        }
                    }
                }
                return ret;
            }
            else
            {
                //prefix expr
                if (MatchToken("'++'") || MatchToken("'--'") || MatchToken("'!'"))
                {
                    var ret = new ASTExpressionStatement
                    {
                        Expressions = new List<ASTNode>()
                    };
                    var ue = new ASTUnaryExpression
                    {
                        Operator = PS_OPERATOR()
                    };
                    if (MatchToken("Identifier"))
                    {
                        var identifier = new ASTIdentifier
                        {
                            Value = Tokens[TokenIndex].Value,
                            TokenID = TokenIndex
                        };
                        ++TokenIndex;
                        var ext = EXT_Identifier();
                        //++a[1]
                        if (ext is ASTArrayAccess aa)
                        {
                            if (aa.ArrayName is ASTArrayAccess aaa)
                            {
                                (aa.ArrayName as ASTArrayAccess).ArrayName = identifier;
                            }
                            else
                            {
                                aa.ArrayName = identifier;
                            }
                            ue.Expression = aa;
                        }
                        //++b
                        else
                        {
                            ue.Expression = identifier;
                        }
                        ret.Expressions.Add(ue);
                        return ret;
                    }
                    //missing identifier
                    else
                    {
                        throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
                    }
                }
                //CONST
                else
                {
                    if (MatchToken("')'") || MatchToken("';'") || MatchToken("','") || MatchToken("']'"))
                    {
                        return null;
                    }
                    var cst = CONST_EXPR();
                    if (MatchToken("')'") || MatchToken("';'") || MatchToken("','") || MatchToken("']'"))
                    {
                        return new ASTExpressionStatement
                        {
                            Expressions = new List<ASTNode>
                            {
                                cst
                            }
                        };
                    }
                    else
                    {
                        var bin = NORM_EXPR_R();
                        (bin as ASTBinaryExpression).Expr1 = cst;
                        return new ASTExpressionStatement
                        {
                            Expressions = new List<ASTNode>
                            {
                                bin
                            }
                        };
                    }
                }
            }
        }
        //NORM_EXPR_R -> ASSIGN_OPERAOTR CONST_EXPR | ASSIGN_OPERAOTR Identifier EXT_Identifier PB_OPERATOR | PB_OPERATOR
        private ASTExpression NORM_EXPR_R()
        {
            //assign
            if (MatchToken("'='")
            || MatchToken("'+='")
            || MatchToken("'-='")
            || MatchToken("'*='")
            || MatchToken("'/='")
            || MatchToken("'%='"))
            {
                var ret = new ASTBinaryExpression
                {
                    Operator = new ASTToken
                    {
                        TokenID = TokenIndex,
                        Value = Tokens[TokenIndex].Value
                    }
                };
                ++TokenIndex;
                //expr2
                if (MatchToken("Identifier"))
                {
                    var identifier = new ASTIdentifier
                    {
                        TokenID = TokenIndex,
                        Value = Tokens[TokenIndex].Value
                    };
                    ++TokenIndex;
                    var ext = EXT_Identifier();
                    //<bin>a[3]...
                    if (ext is ASTArrayAccess aa)
                    {
                        if (aa.ArrayName is ASTArrayAccess aaa)
                        {
                            (aa.ArrayName as ASTArrayAccess).ArrayName = identifier;
                        }
                        else
                        {
                            aa.ArrayName = identifier;
                        }
                        var expr2 = PB_OPERATOR();
                        //<bin>a[3]+...
                        if (expr2 is ASTBinaryExpression be)
                        {
                            if (be.Expr1 is ASTBinaryExpression bin)
                            {
                                bin.Expr1 = aa;
                            }
                            else
                            {
                                be.Expr1 = aa;
                            }
                            ret.Expr2 = be;
                        }
                        //<bin>a[3]++
                        else if (expr2 is ASTPostfixExpression pe)
                        {
                            pe.Expression = aa;
                            ret.Expr2 = pe;
                        }
                        else
                        {
                            ret.Expr2 = aa;
                        }
                    }
                    //<bin>a...
                    else
                    {
                        var expr2 = PB_OPERATOR();
                        //=a+...
                        if (expr2 is ASTBinaryExpression be)
                        {
                            if (be.Expr1 is ASTBinaryExpression bin)
                            {
                                bin.Expr1 = identifier;
                            }
                            else
                            {
                                be.Expr1 = identifier;
                            }
                            ret.Expr2 = be;
                        }
                        //=a++
                        else if (expr2 is ASTPostfixExpression pe)
                        {
                            pe.Expression = identifier;
                            ret.Expr2 = pe;
                        }
                        //=a(...)
                        else if (expr2 is ASTFunctionCall fc)
                        {
                            fc.FunctionName = identifier;
                            var r2 = PB_OPERATOR();
                            if (r2 != null)
                            {
                                (r2 as ASTBinaryExpression).Expr1 = fc;
                                ret.Expr2 = r2;
                            }
                            else
                            {
                                ret.Expr2 = fc;
                            }
                        }
                        //=a;
                        else
                        {
                            ret.Expr2 = identifier;
                        }
                        //;
                    }
                    return ret;
                }
                //missing expr2
                else
                {
                    ret.Expr2 = CONST_EXPR();
                    return ret;
                }
            }
            else
            {
                return PB_OPERATOR();
            }
        }
        //PB_OPERATOR -> ++|--| BIN_OPERATOR Identifier/CONST PB_OPERATOR | OP_EXPR2 | ε
        private ASTExpression PB_OPERATOR()
        {
            if (MatchToken("'++'") || MatchToken("'--'"))
            {
                var op = new ASTToken
                {
                    Value = Tokens[TokenIndex].Value,
                    TokenID = TokenIndex
                };
                ++TokenIndex;
                return new ASTPostfixExpression
                {
                    Operator = op
                };
            }
            //expr2 func call
            else if (MatchToken("'('"))
            {
                return OP_EXPR2();
            }
            else
            {
                var ret = new ASTBinaryExpression();
                if (MatchToken("'*'")
                || MatchToken("'/'")
                || MatchToken("'%'"))
                {
                    var op = new ASTToken
                    {
                        Value = Tokens[TokenIndex].Value,
                        TokenID = TokenIndex
                    };
                    ++TokenIndex;
                    if (MatchToken("Identifier"))
                    {
                        var identifier = new ASTIdentifier
                        {
                            Value = Tokens[TokenIndex].Value,
                            TokenID = TokenIndex
                        };
                        ++TokenIndex;
                        var ext = EXT_Identifier();
                        if (ext != null)
                        {
                            (ext as ASTArrayAccess).ArrayName = identifier;
                            if ((ext as ASTArrayAccess).ArrayName is ASTArrayAccess aaa)
                            {
                                (aaa.ArrayName as ASTArrayAccess).ArrayName = identifier;
                            }
                            else
                            {
                                (ext as ASTArrayAccess).ArrayName = identifier;
                            }
                        }
                        if (MatchToken("')") || MatchToken("';'"))
                        {
                            ret.Operator = op;
                            ret.Expr2 = ext;
                        }
                        else
                        {
                            ret.Expr1 = new ASTBinaryExpression
                            {
                                Operator = op,
                                Expr2 = identifier
                            };
                            var r = PB_OPERATOR() as ASTBinaryExpression;
                            ret.Expr2 = r.Expr2;
                            ret.Operator = r.Operator;
                        }
                    }
                    //missing identifier
                    else
                    {
                        var e1 = CONST_EXPR();
                        if (MatchToken("')") || MatchToken("';'"))
                        {
                            ret.Operator = op;
                            ret.Expr2 = e1;
                        }
                        else
                        {
                            ret.Expr1 = new ASTBinaryExpression
                            {
                                Operator = op,
                                Expr2 = e1
                            };
                            var r = PB_OPERATOR() as ASTBinaryExpression;
                            ret.Expr2 = r.Expr2;
                            ret.Operator = r.Operator;
                        }
                    }
                    return ret;
                }
                else if (MatchToken("'+'")
                || MatchToken("'-'")
                || MatchToken("'>'")
                || MatchToken("'<'")
                || MatchToken("'=='")
                || MatchToken("'>='")
                || MatchToken("'<='")
                || MatchToken("'!='"))
                {
                    ret.Operator = new ASTToken
                    {
                        Value = Tokens[TokenIndex].Value,
                        TokenID = TokenIndex
                    };
                    ++TokenIndex;
                    var r = NORM_EXPR();
                    if (r is ASTExpression e)
                    {
                        ret.Expr2 = e;
                    }
                    else if (r is ASTExpressionStatement es)
                    {
                        ret.Expr2 = es.Expressions[0] as ASTExpression;
                    }
                    return ret;
                }
                else
                {
                    return null;
                }
            }
        }
        //OP_EXPR2 -> ( ARG_LIST )
        private ASTFunctionCall OP_EXPR2()
        {
            ++TokenIndex;
            var ret = new ASTFunctionCall
            {
                ArgList = ARG_LIST()
            };
            if (MatchToken("')'"))
            {
                ++TokenIndex;
                return ret;
            }
            else
            {
                throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
            }
        }
        //ARG_LIST -> NORM_EXPR ARG_LIST | , NORM_EXPR ARG_LIST | ε
        private List<ASTNode> ARG_LIST()
        {
            if (MatchToken("')'"))
            {
                return new List<ASTNode>();
            }
            else if (MatchToken("','"))
            {
                ++TokenIndex;
            }
            var ret = new List<ASTNode>
            {
                (NORM_EXPR() as ASTExpressionStatement).Expressions[0]
            };
            var rett = ARG_LIST();
            if (rett.Count > 0)
            {
                ret.AddRange(rett);
            }
            return ret;
        }
        //EXT_Identifier → ε | ARRAY_MARK
        private ASTArrayAccess EXT_Identifier()
        {
            if (MatchToken("'['"))
            {
                ++TokenIndex;
                var ret = new ASTArrayAccess
                {
                    Elements = new List<ASTExpression>()
                        {
                            (NORM_EXPR() as ASTExpressionStatement).Expressions[0] as ASTExpression
                        }
                };
                if (MatchToken("']'"))
                {
                    ++TokenIndex;
                    if (MatchToken("'['"))
                    {
                        var rett = EXT_Identifier();
                        rett.ArrayName = ret;
                        return rett;
                    }
                    else
                    {
                        return ret;
                    }
                }
                //missing ']'
                else
                {
                    throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
                }
            }
            else
            {
                return null;
            }
        }
        //PS_OPERATOR -> ++ | -- | !
        private ASTToken PS_OPERATOR()
        {
            if (MatchToken("'++'") || MatchToken("'--'") || MatchToken("'!'"))
            {
                var ret = new ASTToken
                {
                    Value = Tokens[TokenIndex].Value,
                    TokenID = TokenIndex
                };
                ++TokenIndex;
                return ret;
            }
            else
            {
                throw new UnsupportedSyntaticException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
            }
        }
        //IF_EXPR -> IF_1 | IF_1 else CONTENT_EXPR
        private ASTSelectionStatement IF_EXPR()
        {
            var ret = IF_1();
            if (MatchToken("'else'"))
            {
                ++TokenIndex;
                if (MatchToken("'if'"))
                {
                    ret.Otherwise = IF_EXPR();
                }
                else
                {
                    ret.Otherwise = CONTENT_EXPR();
                }
            }
            return ret;
        }
        //IF_1 -> if ( NORM_EXPR ) CONTENT_EXPR
        private ASTSelectionStatement IF_1()
        {
            ++TokenIndex;
            if (MatchToken("'('"))
            {
                ++TokenIndex;
                var ret = new ASTSelectionStatement()
                {
                    Condition = new List<ASTNode>()
                };
                var cond1 = (NORM_EXPR() as ASTExpressionStatement).Expressions[0];
                if (MatchToken("'&&'") || MatchToken("'||'"))
                {
                    var cond = new ASTBinaryExpression
                    {
                        Operator = new ASTToken
                        {
                            Value = Tokens[TokenIndex].Value,
                            TokenID = TokenIndex
                        },
                        Expr1 = cond1 as ASTExpression
                    };
                    ++TokenIndex;
                    cond.Expr2 = (NORM_EXPR() as ASTExpressionStatement).Expressions[0] as ASTExpression;
                    ret.Condition.Add(cond);
                }
                else
                {
                    ret.Condition.Add(cond1);
                }
                if (MatchToken("')'"))
                {
                    ++TokenIndex;
                    ret.Then = CONTENT_EXPR();
                    return ret;
                }
                //missing ')'
                else
                {
                    throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
                }
            }
            //missing '('
            else
            {
                throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
            }
        }
        //CONTENT_EXPR -> EXPRESSION | FUNC_BODY
        private ASTStatement CONTENT_EXPR()
        {
            if (MatchToken("'{'"))
            {
                return FUNC_BODY();
            }
            else
            {
                return EXPRESSION() as ASTStatement;
            }
        }
        //FOR_EXPR -> for ( VAR_DEF/NORM_EXPR ; NORM_EXPR ; NORM_EXPR ) CONTENT_EXPR
        private ASTStatement FOR_EXPR()
        {
            ++TokenIndex;
            if (MatchToken("'('"))
            {
                ++TokenIndex;
                //1 has define
                if (MatchToken("'double'")
                || MatchToken("'float'")
                || MatchToken("'int'")
                || MatchToken("'char'")
                || MatchToken("'bool'")
                || MatchToken("'long'")
                || MatchToken("'void'"))
                {
                    var ret = new ASTIterationDeclaredStatement
                    {
                        Initilize = VAR_DEF(),
                        Condition = new List<ASTNode>()
                    };
                    var cond1 = (NORM_EXPR() as ASTExpressionStatement).Expressions[0];
                    if (MatchToken("'&&'") || MatchToken("'||'"))
                    {
                        var cond = new ASTBinaryExpression
                        {
                            Operator = new ASTToken
                            {
                                Value = Tokens[TokenIndex].Value,
                                TokenID = TokenIndex
                            },
                            Expr1 = cond1 as ASTExpression
                        };
                        ++TokenIndex;
                        cond.Expr2 = (NORM_EXPR() as ASTExpressionStatement).Expressions[0] as ASTExpression;
                        ret.Condition.Add(cond);
                    }
                    else
                    {
                        ret.Condition.Add(cond1);
                    }
                    if (MatchToken("';'"))
                    {
                        ++TokenIndex;
                        var tmp = NORM_EXPR();
                        if (tmp != null)
                        {
                            ret.Step = new List<ASTNode>
                            {
                                (tmp as ASTExpressionStatement).Expressions[0]
                            };
                        }
                        if (MatchToken("')'"))
                        {
                            ++TokenIndex;
                            ret.Stat = CONTENT_EXPR();
                            return ret;
                        }
                        //missing ')'
                        else
                        {
                            throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
                        }
                    }
                    //missing second ';'
                    else
                    {
                        throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
                    }
                }
                else
                {
                    var ret = new ASTIterationStatement();
                    //no def
                    if (MatchToken("Identifier"))
                    {
                        ret.Initilize = new List<ASTNode>
                        {
                            (NORM_EXPR() as ASTExpressionStatement).Expressions[0]
                        };
                    }
                    if (MatchToken("';'"))
                    {
                        ++TokenIndex;
                        ret.Condition = new List<ASTNode>();
                        var cond1 = (NORM_EXPR() as ASTExpressionStatement).Expressions[0];
                        if (MatchToken("'&&'") || MatchToken("'||'"))
                        {
                            var cond = new ASTBinaryExpression
                            {
                                Operator = new ASTToken
                                {
                                    Value = Tokens[TokenIndex].Value,
                                    TokenID = TokenIndex
                                },
                                Expr1 = cond1 as ASTExpression
                            };
                            ++TokenIndex;
                            cond.Expr2 = (NORM_EXPR() as ASTExpressionStatement).Expressions[0] as ASTExpression;
                            ret.Condition.Add(cond);
                        }
                        else
                        {
                            ret.Condition.Add(cond1);
                        }
                        if (MatchToken("';'"))
                        {
                            ++TokenIndex;
                            var tmp = NORM_EXPR();
                            if (tmp != null)
                            {
                                ret.Step = new List<ASTNode>
                                {
                                    (tmp as ASTExpressionStatement).Expressions[0]
                                };
                            }
                            if (MatchToken("')'"))
                            {
                                ++TokenIndex;
                                ret.Stat = CONTENT_EXPR();
                                return ret;
                            }
                            //missing ')'
                            else
                            {
                                throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
                            }
                        }
                        //missing second ';'
                        else
                        {
                            throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
                        }
                    }
                    //missing first ';'
                    else
                    {
                        throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
                    }
                }
            }
            //missing '('
            else
            {
                throw new TokenMissingException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
            }
        }
        //TYPENAME -> double|float|int|char|bool|long|void
        private ASTToken TYPENAME()
        {
            var ret = new ASTToken();
            var ToAnalyze = Tokens[TokenIndex];
            switch (ToAnalyze.Value)
            {
                case "double":
                case "float":
                case "int":
                case "char":
                case "bool":
                case "long":
                case "void":
                    ret.TokenID = TokenIndex;
                    ret.Value = ToAnalyze.Value;
                    break;
                default://unsupported typename
                    throw new UnsupportedTypeNameException(Tokens[TokenIndex].Value, Tokens[TokenIndex].Line, Tokens[TokenIndex].Column);
            }
            ++TokenIndex;
            return ret;
        }
        public void Run(string arg)
        {
            var start = System.DateTime.Now;
            Console.WriteLine($"Reading \"{arg}\"...");
            if (LoadTokens(arg) != 0) return;
            Console.WriteLine("Generating AST...");
            ParseTokens(arg);
            var end = System.DateTime.Now;
            Console.WriteLine($"Done in {(end - start).TotalMilliseconds}ms.");
        }
    }
}
