using JanCLang.AstNodes;
using JanCLang.Misc;
using System.Collections.Generic;
using System.Linq;

namespace JanCLang.InterpreterMachine
{
    // Builds an Abstract Syntax Tree out of tokens that Lexer creates. 
    internal class SyntaxAnalyzer
    {
        private Token _token;
        private Queue<Token> _tokenQueue = new Queue<Token>();
        // Storage for original token queue while parsing functions.
        private Queue<Token> _tokenQueueTmp = new Queue<Token>();
        private SymbolTable SyT;

        // Storage used while parsing functions.
        private List<IntermediateFunction> _intermediateFunctionStorage = new List<IntermediateFunction>();
        // Function name whose block is beeing parsed.
        private string _currFunctionName;
        // Determines if a user function has to return (vrni).
        private bool _funcHasToReturn = false;
        // User function can only return (vrni) at the very end of it's most outer block.
        // Created blocks are counted and if _blockCount != 1 when 'vrni' keyword is encountered, parser throws an exception.
        private int _blockCount = 0;

        // Most left expression can start with '+' or '-'. Operands coming after can only be binary.
        // After the first expression the value is set to false.
        private bool _firstExpression = true;
        // Registered method whose return value is beeing calculated in expression.
        private AstRegisteredMethodCall _methodToAssign;
        // Function whose return value is beeing calculated in expression.
        private AstFunctionCall _functionToAssign;
        // For building AST out of expression. If operand cannot be placed in a node, it is carried to another node.
        private Ast _operandToCarry = null;



        // ENTRY POINT
        // If there are any functions defined, they are registered first and their block tokens are stored
        // in the IntermediateFunction object. After registering all the functions, stored tokens are parsed.
        // Program is parsed last.
        internal AstBlock RunParser(Queue<Token> tokens, SymbolTable syt)
        {
            _tokenQueue = tokens;
            SyT = syt;

            while (PeekToken(TokenType.Function))
            {
                RegisterFunction(true);
            }

            _tokenQueueTmp = _tokenQueue;
            ParseFunctions();
            _tokenQueue = _tokenQueueTmp;

            NextToken();
            if (_token.Type != TokenType.Program)
            {
                throw new UnexpectedTokenException(_token);
            }
            NextToken();

            AstBlock program = Block();
            ExpectToken(TokenType.EOF);

            return program;
        }

        private void RegisterFunction(bool firstParse)
        {
            string functionName;
            bool returns = false;

            DataType returnType = ExpectAndReturnDataTypeToken(true);
            if (returnType != DataType.Prazno)
            {
                returns = true;
            }

            ExpectToken(TokenType.Identifier);
            IsFunctionPredefined();

            functionName = _token.Value;
            var functionToRegister = new Function(functionName, returnType);
            var intermediateFunction = new IntermediateFunction(functionName, returns);

            ExpectToken(TokenType.L_Brack);

            if (!PeekToken(TokenType.R_Brack))
            {
                while (true)
                {
                    DataType parameterType = ExpectAndReturnDataTypeToken(false);

                    ExpectToken(TokenType.Identifier);
                    functionToRegister.Parameters.Add(new AstParameter(_token.SourceCodeLine, _token.Value, parameterType));

                    if (!PeekToken(TokenType.R_Brack))
                    {
                        ExpectToken(TokenType.Comma);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            NextToken();

            AllocateTokes(intermediateFunction);
            _intermediateFunctionStorage.Add(intermediateFunction);
            SyT.Functions.Add(functionName, functionToRegister);

        }

        private void AllocateTokes(IntermediateFunction function)
        {
            function.SaveToken(_token);

            int blockCount = 1;
            while (blockCount > 0)
            {
                NextToken();
                function.SaveToken(_token);
                switch (_token.Type)
                {
                    case TokenType.L_Curly:
                        blockCount++;
                        break;
                    case TokenType.R_Curly:
                        blockCount--;
                        break;
                }
            }
        }

        private void ParseFunctions()
        {
            foreach (var function in _intermediateFunctionStorage)
            {
                _tokenQueue = function.FunctionTokens;
                _funcHasToReturn = function.Returns;
                _currFunctionName = function.FunctionName;
                NextToken();
                SyT.Functions[function.FunctionName].Block = Block();

                if (_funcHasToReturn)
                {
                    throw new FunctionReturnException(function.FunctionName);
                }
            }
        }

        private AstBlock Block()
        {
            if (_token.Type != TokenType.L_Curly)
            {
                throw new UnexpectedTokenException(_token);
            }

            var block = new AstBlock();
            ParseBlock(block);

            return block;
        }

        private void ParseBlock(AstBlock block)
        {
            bool emptyBlock = false;
            bool functionReturn = false;
            string variableName;

            _blockCount++;

            do
            {
                NextToken();

                switch (_token.Type)
                {
                    case TokenType.Int:
                        MakeDeclarationNode(block, DataType.Celo);
                        variableName = _token.Value;
                        if (PeekToken(TokenType.Eql)) MakeAssignmentNode(block, variableName);
                        ExpectToken(TokenType.EOL);
                        break;

                    case TokenType.Double:
                        MakeDeclarationNode(block, DataType.Decimalno);
                        variableName = _token.Value;
                        if (PeekToken(TokenType.Eql)) MakeAssignmentNode(block, variableName);
                        ExpectToken(TokenType.EOL);
                        break;

                    case TokenType.String:
                        MakeDeclarationNode(block, DataType.Niz);
                        variableName = _token.Value;
                        if (PeekToken(TokenType.Eql)) MakeAssignmentNode(block, variableName);
                        ExpectToken(TokenType.EOL);
                        break;

                    case TokenType.Identifier:
                        if (IsFunction()) MakeFunctionCallNode(block);
                        else if (IsRegisteredMethod()) MakeRegisteredMethodNode(block);
                        else MakeAssignmentNode(block, _token.Value, true);
                        ExpectToken(TokenType.EOL);
                        break;

                    case TokenType.OutputMethod:
                        MakeWriteNode(block);
                        ExpectToken(TokenType.EOL);
                        break;

                    case TokenType.InputMethod:
                        MakeReadNode(block);
                        ExpectToken(TokenType.EOL);
                        break;

                    case TokenType.If:
                        MakeConditionalNode(block);
                        break;

                    case TokenType.While:
                        MakeWhileNode(block);
                        break;

                    case TokenType.Return:
                        SetFunctionReturn();
                        ExpectToken(TokenType.EOL);
                        functionReturn = true;
                        break;

                    // Block can be empty only if it is a part of a language construct (funkcija, Program, ce, sicer and dokler).
                    case TokenType.R_Curly:
                        emptyBlock = true;
                        break;

                    default:
                        throw new UnexpectedTokenException(_token);
                }

                if (emptyBlock)
                {
                    break;
                }

                if (functionReturn)
                {
                    ExpectToken(TokenType.R_Curly);

                    if (_blockCount != 1)
                    {
                        throw new FunctionReturnAtEndException(_currFunctionName);
                    }

                    break;
                }

            } while (!PeekToken(TokenType.R_Curly));

            _blockCount--;
        }

        private void MakeDeclarationNode(AstBlock block, DataType dataType)
        {
            ExpectToken(TokenType.Identifier);
            block.Statements.Add(new AstDeclaration(_token.SourceCodeLine, _token.Value, dataType));
        }

        private void MakeFunctionCallNode(AstBlock block, bool isAssigned = false)
        {
            Function function = SyT.Functions[_token.Value];//_functions[_token.Value];
            var functionCall = new AstFunctionCall(_token.SourceCodeLine, _token.Value, function.ReturnType);

            ExpectToken(TokenType.L_Brack);

            if (function.Parameters.Count == 0)
            {
                ExpectToken(TokenType.R_Brack);
            }
            else
            {
                functionCall.Parameters = function.ParametersDeepCopy();

                for (var pIndex = 0; pIndex < function.Parameters.Count; pIndex++)
                {
                    NextToken();

                    switch (_token.Type)
                    {
                        case TokenType.Literal:
                            functionCall.Parameters[pIndex].Value = new AstLiteral(_token.SourceCodeLine, _token.Value);
                            break;

                        case TokenType.Identifier:
                            functionCall.Parameters[pIndex].Value = new AstVariable(_token.SourceCodeLine, _token.Value);
                            break;

                        default:
                            throw new UnexpectedTokenException(_token);
                    }

                    if (pIndex == function.Parameters.Count - 1)
                    {
                        ExpectToken(TokenType.R_Brack);
                    }
                    else
                    {
                        ExpectToken(TokenType.Comma);
                    }
                }
            }

            if (isAssigned)
            {
                _functionToAssign = functionCall;
            }
            else
            {
                block.Statements.Add(functionCall);
            }

        }

        private void MakeRegisteredMethodNode(AstBlock block, bool isAssigned = false)
        {
            var rMethod = new AstRegisteredMethodCall(_token.SourceCodeLine, _token.Value);
            RegisteredMethod registeredMethod = SyT.RegisteredMethods[_token.Value];

            ExpectToken(TokenType.L_Brack);

            if (registeredMethod.ParametersTypes.Count == 0)
            {
                ExpectToken(TokenType.R_Brack);
            }
            else
            {
                for (int pIndex = 0; pIndex < registeredMethod.ParametersTypes.Count; pIndex++)
                {
                    NextToken();

                    switch (_token.Type)
                    {
                        case TokenType.Literal:
                            rMethod.PassedValues.Add(new AstLiteral(_token.SourceCodeLine, _token.Value));
                            break;

                        case TokenType.Identifier:
                            rMethod.PassedValues.Add(new AstVariable(_token.SourceCodeLine, _token.Value));
                            break;

                        default:
                            throw new UnexpectedTokenException(_token);
                    }

                    if (pIndex == registeredMethod.ParametersTypes.Count - 1)
                    {
                        ExpectToken(TokenType.R_Brack);
                    }
                    else
                    {
                        ExpectToken(TokenType.Comma);
                    }
                }
            }

            if (isAssigned)
            {
                _methodToAssign = rMethod;
            }
            else
            {
                block.Statements.Add(rMethod);
            }
        }

        private void MakeAssignmentNode(AstBlock block, string variableName, bool isInitialization = false)
        {
            AstAssignment node;

            // If a statement is in the form of 'identifier = expression;' it is treated as initialization.
            if (isInitialization)
            {
                ExpectToken(TokenType.Eql);
            }

            block.Statements.Add(new AstAssignment());

            node = block.Statements.Last() as AstAssignment;
            node.Variable = new AstVariable(_token.SourceCodeLine, variableName);

            NextToken();

            var rpnStack = new Stack<Ast>();
            var operatorStack = new Stack<Token>();

            _firstExpression = true;
            ShuntingYardAlgorithm(block, rpnStack, operatorStack);

            node.Expression = new AstExpression(_token.SourceCodeLine);
            ParseRPN(rpnStack, node.Expression);
        }

        private void MakeWriteNode(AstBlock block)
        {
            ExpectToken(TokenType.L_Brack);
            block.Statements.Add(new AstWrite(_token.SourceCodeLine));

            NextToken();
            switch (_token.Type)
            {
                case TokenType.Literal:
                    ToWrite(block);
                    break;

                case TokenType.Identifier:
                    ToWrite(block);
                    break;

                default:
                    throw new UnexpectedTokenException(_token);
            }
            ExpectToken(TokenType.R_Brack);
        }

        private void MakeReadNode(AstBlock block)
        {
            ExpectToken(TokenType.L_Brack);

            NextToken();
            switch (_token.Type)
            {
                case TokenType.Identifier:
                    block.Statements.Add(new AstRead(_token.SourceCodeLine, _token.Value));
                    break;

                default:
                    throw new UnexpectedTokenException(_token);
            }

            ExpectToken(TokenType.R_Brack);
        }

        private void MakeConditionalNode(AstBlock block)
        {
            block.Statements.Add(new AstConditional());

            var node = block.Statements.Last() as AstConditional;

            ExpectToken(TokenType.L_Brack);
            Condition(node.Condition);
            ExpectToken(TokenType.R_Brack);

            NextToken();
            node.IfBlock = Block();

            if (PeekToken(TokenType.Else))
            {
                NextToken();
                node.ElseBlock = Block();
            }
        }

        private void MakeWhileNode(AstBlock block)
        {
            block.Statements.Add(new AstWhile());

            var node = block.Statements.Last() as AstWhile;

            ExpectToken(TokenType.L_Brack);
            Condition(node.Condition);
            ExpectToken(TokenType.R_Brack);

            NextToken();
            node.Block = Block();
        }

        private void SetFunctionReturn()
        {
            if (!_funcHasToReturn)
            {
                throw new UnexpectedTokenException(_token);
            }

            NextToken();
            switch (_token.Type)
            {
                case TokenType.Literal:
                    SyT.Functions[_currFunctionName].ReturnAst = new AstLiteral(_token.SourceCodeLine, _token.Value);

                    break;

                case TokenType.Identifier:
                    SyT.Functions[_currFunctionName].ReturnAst = new AstVariable(_token.SourceCodeLine, _token.Value);
                    break;

                default:
                    throw new UnexpectedTokenException(_token);
            }

            _funcHasToReturn = false;
        }

        private void ToWrite(AstBlock block)
        {
            switch (_token.Type)
            {
                case TokenType.Literal:
                    (block.Statements.Last() as AstWrite).ToWrite.Add(new AstLiteral(_token.SourceCodeLine, _token.Value));
                    break;

                case TokenType.Identifier:
                    (block.Statements.Last() as AstWrite).ToWrite.Add(new AstVariable(_token.SourceCodeLine, _token.Value));
                    break;
            }

            if (PeekToken(TokenType.Operator))
            {
                if (_token.Value != "+")
                {
                    throw new UnexpectedTokenException(_token);
                }

                if (PeekToken(TokenType.Literal) || PeekToken(TokenType.Identifier))
                {
                    ToWrite(block);
                }
                else
                {
                    NextToken();
                    throw new UnexpectedTokenException(_token);
                }
            }
        }

        private void Condition(AstComparison condition)
        {
            NextToken();
            switch (_token.Type)
            {
                case TokenType.Literal:
                    condition.Left = new AstLiteral(_token.SourceCodeLine, _token.Value);
                    break;

                case TokenType.Identifier:
                    condition.Left = new AstVariable(_token.SourceCodeLine, _token.Value);
                    break;

                default:
                    throw new UnexpectedTokenException(_token);
            }

            ExpectToken(TokenType.Comparator);
            switch (_token.Value)
            {
                case "==":
                    condition.Comparator = Comparator.Eql;
                    break;
                case "!=":
                    condition.Comparator = Comparator.Neq;
                    break;
                case "<":
                    condition.Comparator = Comparator.Lss;
                    break;
                case "<=":
                    condition.Comparator = Comparator.Leq;
                    break;
                case ">":
                    condition.Comparator = Comparator.Gtr;
                    break;
                case ">=":
                    condition.Comparator = Comparator.Geq;
                    break;
            }

            condition.SourceCodeLine = _token.SourceCodeLine;

            NextToken();
            switch (_token.Type)
            {
                case TokenType.Literal:
                    condition.Right = new AstLiteral(_token.SourceCodeLine, _token.Value);
                    break;

                case TokenType.Identifier:
                    condition.Right = new AstVariable(_token.SourceCodeLine, _token.Value);
                    break;

                default:
                    throw new UnexpectedTokenException(_token);
            }
        }

        // Converts infix notation to postfix.
        private void ShuntingYardAlgorithm(AstBlock block, Stack<Ast> rpnStack, Stack<Token> operatorStack)
        {
            if (_firstExpression)
            {
                if (_token.Type == TokenType.Operator)
                {
                    if (_token.Value == "+" || _token.Value == "-")
                    {
                        operatorStack.Push(_token);
                        NextToken();
                    }
                    else
                    {
                        throw new UnexpectedTokenException(_token);
                    }
                }

                _firstExpression = false;
            }

            switch (_token.Type)
            {
                case TokenType.Literal:
                    rpnStack.Push(new AstLiteral(_token.SourceCodeLine, _token.Value));
                    break;

                case TokenType.Identifier:
                    if (IsFunction())
                    {
                        MakeFunctionCallNode(block, true);
                        rpnStack.Push(_functionToAssign.DeepCopy());
                    }
                    else if (IsRegisteredMethod())
                    {
                        MakeRegisteredMethodNode(block, true);
                        rpnStack.Push(_methodToAssign.DeepCopy());
                    }
                    else
                    {
                        rpnStack.Push(new AstVariable(_token.SourceCodeLine, _token.Value));
                    }

                    break;

                case TokenType.L_Brack:
                    operatorStack.Push(_token);

                    NextToken();
                    ShuntingYardAlgorithm(block, rpnStack, operatorStack);

                    NextToken();
                    if (_token.Type != TokenType.R_Brack)
                    {
                        throw new UnexpectedTokenException(_token);
                    }

                    operatorStack.Pop();
                    break;

                default:
                    throw new UnexpectedTokenException(_token);
            }

            if (PeekToken(TokenType.Operator))
            {
                while (operatorStack.Count > 0 && operatorStack.Peek().Type != TokenType.L_Brack && _token.OperatorPrecedence <= operatorStack.Peek().OperatorPrecedence)
                {
                    rpnStack.Push(new AstOperator(operatorStack.Pop()));
                }

                operatorStack.Push(_token);

                NextToken();
                ShuntingYardAlgorithm(block, rpnStack, operatorStack);
            }
            else
            {
                while (operatorStack.Count > 0 && operatorStack.Peek().Type != TokenType.L_Brack)
                {
                    rpnStack.Push(new AstOperator(operatorStack.Pop()));
                }
            }
        }

        // Makes an AST out of rpn notation. It reads it from right to left.
        private void ParseRPN(Stack<Ast> rpn, AstExpression expression)
        {
            if (rpn.Count == 0 && _operandToCarry == null)
            {
                return;
            }

            Ast operand;

            if (_operandToCarry == null)
            {
                operand = rpn.Pop();
            }
            else
            {
                operand = _operandToCarry;
                _operandToCarry = null;
            }

            if (operand.AstType == AstType.Operator)
            {
                if (expression.Operator == null)
                {
                    expression.Operator = operand as AstOperator;
                }
                else if (expression.Right == null)
                {
                    expression.Right = new AstExpression(_token.SourceCodeLine);
                    (expression.Right as AstExpression).Operator = operand as AstOperator;

                    ParseRPN(rpn, (AstExpression)expression.Right);
                }
                else if (expression.Left == null)
                {
                    expression.Left = new AstExpression(_token.SourceCodeLine);
                    (expression.Left as AstExpression).Operator = operand as AstOperator;

                    ParseRPN(rpn, (AstExpression)expression.Left);
                }
                else
                {
                    _operandToCarry = operand;
                    return;
                }
            }
            else
            {
                if (expression.Right == null)
                {
                    expression.Right = operand;
                }
                else if (expression.Left == null)
                {
                    expression.Left = operand;
                }
                else
                {
                    _operandToCarry = operand;
                    return;
                }
            }

            ParseRPN(rpn, expression);
        }


        private void NextToken()
        {
            if (_tokenQueue.Count > 0)
            {
                _token = _tokenQueue.Dequeue();
            }
            else
            {
                throw new UnexpectedTokenException(_token);
            }
        }

        private bool PeekToken(TokenType tokenType)
        {
            Token nextToken = _tokenQueue.Peek();

            if (nextToken.Type == tokenType)
            {
                NextToken();
                return true;
            }

            return false;
        }

        private void ExpectToken(TokenType tokenType)
        {
            NextToken();
            if (_token.Type != tokenType)
            {
                throw new UnexpectedTokenException(_token);
            }

            return;
        }

        private DataType ExpectAndReturnDataTypeToken(bool isFunctionReturnType)
        {
            NextToken();
            switch (_token.Type)
            {
                case TokenType.Void:
                    if (isFunctionReturnType)
                    {
                        return DataType.Prazno;
                    }
                    throw new UnexpectedTokenException(_token);

                case TokenType.Int:
                    return DataType.Celo;

                case TokenType.Double:
                    return DataType.Decimalno;

                case TokenType.String:
                    return DataType.Niz;

                default:
                    throw new UnexpectedTokenException(_token);
            }
        }

        private bool IsFunction()
        {
            if (SyT.Functions.ContainsKey(_token.Value))
            {
                return true;
            }

            return false;
        }

        private void IsFunctionPredefined()
        {
            if (IsFunction() || IsRegisteredMethod())
            {
                throw new PredefinedFunctionException(_token);
            }
        }

        private bool IsRegisteredMethod()
        {
            if (SyT.RegisteredMethods.ContainsKey(_token.Value))
            {
                return true;
            }

            return false;
        }
    }
}
