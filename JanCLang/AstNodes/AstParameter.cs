using JanCLang.Misc;

namespace JanCLang.AstNodes
{
    // Ast node. Represents a user defined function parameter.
    internal class AstParameter : Ast
    {
        internal DataType Type;
        internal string Name;
        internal Ast Value;

        internal AstParameter(int sourceCodeLine, string parameterName, DataType parameterType)
        {
            AstType = AstType.Parameter;
            SourceCodeLine = sourceCodeLine;
            Name = parameterName;
            Type = parameterType;
        }

        internal AstParameter ShallowCopy()
        {
            return (AstParameter)MemberwiseClone();
        }
    }
}
