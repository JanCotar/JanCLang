using JanCLang.Misc;
using System;

namespace JanCLang.AstNodes
{
    // Ast node. Represents a literal.
    internal class AstLiteral : Ast
    {
        internal DataType Type;
        internal object Value;

        internal AstLiteral(int sourceCodeLine, string value)
        {
            AstType = AstType.Literal;
            SourceCodeLine = sourceCodeLine;

            if (value.StartsWith("\""))
            {
                Type = DataType.Niz;
                Value = value.Substring(1, value.Length - 2);
            }
            else if (int.TryParse(value, out int x))
            {
                Type = DataType.Celo;
                Value = x;
            }
            else
            {
                Type = DataType.Decimalno;
                Value = Convert.ToDouble(value);
            }
        }

        internal AstLiteral ShallowCopy()
        {
            return (AstLiteral)MemberwiseClone();
        }
    }
}
