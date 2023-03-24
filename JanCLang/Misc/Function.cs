using JanCLang.AstNodes;
using System.Collections.Generic;

namespace JanCLang.Misc
{
    // Holds a user defined function information.
    internal class Function
    {
        internal string Name;
        internal List<AstParameter> Parameters = new List<AstParameter>();
        internal AstBlock Block;
        internal bool Returns;
        internal DataType ReturnType;
        internal Ast ReturnAst;

        internal Function(string functionName, DataType returnType)
        {
            Name = functionName;
            ReturnType = returnType;
            Returns = ReturnType == DataType.Prazno ? false : true;
        }

        internal Function DeepCopy()
        {
            var copy = (Function)MemberwiseClone();

            copy.Parameters = new List<AstParameter>();
            foreach (AstParameter parameter in Parameters)
            {
                copy.Parameters.Add(parameter.ShallowCopy());
            }

            if (ReturnType == DataType.Prazno)
            {
                return copy;
            }

            switch (ReturnAst.AstType)
            {
                case AstType.Literal:
                    copy.ReturnAst = (ReturnAst as AstLiteral).ShallowCopy();
                    break;
                case AstType.Variable:
                    copy.ReturnAst = (ReturnAst as AstVariable).ShallowCopy();
                    break;
            }

            return copy;
        }

        internal List<AstParameter> ParametersDeepCopy()
        {
            var parCopy = new List<AstParameter>();

            foreach (AstParameter parameter in Parameters)
            {
                parCopy.Add(parameter.ShallowCopy());
            }

            return parCopy;
        }
    }
}
