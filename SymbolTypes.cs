using System;
using System.Collections.Generic;

namespace VinewoodCC
{
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
        public string Value { get; set; }
        public STVariableItem(string id, SymbolType st, string val)
        {
            Identifier = id;
            STType = st;
            Value = val;
        }
    }
    public class STArrayItem : STItem
    {
        public List<int> Dimensions { get; set; }
        public STArrayItem(string id, SymbolType st, List<int> dims)
        {
            Identifier = id;
            STType = st;
            Dimensions = dims;
        }
    }
    public class STFunctionItem : STItem
    {
        public List<string> ArgTypes { get; set; }
        public STFunctionItem(string id, SymbolType st, List<string> argt)
        {
            Identifier = id;
            STType = st;
            ArgTypes = argt;
        }
    }
}