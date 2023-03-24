namespace JanCLang.Misc
{
    internal enum AstType
    {
        Assignment,
        Block,
        Comparison,
        Conditional,
        Read,
        Write,
        Declaration,
        Expression,
        Literal,
        Operator,
        Parameter,
        FunctionCall,
        RegisteredMethodCall,
        Variable,
        While
    }

    // Translate for exceptions in your language
    internal enum DataType
    {
        Prazno,        // void
        Celo,          // int
        Decimalno,     // double
        Niz            // string
    }

    internal enum Operator
    {
        Plus,
        Minus,
        Multiplication,
        Division
    }

    internal enum TokenType
    {
        // Keywords
        Program,       // Program
        Function,      // funkcija
        Return,        // vrni
        Void,          // prazno
        String,        // niz
        Int,           // celo
        Double,        // decimalno
        OutputMethod,  // Izpisi
        InputMethod,   // Vnos
        If,            // ce
        Else,          // sicer
        While,         // dokler

        // Separators
        EOL,           // End Of Line
        L_Brack,
        R_Brack,
        L_Curly,
        R_Curly,
        Comma,

        // Operators
        Eql,
        Operator,
        Comparator,

        // Literals and identifiers
        Literal,
        Identifier,

        // Special
        EOF            // End Of File
    }

    internal enum Comparator
    {
        Eql,
        Neq,
        Lss,
        Leq,
        Gtr,
        Geq
    }
}
