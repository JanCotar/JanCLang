using JanCLang.Misc;

namespace JanCLang.AstNodes
{
    // Ast node. Represents an input method that assigns a value to a variable.
    internal class AstRead : Ast
    {
        internal AstVariable Variable;

        internal AstRead(int sourceCodeLine, string variableName, DataType variableType = DataType.Prazno)
        {
            AstType = AstType.Read;
            SourceCodeLine = sourceCodeLine;
            Variable = new AstVariable(sourceCodeLine, variableName, variableType);
        }
    }
}
