using System;

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