using JanCLang.AstNodes;
using JanCLang.Misc;
using System;
using System.Collections.Generic;

namespace JanCLang.InterpreterMachine
{
    // Checks semantics of the program by examining the Abstract Syntax Tree that the Parser creates.
    // Checks scopes of variables, their point of declaration and initialization.
    // Performs type checking on variables, functions and registered methods and their parameters and return types respectively. 
    internal class SemanticAnalyzer
    {
        private SymbolTable SyT;
        
        // Semantic analyzer only needs to know if a variable is declared and/or initialized.
        private Stack<Dictionary<string, (AstVariable Variable, bool isInitialized)>> _variables = new Stack<Dictionary<string, (AstVariable, bool)>>();



        // ENTRY POINT
        internal void RunSemanticAnalyser(AstBlock block, SymbolTable syt)
        {
            SyT = syt;

            foreach (Function function in SyT.Functions.Values)
            {
                AnalyzeFunction(function);
            }

            MakeScope();
            AnalyzeBlock(block);
        }

        private void AnalyzeFunction(Function function)
        {
            MakeScope();

            foreach (AstParameter parameter in function.Parameters)
            {
                IsVariablePredefined(parameter.Name, parameter.SourceCodeLine);
                AddVariable(parameter.Name, new AstVariable(parameter.SourceCodeLine, parameter.Name, parameter.Type));
                InitializeVariable(parameter.Name);
            }

            AnalyzeBlock(function.Block, false);

            if (function.Returns)
            {
                switch (function.ReturnAst.AstType)
                {
                    case AstType.Literal:
                        if (!CheckDataTypes(function.ReturnType, (function.ReturnAst as AstLiteral).Type))
                        {
                            throw new FunctionReturnTypeException(function);
                        }

                        break;

                    case AstType.Variable:
                        var variable = function.ReturnAst as AstVariable;
                        IsVariableInitialized(variable.Name, variable.SourceCodeLine);
                        variable = GetVariable(variable.Name);

                        if (!CheckDataTypes(function.ReturnType, variable.Type))
                        {
                            throw new FunctionReturnTypeException(function);
                        }

                        break;
                }
            }

            RemoveScope();
        }

        private void AnalyzeBlock(AstBlock block, bool manageVariables = true)
        {
            // Variables declared in this block. They are deleted after the end of the block if manageVariables = true.
            var declaredVariables = new List<string>();
            // Variables first initialized in this block. They are deinitialized after the end of the block if manageVariables = true.
            var notInitializedVariables = new List<string>();

            foreach (Ast statement in block.Statements)
            {
                switch (statement.AstType)
                {
                    case AstType.Declaration:
                        AnalyzeDeclaration(statement as AstDeclaration, declaredVariables);
                        break;

                    case AstType.Assignment:
                        AnalyzeAssignment(statement as AstAssignment, notInitializedVariables);
                        break;

                    case AstType.Write:
                        AnalyzeWrite(statement as AstWrite);
                        break;

                    case AstType.Read:
                        AnalyzeRead(statement as AstRead);
                        break;

                    case AstType.Conditional:
                        AnalyzeConditional(statement as AstConditional);
                        break;

                    case AstType.While:
                        AnalyzeWhile(statement as AstWhile);
                        break;

                    case AstType.FunctionCall:
                        AnalyzeFunctionCall(statement as AstFunctionCall);
                        break;

                    case AstType.RegisteredMethodCall:
                        AnalyzeRegisteredMethodCall(statement as AstRegisteredMethodCall);
                        break;
                }
            }

            if (manageVariables)
            {
                foreach (string variable in notInitializedVariables)
                {
                    _variables.Peek()[variable] = (_variables.Peek()[variable].Variable, false);
                }

                foreach (string variable in declaredVariables)
                {
                    _variables.Peek().Remove(variable);
                }
            }
        }

        private void AnalyzeDeclaration(AstDeclaration node, List<string> declaredVariables)
        {
            IsVariablePredefined(node.Variable.Name, node.Variable.SourceCodeLine);
            declaredVariables.Add(node.Variable.Name);
            AddVariable(node.Variable.Name, node.Variable);
        }

        private void AnalyzeAssignment(AstAssignment node, List<string> notInitializedVariables)
        {
            IsVariableDeclared(node.Variable.Name, node.Variable.SourceCodeLine);

            if (!_variables.Peek()[node.Variable.Name].isInitialized)
            {
                notInitializedVariables.Add(node.Variable.Name);
            }

            node.Variable.Type = GetVariableType(node.Variable);

            AnalyzeExpression(node.Expression, node.Variable.Type);
            InitializeVariable(node.Variable.Name);
        }

        private void AnalyzeWrite(AstWrite node)
        {
            foreach (var item in node.ToWrite)
            {
                if (item.AstType == AstType.Variable)
                {
                    var variable = item as AstVariable;
                    IsVariableInitialized(variable.Name, variable.SourceCodeLine);
                }
            }
        }

        private void AnalyzeRead(AstRead node)
        {
            if (SyT.Read == null)
            {
                throw new UnregisteredReadException(node.SourceCodeLine);
            }

            IsVariableDeclared(node.Variable.Name, node.Variable.SourceCodeLine);
            InitializeVariable(node.Variable.Name);
        }

        private void AnalyzeConditional(AstConditional node)
        {
            AnalyzeCondition(node);

            AnalyzeBlock(node.IfBlock);

            if (node.ElseBlock != null)
            {
                AnalyzeBlock(node.ElseBlock);
            }
        }

        private void AnalyzeWhile(AstWhile node)
        {
            AnalyzeCondition(node);
            AnalyzeBlock(node.Block);
        }

        private void AnalyzeFunctionCall(AstFunctionCall node)
        {
            Function function = SyT.Functions[node.Name].DeepCopy();
            function.Parameters = node.Parameters;


            foreach (AstParameter parameter in node.Parameters)
            {
                switch (parameter.Value.AstType)
                {
                    case AstType.Literal:
                        var pLiteral = parameter.Value as AstLiteral;

                        if (!CheckDataTypes(parameter.Type, pLiteral.Type))
                        {
                            throw new LiteralParameterIncompatibilityException(pLiteral.Value, parameter, pLiteral.SourceCodeLine);
                        }

                        break;

                    case AstType.Variable:
                        var pVariable = parameter.Value as AstVariable;
                        IsVariableInitialized(pVariable.Name, pVariable.SourceCodeLine);

                        if (!CheckDataTypes(parameter.Type, _variables.Peek()[pVariable.Name].Variable.Type))
                        {
                            throw new VariableParameterIncompatibilityException(_variables.Peek()[pVariable.Name].Variable, parameter, pVariable.SourceCodeLine);
                        }

                        break;
                }
            }
        }

        private void AnalyzeRegisteredMethodCall(AstRegisteredMethodCall node)
        {
            RegisteredMethod rMethod = SyT.RegisteredMethods[node.Name];

            for (int pIndex = 0; pIndex < node.PassedValues.Count; pIndex++)
            {
                switch (node.PassedValues[pIndex].AstType)
                {
                    case AstType.Literal:
                        var literal = node.PassedValues[pIndex] as AstLiteral;

                        if (!CheckDataTypes(rMethod.ParametersTypes[pIndex], literal.Type))
                        {
                            throw new LiteralParameterIncompatibilityException(pIndex, literal.Value, node, rMethod.ParametersTypes[pIndex], literal.SourceCodeLine);
                        }

                        break;

                    case AstType.Variable:
                        var variable = node.PassedValues[pIndex] as AstVariable;
                        IsVariableInitialized(variable.Name, variable.SourceCodeLine);
                        variable = GetVariable(variable.Name);

                        if (!CheckDataTypes(rMethod.ParametersTypes[pIndex], variable.Type))
                        {
                            throw new VariableParameterIncompatibilityException(pIndex, variable, node, rMethod.ParametersTypes[pIndex], variable.SourceCodeLine);
                        }

                        break;
                }
            }
        }

        private void AnalyzeExpression(AstExpression expression, DataType assignedToType)
        {
            if (expression.Operator != null && assignedToType == DataType.Niz && expression.Operator.Operator != Operator.Plus)
            {
                throw new StringConcatenationException(expression.SourceCodeLine);
            }

            if (expression.Left != null)
            {
                AnalyzeExpressionNode(expression.Left, assignedToType, false);
            }

            bool isDivision = expression.Operator != null && expression.Operator.Operator == Operator.Division ? true : false;
            AnalyzeExpressionNode(expression.Right, assignedToType, isDivision);
        }

        private void AnalyzeExpressionNode(Ast exprNode, DataType assignedToType, bool isRightSideNodeAndDivision)
        {
            switch (exprNode.AstType)
            {
                case AstType.Literal:
                    var nLiteral = exprNode as AstLiteral;

                    if (!CheckDataTypes(assignedToType, nLiteral.Type))
                    {
                        throw new AssignValueException(nLiteral.Value, assignedToType, nLiteral.SourceCodeLine);
                    }

                    if (isRightSideNodeAndDivision && nLiteral.Type != DataType.Niz && Convert.ToDouble(nLiteral.Value) == 0)
                    {
                        throw new DivisionByZeroException(nLiteral.SourceCodeLine);
                    }

                    break;

                case AstType.Variable:
                    var nVariable = exprNode as AstVariable;
                    IsVariableInitialized(nVariable.Name, nVariable.SourceCodeLine);

                    if (assignedToType != DataType.Niz && !CheckDataTypes(assignedToType, GetVariableType(nVariable)))
                    {
                        throw new AssignValueException(nVariable.Name, assignedToType, nVariable.SourceCodeLine);
                    }

                    break;

                case AstType.FunctionCall:
                    var nFunction = exprNode as AstFunctionCall;

                    if (nFunction.ReturnType == DataType.Prazno)
                    {
                        throw new FunctionDoesNotReturnException(nFunction.Name, nFunction.SourceCodeLine);
                    }

                    if (assignedToType != DataType.Niz && !CheckDataTypes(assignedToType, nFunction.ReturnType))
                    {
                        throw new AssignValueException(nFunction, assignedToType, nFunction.SourceCodeLine);
                    }

                    AnalyzeFunctionCall(nFunction);

                    break;

                case AstType.RegisteredMethodCall:
                    var nMethod = exprNode as AstRegisteredMethodCall;

                    if (!SyT.RegisteredMethods[nMethod.Name].Returns)
                    {
                        throw new RegisteredMethodDoesNotReturnException(nMethod.Name, nMethod.SourceCodeLine);
                    }

                    if (assignedToType != DataType.Niz && !CheckDataTypes(assignedToType, SyT.RegisteredMethods[nMethod.Name].ReturnType))
                    {
                        throw new AssignValueException(SyT.RegisteredMethods[nMethod.Name], assignedToType, nMethod.SourceCodeLine);
                    }

                    AnalyzeRegisteredMethodCall(nMethod);

                    break;

                case AstType.Expression:
                    var expression = exprNode as AstExpression;
                    AnalyzeExpression(expression, assignedToType);
                    break;
            }
        }

        private void AnalyzeCondition(Ast node)
        {
            AstComparison compare = node.AstType == AstType.Conditional ? (node as AstConditional).Condition : (node as AstWhile).Condition;

            DataType leftType, rightType;

            if (compare.Left.AstType == AstType.Literal)
            {
                leftType = (compare.Left as AstLiteral).Type;
            }
            else
            {
                var variable = compare.Left as AstVariable;
                IsVariableInitialized(variable.Name, variable.SourceCodeLine);
                leftType = GetVariableType(variable);
            }

            if (compare.Right.AstType == AstType.Literal)
            {
                rightType = (compare.Right as AstLiteral).Type;
            }
            else
            {
                var variable = compare.Right as AstVariable;
                IsVariableInitialized(variable.Name, variable.SourceCodeLine);
                rightType = GetVariableType(variable);
            }

            if (!CheckDataTypes(leftType, rightType) && !(leftType == DataType.Celo && rightType == DataType.Decimalno))
            {
                throw new IncomparableValuesException(leftType, rightType, compare.SourceCodeLine);
            }
        }

        private void MakeScope()
        {
            var scope = new Dictionary<string, (AstVariable, bool)>();
            _variables.Push(scope);
        }

        private void RemoveScope()
        {
            _variables.Pop();
        }

        private bool CheckDataTypes(DataType type1, DataType type2)
        {
            if (type1 == type2 || type1 == DataType.Decimalno && type2 == DataType.Celo)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void AddVariable(string variableName, AstVariable variable)
        {
            _variables.Peek().Add(variableName, (variable, false));
        }

        private AstVariable GetVariable(string variableName)
        {
            return _variables.Peek()[variableName].Variable;
        }

        private DataType GetVariableType(AstVariable variable)
        {
            return _variables.Peek()[variable.Name].Variable.Type;
        }

        private void IsVariablePredefined(string checkName, int sourceCodeLine)
        {
            if (_variables.Peek().ContainsKey(checkName))
            {
                throw new PredefinedVariableException(checkName, sourceCodeLine);
            }

            if (IsFunction(checkName) || IsRegisteredMethod(checkName))
            {
                throw new VariableNamedAsFunctionException(checkName, sourceCodeLine);
            }
        }

        private void IsVariableDeclared(string checkName, int sourceCodeLine)
        {
            if (!_variables.Peek().ContainsKey(checkName))
            {
                throw new UndeclaredVariableException(checkName, sourceCodeLine);
            }
        }

        private void IsVariableInitialized(string checkName, int sourceCodeLine)
        {
            IsVariableDeclared(checkName, sourceCodeLine);
            if (!_variables.Peek()[checkName].isInitialized)
            {
                throw new UninitializedVariableException(checkName, sourceCodeLine);
            }
        }

        private void InitializeVariable(string variableName)
        {
            _variables.Peek()[variableName] = (_variables.Peek()[variableName].Variable, true);
        }

        private bool IsFunction(string checkName)
        {
            if (SyT.Functions.ContainsKey(checkName))
            {
                return true;
            }

            return false;
        }

        private bool IsRegisteredMethod(string checkName)
        {
            if (SyT.RegisteredMethods.ContainsKey(checkName))
            {
                return true;
            }

            return false;
        }
    }
}
