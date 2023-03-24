using JanCLang.Misc;
using System.Collections.Generic;

namespace JanCLang.AstNodes
{
    // Ast node. Represents an output method.
    internal class AstWrite : Ast
    {
        internal List<Ast> ToWrite = new List<Ast>();

        internal AstWrite(int sourceCodeLine)
        {
            AstType = AstType.Write;
            SourceCodeLine = sourceCodeLine;
        }
    }
}
