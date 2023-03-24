using System.Collections.Generic;

namespace JanCLang.Misc
{
    // Used by Parser for storing function block tokens and parsing them after first registering all the functions. Enables recursion.
    internal class IntermediateFunction
    {
        internal string FunctionName;
        internal bool Returns;
        internal Queue<Token> FunctionTokens = new Queue<Token>();

        internal IntermediateFunction(string functionName, bool returns)
        {
            FunctionName = functionName;
            Returns = returns;
        }

        internal void SaveToken(Token token)
        {
            FunctionTokens.Enqueue(token);
        }
    }
}
