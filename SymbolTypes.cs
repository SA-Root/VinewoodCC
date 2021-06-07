using System;
using System.Collections.Generic;

namespace VinewoodCC
{
    namespace Semantic
    {
        public class AOTCheckExtraArg
        {
            public LinkedList<STLoopItem> Loops { get; set; }
            public string VType { get; set; }
            public Boolean isInLoop { get; set; }
            public Stack<int> MultiDimArray { get; set; }
            public AOTCheckExtraArg()
            {
                Loops = new LinkedList<STLoopItem>();
                VType = null;
                isInLoop = false;
                MultiDimArray = null;
            }
        }
        public enum SymbolType
        {
            Variable,
            Array,
            Function,
            Loop
        }
        public abstract class STItem
        {
            public string Identifier { get; set; }
            public SymbolType STType { get; set; }
        }
        public class STLoopItem : STItem
        {
            //Identifier means loop-var
            public Dictionary<string, STItem> LPT { get; set; }
            public STLoopItem(string lvar, Dictionary<string, STItem> lpt)
            {
                Identifier = lvar;
                LPT = lpt;
                STType = SymbolType.Loop;
            }
            public STLoopItem() { }
        }
        public class STVariableItem : STItem
        {
            public string ValueType { get; set; }
            public STVariableItem(string id, string vtype)
            {
                Identifier = id;
                STType = SymbolType.Variable;
                ValueType = vtype;
            }
            public STVariableItem() { }
        }
        public class STArrayItem : STItem
        {
            public List<int> Dimensions { get; set; }
            public string ValueType { get; set; }
            public STArrayItem(string id, List<int> dims, string vtype)
            {
                Identifier = id;
                STType = SymbolType.Array;
                Dimensions = dims;
                ValueType = vtype;
            }
            public STArrayItem() { }
        }
        public class STFunctionItem : STItem
        {
            public List<string> ArgTypes { get; set; }
            public string RetType { get; set; }
            public Dictionary<string, STItem> LST { get; set; }
            public STFunctionItem(string id, string rett, List<string> argt, Dictionary<string, STItem> lst)
            {
                Identifier = id;
                STType = SymbolType.Function;
                RetType = rett;
                ArgTypes = argt;
                LST = lst;
            }
            public STFunctionItem() { }
        }
    }

}