using JanCLang.AstNodes;
using JanCLang.InterpreterMachine;
using JanCLang.Misc;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace JanCLang
{
    /// <summary>
    /// Provides methods for running JanC source code, defining builtin methods for write (Izpisi) and read (Vnos) and registering C# delegates into the JanC environment.
    /// </summary>
    public class JanC
    {
        private SymbolTable SyT = new SymbolTable();
        private HashSet<Type> _validDataTypes = new HashSet<Type> { typeof(int), typeof(double), typeof(string) };
        private Queue<string> _lexemes = new Queue<string>();

        /// <summary>
        /// Method for registering JanC 'Izpisi' output action. Default is empty action.
        /// </summary>
        public void RegisterWrite(Action<string> action)
        {
            SyT.Write = action;
        }

        /// <summary>
        /// Method for registering JanC 'Vnos' input function. If not registered, use of 'Vnos' keyword throws an exception.
        /// </summary>
        public void RegisterRead(Func<string> function)
        {
            SyT.Read = function;
        }

        /// <summary>
        /// Method for registering an arbitrary action or function. Only int, double and string input/output data types are valid.
        /// </summary>
        public void RegisterMethod(string methodName, Delegate method)
        {
            if (methodName == null || methodName == "")
            {
                throw new MethodNameEmptyException();
            }

            if (SyT.ReservedSymbols.ContainsKey(methodName))
            {
                throw new MethodNameNotAllowedException(methodName);
            }

            if (SyT.RegisteredMethods.ContainsKey(methodName))
            {
                throw new PredefinedMethodException(methodName);
            }

            foreach (char c in methodName)
            {
                if (!SyT.allowedChar.Contains(char.ToLower(c)))
                {
                    throw new MethodNameUnallowedCharacterException(methodName, c);
                }
            }

            foreach (ParameterInfo parameter in method.Method.GetParameters())
            {
                Type pType = parameter.ParameterType;

                if (!_validDataTypes.Contains(pType))
                {
                    throw new RegisteredMethodParameterTypeException(methodName);
                }
            }

            Type rType = method.Method.ReturnType;

            if (!_validDataTypes.Contains(rType) && !rType.Equals(typeof(void)))
            {
                throw new RegisteredMethodReturnTypeException(methodName);
            }

            SyT.RegisteredMethods.Add(methodName, new RegisteredMethod(methodName, method));
        }

        /// <summary>
        /// Interprets and runs JanC source code.
        /// </summary>
        public void Run(string sourceCode)
        {
            try
            {
                var lexicalAnalyzer = new LexicalAnalyzer();
                Queue<Token> tokens = lexicalAnalyzer.RunLexicalAnalyzer(_lexemes, sourceCode, SyT);

                var syntaxAnalyzer = new SyntaxAnalyzer();
                AstBlock programAST = syntaxAnalyzer.RunParser(tokens, SyT);

                var semanticAnalyzer = new SemanticAnalyzer();
                semanticAnalyzer.RunSemanticAnalyser(programAST, SyT);

                var interpreter = new Interpreter();
                interpreter.RunInterpreter(programAST, SyT);
            }
            catch (JanCException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new UnknownException(e.Message);
            }
        }
    }
}
