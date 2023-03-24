using JanCLang.AstNodes;
using System;

namespace JanCLang.Misc
{
    // Base exception
    internal class JanCException : Exception
    {
        public JanCException(string message) : base(message) { }
    }


    // Lexical analyzer exceptions
    internal class IdentifierTooLongException : JanCException
    {
        // (Eng. message) Error: Lexeme has too many characters. Allowed number of characters for a variable, function or registered method name is 200. Line: tokenLine.
        public IdentifierTooLongException(string lexeme, int tokenLine)
            : base("Napaka: '" + lexeme + "' ima preveč znakov. Največje dovoljeno število znakov za ime spremenljivke, funkcije ali registrirane metode je 200. Vrstica: " + tokenLine) { }
    }

    internal class IdentifierUnallowedCharacterException : JanCException
    {
        // (Eng. message) Error: Lexeme contains not allowed character character. Line: tokenLine.
        public IdentifierUnallowedCharacterException(string lexeme, char character, int tokenLine)
            : base("Napaka: '" + lexeme + "' vsebuje nedovoljeni znak '" + character + "'. Vrstica: " + tokenLine) { }
    }

    internal class IdentifierStartsWithNumberException : JanCException
    {
        // (Eng. message) Error: Lexeme cannot start with a number. Line: tokenLine.
        internal IdentifierStartsWithNumberException(string lexeme, int tokenLine)
            : base("Napaka: '" + lexeme + "' se ne sme začeti s številko. Vrstica: " + tokenLine) { }
    }

    internal class NumberZeroFormatException : JanCException
    {
        // (Eng. message) Error: Number lexeme is only allowed in the form of '0'. Line: tokenLine.
        public NumberZeroFormatException(string lexeme, int tokenLine)
            : base("Napaka: Število '" + lexeme + "' je dovoljeno samo v obliki '0'. Vrstica: " + tokenLine) { }
    }

    internal class IntegerNumberFormatException : JanCException
    {
        // (Eng. message) Error: Number lexeme cannot start with '0'. Line: tokenLine.
        public IntegerNumberFormatException(string lexeme, int tokenLine)
            : base("Napaka: Število '" + lexeme + "' se ne more začeti z '0'. Vrstica: " + tokenLine) { }
    }

    internal class DecimalNumberFormatException : JanCException
    {
        // (Eng. message) Error: Number lexeme cannot start with '0'. Line: tokenLine.
        public DecimalNumberFormatException(string lexeme, int tokenLine)
            : base("Napaka: Število '" + lexeme + "' se ne more začeti z '0'. Vrstica: " + tokenLine) { }
    }

    internal class DecimalNumberZeroFormatException : JanCException
    {
        // (Eng. message) Error: Number lexeme is only allowed in the form of '0.X'. Line: tokenLine.
        public DecimalNumberZeroFormatException(string lexeme, int tokenLine)
            : base("Napaka: Število '" + lexeme + "' je dovoljeno samo v obliki '0.X'. Vrstica: " + tokenLine) { }
    }


    // Syntax analyzer exceptions
    internal class UnexpectedTokenException : JanCException
    {
        // (Eng. message) Error: Unexpected symbol token.Value. Line: token.SourceCodeLine.
        internal UnexpectedTokenException(Token token)
            : base("Napaka: Nepričakovan simbol '" + token.Value + "'. Vrstica: " + token.SourceCodeLine) { }
    }

    internal class PredefinedFunctionException : JanCException
    {
        // (Eng. message) Error: Function or registered method named token.Value already exists. Line: token.SourceCodeLine.
        internal PredefinedFunctionException(Token token)
            : base("Napaka: Funkcija ali registrirana metoda z imenom '" + token.Value + "' že obstaja. Vrstica: " + token.SourceCodeLine) { }
    }

    internal class FunctionReturnException : JanCException
    {
        // (Eng. message) Error: Function functionName must return a value.
        internal FunctionReturnException(string functionName)
            : base("Napaka: Funkcija '" + functionName + "' mora vrniti vrednost.") { }
    }

    internal class FunctionReturnAtEndException : JanCException
    {
        // (Eng. message) Error: Function functionName can only return a value right before the end of it's declaration.
        internal FunctionReturnAtEndException(string functionName)
            : base("Napaka: Funkcija '" + functionName + "' lahko vrne vrednost samo neposredno pred zaključkom svoje definicije.") { }
    }


    // Semantic analyzer exceptions
    internal class UnregisteredReadException : JanCException
    {
        // (Eng. message) Error: Input method not registered. Line: sourceCodeLine.
        internal UnregisteredReadException(int sourceCodeLine)
            : base("Napaka: Metoda za vnos ni registrirana. Vrstica: " + sourceCodeLine) { }
    }

    internal class RegisteredMethodDoesNotReturnException : JanCException
    {
        // (Eng. message) Error: Registered method methodName does not return a value and it is not possible to use it in a value assignment. Line: sourceCodeLine.
        internal RegisteredMethodDoesNotReturnException(string methodName, int sourceCodeLine)
            : base("Napaka: Registrirana metoda '" + methodName + "' ne vrača vrednosti in je ni mogoče uporabiti za prirejanje. Vrstica: " + sourceCodeLine) { }
    }

    internal class UndeclaredVariableException : JanCException
    {
        // (Eng. message) Error: Variable variableName is not declared. Line: sourceCodeLine.
        internal UndeclaredVariableException(string variableName, int sourceCodeLine)
            : base("Napaka: Spremenljivka z imenom '" + variableName + "' ni deklarirana. Vrstica: " + sourceCodeLine) { }
    }

    internal class UninitializedVariableException : JanCException
    {
        // (Eng. message) Error: Variable variableName is not initialized. Line: sourceCodeLine.
        internal UninitializedVariableException(string variableName, int sourceCodeLine)
            : base("Napaka: Spremenljivka z imenom '" + variableName + "' ni inicializirana. Vrstica: " + sourceCodeLine) { }
    }

    internal class VariableNamedAsFunctionException : JanCException
    {
        // (Eng. message) Error: Variable variableName cannot be named as a function or registered method. Line: sourceCodeLine.
        internal VariableNamedAsFunctionException(string variableName, int sourceCodeLine)
            : base("Napaka: Spremenljivka z imenom '" + variableName + "' ne more imeti istega imena kot obstoječa funkcija ali registrirana metoda. Vrstica: " + sourceCodeLine) { }
    }

    internal class PredefinedVariableException : JanCException
    {
        // (Eng. message) Error: Variable variableName already exists. Line: sourceCodeLine.
        internal PredefinedVariableException(string variableName, int sourceCodeLine)
            : base("Napaka: Spremenljivka z imenom '" + variableName + "' že obstaja. Vrstica: " + sourceCodeLine) { }
    }

    internal class LiteralParameterIncompatibilityException : JanCException
    {
        // (Eng. message) Error: Value cannot be assigned to type parameter.Type of parameter parameter.Name. Line: sourceCodeLine.
        internal LiteralParameterIncompatibilityException(object value, AstParameter parameter, int sourceCodeLine)
            : base("Napaka: Vrednosti '" + value + "' ne morem prirediti tipu " + parameter.Type + " parametra '" + parameter.Name + "', ker je v napačni obliki. Vrstica: " + sourceCodeLine) { }

        // (Eng. message) Error: (parameterIndex + 1). parameter of type parameterType of registered method method.Name cannot be assigned a value. Line: sourceCodeLine.
        internal LiteralParameterIncompatibilityException(int parameterIndex, object value, AstRegisteredMethodCall method, DataType parameterType, int sourceCodeLine)
            : base("Napaka: " + (parameterIndex + 1) + ". parametru tipa " + parameterType + " v registrirani metodi '" + method.Name + "' ne morem prirediti vrednosti '" + value + "', ker je v napačni obliki. Vrstica: " + sourceCodeLine) { }
    }

    internal class VariableParameterIncompatibilityException : JanCException
    {
        // (Eng. message) Error: Type variable.Type of variable variable.Name does not match type parameter.Type of parameter parameter.Name. Line: sourceCodeLine.
        internal VariableParameterIncompatibilityException(AstVariable variable, AstParameter parameter, int sourceCodeLine)
            : base("Napaka: Tip " + variable.Type + " spremenljivke '" + variable.Name + "' se ne ujema s tipom " + parameter.Type + " parametra '" + parameter.Name + "'. Vrstica: " + sourceCodeLine) { }

        // (Eng. message) Error: (parameterIndex + 1). parameter of type parameterType of registered method method.Name cannot be assigned a value of variable variable.Name. Line: sourceCodeLine.
        internal VariableParameterIncompatibilityException(int parameterIndex, AstVariable variable, AstRegisteredMethodCall method, DataType parameterType, int sourceCodeLine)
            : base("Napaka: " + (parameterIndex + 1) + ". parametru tipa " + parameterType + " v registrirani metodi '" + method.Name + "' ne morem prirediti spremenljivke '" + variable.Name + "', ker je v napačni obliki. Vrstica: " + sourceCodeLine) { }
    }

    internal class StringConcatenationException : JanCException
    {
        // (Eng. message) Error: For string concatenation use +. Line: sourceCodeLine.
        internal StringConcatenationException(int sourceCodeLine)
            : base("Napaka: Za združevanje nizov uporabite znak '+'. Vrstica: " + sourceCodeLine) { }
    }

    internal class IncomparableValuesException : JanCException
    {
        // (Eng. message) Error: Type left and type right cannot be compared. Line: sourceCodeLine.
        internal IncomparableValuesException(object left, object right, int sourceCodeLine)
            : base("Napaka: Tipov " + left + " in " + right + " ne morem primerjati. Vrstica: " + sourceCodeLine) { }
    }

    internal class FunctionDoesNotReturnException : JanCException
    {
        // (Eng. message) Error: Function functionName does not return a value and it is not possible to use it in a value assignment. Line: sourceCodeLine.
        internal FunctionDoesNotReturnException(string functionName, int sourceCodeLine)
            : base("Napaka: Funkcija '" + functionName + "' ne vrača vrednosti in je ni mogoče uporabiti za prirejanje. Vrstica: " + sourceCodeLine) { }
    }

    internal class FunctionReturnTypeException : JanCException
    {
        // (Eng. message) Error: Function function.Name must return a value of type function.ReturnType.
        internal FunctionReturnTypeException(Function function)
            : base("Napaka: Funkcija '" + function.Name + "' mora vrniti vrednost tipa " + function.ReturnType + ".") { }
    }


    // Interpreter exceptions
    internal class RegisteredMethodExternalException : JanCException
    {
        // (Eng. message) Error: Registered method registeredMethodName threw an exception. Check registered method or registered method call. Line: sourceCodeLine Source exception: originalMessage.
        internal RegisteredMethodExternalException(string registeredMethodName, string originalMessage, int sourceCodeLine)
            : base("Napaka: Registirana metoda '" + registeredMethodName + "' je vrnila napako. Preverite metodo ali klic metode. Vrstica: " + sourceCodeLine + " Izvorna napaka: " + originalMessage) { }
    }

    internal class AssignValueException : JanCException
    {
        // (Eng. message) Error: Variable variableName cannot be assigned to a variable of type variableType. Line: sourceCodeLine.
        internal AssignValueException(string variableName, DataType variableType, int sourceCodeLine)
            : base("Napaka: Spremenljivke '" + variableName + "' ne morem prirediti spremenljivki tipa " + variableType + ", ker je v napačni obliki. Vrstica: " + sourceCodeLine) { }

        // (Eng. message) Error: Value cannot be assigned to a variable of type variableType. Line: sourceCodeLine.
        internal AssignValueException(object value, DataType variableType, int sourceCodeLine)
            : base("Napaka: Vrednosti '" + value + "' ne morem prirediti spremenljivki tipa " + variableType + ", ker je v napačni obliki. Vrstica: " + sourceCodeLine) { }

        // (Eng. message) Error: Function function.Name returns a value of type function.ReturnType that is not possible to assign to a type assignToTYpe. Line: sourceCodeLine.
        internal AssignValueException(AstFunctionCall function, DataType assignToTYpe, int sourceCodeLine)
            : base("Napaka: Funkcija '" + function.Name + "' vrača vrednost tipa " + function.ReturnType + ", ki je ne morem prirediti tipu " + assignToTYpe + ". Vrstica: " + sourceCodeLine) { }

        // (Eng. message) Error: Registered method registeredMethod.Name returns a value of type registeredMethod.ReturnType that is not possible to assign to a type assignToType. Line: sourceCodeLine.
        internal AssignValueException(RegisteredMethod registeredMethod, DataType assignToType, int sourceCodeLine)
            : base("Napaka: Registrirana metoda '" + registeredMethod.Name + "' vrača vrednost tipa " + registeredMethod.ReturnType + ", ki je ne morem prirediti tipu " + assignToType + ". Vrstica: " + sourceCodeLine) { }
    }

    internal class CanNotCalculateException : JanCException
    {
        // (Eng. message) Error: Cannot calculate. Line: sourceCodeLine.
        internal CanNotCalculateException(int sourceCodeLine)
            : base("Napaka: Ne morem izračunati vrednosti. Vrstica: " + sourceCodeLine) { }
    }

    internal class DivisionByZeroException : JanCException
    {
        // (Eng. message) Error: Cannont devide by 0. Line: sourceCodeLine.
        internal DivisionByZeroException(int sourceCodeLine)
            : base("Napaka: Ne morem deliti z 0. Vrstica: " + sourceCodeLine) { }
    }


    // Misc exceptions
    internal class RegisteredMethodParameterTypeException : JanCException
    {
        // (Eng. message) Error: Only methods with int, double and string data type parameters can be registered (JanC.RegisterMethod). Method: methodName.
        internal RegisteredMethodParameterTypeException(string methodName)
            : base("Napaka: Pri ustvarjanju registriranih metod (JanC.RegisterMethod) lahko podajate samo delegate z int, double ali string podatkovnim tipom. Metoda: " + methodName) { }
    }

    internal class RegisteredMethodReturnTypeException : JanCException
    {
        // (Eng. message) Error: Only methods with int, double and string data type return value can be registered (JanC.RegisterMethod). Method: methodName.
        internal RegisteredMethodReturnTypeException(string methodName)
            : base("Napaka: Pri ustvarjanju registriranih metod (JanC.RegisterMethod) lahko podajate samo delegate, ki vračajo int, double ali string podatkovni tip. Metoda: " + methodName) { }
    }

    internal class PredefinedMethodException : JanCException
    {
        // (Eng. message) Error: Method methodName already exists.
        internal PredefinedMethodException(string methodName)
            : base("Napaka: Metoda z imenom '" + methodName + "' že obstaja.") { }
    }

    internal class MethodNameEmptyException : JanCException
    {
        // (Eng. message) Error: Registered method must have a name.
        public MethodNameEmptyException()
            : base("Napaka: Registrirana metoda mora imeti ime.") { }
    }

    internal class MethodNameNotAllowedException : JanCException
    {
        // (Eng. message) Error: Registered method methodName cannot be named as a keyword or reserved symbol.
        public MethodNameNotAllowedException(string methodName)
            : base("Napaka: Registrirana metoda '" + methodName + "' ne more biti poimenovana kot rezervirana beseda oz. simbol.") { }
    }

    internal class MethodNameUnallowedCharacterException : JanCException
    {
        // (Eng. message) Error: Registered method methodName contains not allowed character character.
        public MethodNameUnallowedCharacterException(string methodName, char character)
            : base("Napaka: Registrirana metoda '" + methodName + "' vsebuje nedovoljeni znak '" + character + "'.") { }
    }

    internal class UnknownException : Exception
    {
        // (Eng. message) Error: We are sorry but an unexpected exception occurred. Please tell the creator about it. Source exception: originalMessage
        internal UnknownException(string originalMessage)
            : base("Napaka: Oprostite, zgodila se je nepredvidena napaka. Prosim, stopite v kontakt z ustvarjalcem in mu sporočite kako je do napake prišlo. Izvorna napaka: " + originalMessage) { }
    }
}
