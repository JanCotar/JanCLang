using JanCLang.AstNodes;
using JanCLang.Misc;
using System;
using System.Collections.Generic;

namespace JanCLang.InterpreterMachine
{
    // Stores information that all the processes for interpreting share.
    internal class SymbolTable
    {
        internal Action<string> Write = _ => { };
        internal Func<string> Read;
        internal Dictionary<string, RegisteredMethod> RegisteredMethods = new Dictionary<string, RegisteredMethod>();
        internal Dictionary<string, Function> Functions = new Dictionary<string, Function>();

        // Stack of variables used to define their scope. Program and each user function uses it's own element in the stack.
        // Start of the program creates the program scope and each user function call pushes it's scope to the top of the stack. After function exits, the scope is removed.
        internal Stack<Dictionary<string, AstVariable>> Variables = new Stack<Dictionary<string, AstVariable>>();

        // Allowed characters for identifiers.
        internal HashSet<char> allowedChar = new HashSet<char> {
              '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
              'a', 'b', 'c', 'č', 'd', 'e', 'f', 'g', 'h', 'i',
              'j', 'k', 'l', 'm', 'n', 'o', 'p', 'r', 's', 'š',
              't', 'u', 'v', 'z', 'ž', 'q', 'w', 'x', 'y', '_'
            };

        // Reserved symbols are used for registering methods and lexical analysis.
        internal Dictionary<string, (TokenType type, int operatorPrecedence)> ReservedSymbols = new Dictionary<string, (TokenType, int)> {
                // Keywords
                { "Program", (TokenType.Program, -1) },
                { "funkcija", (TokenType.Function, -1) },
                { "vrni", (TokenType.Return, -1) },
                { "prazno", (TokenType.Void, -1) },
                { "niz", (TokenType.String, -1) },
                { "celo", (TokenType.Int, -1) },
                { "decimalno", (TokenType.Double, -1) },
                { "Izpiši", (TokenType.OutputMethod, -1) },
                { "Vnos", (TokenType.InputMethod, -1) },
                { "če", (TokenType.If, -1) },
                { "sicer", (TokenType.Else, -1) },
                { "dokler", (TokenType.While, -1) },

                // Separators
                { ";", (TokenType.EOL, -1) },
                { "(", (TokenType.L_Brack, -1) },
                { ")", (TokenType.R_Brack, -1) },
                { "{", (TokenType.L_Curly, -1) },
                { "}", (TokenType.R_Curly, -1) },
                { ",", (TokenType.Comma, -1) },

                // Operators and comparators
                { "=", (TokenType.Eql, -1) },
                { "+", (TokenType.Operator, 0) },
                { "-", (TokenType.Operator, 0) },
                { "*", (TokenType.Operator, 1) },
                { "/", (TokenType.Operator, 1) },
                { "==", (TokenType.Comparator, -1) },
                { "!=", (TokenType.Comparator, -1) },
                { "<", (TokenType.Comparator, -1) },
                { ">", (TokenType.Comparator, -1) },
                { "<=", (TokenType.Comparator, -1) },
                { ">=", (TokenType.Comparator, -1) },
        };
    }
}
