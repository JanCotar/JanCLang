using System;
using System.Collections.Generic;
using System.Reflection;

namespace JanCLang.Misc
{
    // Holds a registered method information.
    internal class RegisteredMethod
    {
        internal string Name;
        internal Delegate Method;
        internal List<DataType> ParametersTypes = new List<DataType>();
        internal DataType ReturnType;
        internal bool Returns = true;

        private Dictionary<Type, DataType> _dataTypesMapping = new Dictionary<Type, DataType>
        {
            { typeof(int), DataType.Celo },
            { typeof(double), DataType.Decimalno },
            { typeof(string), DataType.Niz }
        };

        internal RegisteredMethod(string methodName, Delegate method)
        {
            Name = methodName;
            Method = method;

            foreach (ParameterInfo parameter in method.Method.GetParameters())
            {
                Type pType = parameter.ParameterType;
                ParametersTypes.Add(_dataTypesMapping[pType]);
            }

            Type rType = method.Method.ReturnType;
            if (_dataTypesMapping.ContainsKey(rType))
            {
                ReturnType = _dataTypesMapping[rType];
            }
            else
            {
                Returns = false;
            }
        }
    }
}
