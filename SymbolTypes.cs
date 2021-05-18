using System;
using System.Collections.Generic;

namespace VinewoodCC
{
    public enum AOTCheckExtraArg
    {
        isInLoop
    }
    public enum SymbolType
    {
        Variable,
        Array,
        Function
    }
    public abstract class STItem
    {
        public string Identifier { get; set; }
        public SymbolType STType { get; set; }
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
        public STArrayItem(string id, List<int> dims)
        {
            Identifier = id;
            STType = SymbolType.Array;
            Dimensions = dims;
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