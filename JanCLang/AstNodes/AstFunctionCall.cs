using JanCLang.Misc;
using System.Collections.Generic;

namespace JanCLang.AstNodes
{
    // Ast node. Represents a call of a user defined function.
    internal class AstFunctionCall : Ast
    {
        internal string Name;
        internal List<AstParameter> Parameters = new List<AstParameter>();
        internal DataType ReturnType;

        internal AstFunctionCall(int sourceCodeLine, string functionName, DataType returnType)
        {
            AstType = AstType.FunctionCall;
            SourceCodeLine = sourceCodeLine;
            Name = functionName;
            ReturnType = returnType;
        }

        internal AstFunctionCall DeepCopy()
        {
            var copy = (AstFunctionCall)MemberwiseClone();

            copy.Parameters = new List<AstParameter>();
            foreach (AstParameter parameter in Parameters)
            {
                copy.Parameters.Add(parameter.ShallowCopy());
            }

            return copy;
        }
    }
}
