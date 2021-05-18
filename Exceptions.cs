using System;

namespace VinewoodCC
{
    public class SemanticErrors
    {
        public static readonly string VCE0001 = "[ERROR]VCE0001: Variable '{0}' already defined";
        public static readonly string VCE0002 = "[ERROR]VCE0002: Variable '{0}' not defined";
        public static readonly string VCE0003 = "[ERROR]VCE0003: 'break' statement not in loop";
        public static readonly string VCE0004 = "[ERROR]VCE0004: Function '{0}' not defined";
        public static readonly string VCE0005 = "[ERROR]VCE0005: '{0}' is not a function";
        public static readonly string VCE0006 = "[ERROR]VCE0006: '{0}' is not a variable or array";
    }
}

namespace ParserSharp
{
    class UnsupportedSyntaticException : Exception
    {
        public UnsupportedSyntaticException(string token, int line, int column) : base()
        {
            Console.WriteLine($"[ERROR]'{token}' unsupported at Line {line},Column {column}");
        }
    }
    class TokenMissingException : Exception
    {
        public TokenMissingException(string token, int line, int column) : base()
        {
            Console.WriteLine($"[ERROR]'{token}' missing at Line {line},Column {column}");
        }
    }
    class UnreachableStateException : Exception
    {
        public UnreachableStateException() : base() { }
    }
    class UnsupportedTypeNameException : UnsupportedSyntaticException
    {
        public UnsupportedTypeNameException(string token, int line, int column) : base(token, line, column) { }
    }
    class VariableDeclarationMissingIdentifierException : TokenMissingException
    {
        public VariableDeclarationMissingIdentifierException(string token, int line, int column) : base(token, line, column) { }
    }
}