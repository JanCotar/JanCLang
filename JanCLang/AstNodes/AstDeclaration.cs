using JanCLang.Misc;

namespace JanCLang.AstNodes
{
    // Ast node. Represents a declaration of a variable.
    internal class AstDeclaration : Ast
    {
        internal AstVariable Variable;

        internal AstDeclaration(int sourceCodeLine, string variableName, DataType variableType = DataType.Prazno)
        {
            AstType = AstType.Declaration;
            Variable = new AstVariable(sourceCodeLine, variableName, variableType);
        }
    }
}
