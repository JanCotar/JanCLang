using JanCLang.Misc;
using System.Collections.Generic;

namespace JanCLang.AstNodes
{
    // Ast node. Represents a call of a registered method.
    internal class AstRegisteredMethodCall : Ast
    {
        internal string Name;
        internal List<Ast> PassedValues = new List<Ast>();

        internal AstRegisteredMethodCall(int sourceCodeLine, string methodName)
        {
            AstType = AstType.RegisteredMethodCall;
            SourceCodeLine = sourceCodeLine;
            Name = methodName;
        }

        internal AstRegisteredMethodCall DeepCopy()
        {
            return (AstRegisteredMethodCall)MemberwiseClone();
        }
    }
}
