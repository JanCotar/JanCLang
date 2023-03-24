using JanCLang.Misc;

namespace JanCLang.AstNodes
{
    // Ast node. Represents a variable.
    internal class AstVariable : Ast
    {
        internal DataType Type;
        internal string Name;
        internal object Value;

        internal AstVariable(int sourceCodeLine, string variableName, DataType variableType = DataType.Prazno)
        {
            AstType = AstType.Variable;
            SourceCodeLine = sourceCodeLine;
            Name = variableName;
            Type = variableType;
        }

        internal AstVariable(int sourceCodeLine, string variableName, object variableValue, DataType variableType = DataType.Prazno)
        {
            AstType = AstType.Variable;
            SourceCodeLine = sourceCodeLine;
            Name = variableName;
            Value = variableValue;
            Type = variableType;
        }

        internal AstVariable ShallowCopy()
        {
            return (AstVariable)MemberwiseClone();
        }
    }
}
