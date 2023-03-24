using JanCLang.AstNodes;
using JanCLang.Misc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JanCLang.InterpreterMachine
{
    // Interprets the Abstract Syntax Tree that the Parser outputs.
    internal class Interpreter
    {
        private SymbolTable SyT;

        // Value that a user function returns.
        private Ast _functionReturn = null;

        // Value that a registered method returns.
        private object _registeredMethodReturn = null;



        // ENTRY POINT
        internal void RunInterpreter(AstBlock block, SymbolTable syt)
        {
            SyT = syt;

            MakeScope();
            InterpretBlock(block);
        }

        private void InterpretBlock(AstBlock block, bool deleteDeclaredVariables = true)
        {
            // Variables declared in this block. They are deleted after the end of the block if deleteDeclaredVariables = true.
            var declaredVariables = new List<string>();

            foreach (Ast statement in block.Statements)
            {
                switch (statement.AstType)
                {
                    case AstType.Declaration:
                        InterpretDeclaration(statement as AstDeclaration, declaredVariables);
                        break;

                    case AstType.Assignment:
                        InterpretAssignment(statement as AstAssignment);
                        break;

                    case AstType.Write:
                        InterpretWrite(statement as AstWrite);
                        break;

                    case AstType.Read:
                        InterpretRead(statement as AstRead);
                        break;

                    case AstType.Conditional:
                        InterpretConditional(statement as AstConditional);
                        break;

                    case AstType.While:
                        InterpretWhile(statement as AstWhile);
                        break;

                    case AstType.FunctionCall:
                        InterpretFunction(statement as AstFunctionCall);
                        break;

                    case AstType.RegisteredMethodCall:
                        InterpretRegisteredMethod(statement as AstRegisteredMethodCall);
                        break;
                }
            }

            if (deleteDeclaredVariables)
            {
                foreach (string variable in declaredVariables)
                {
                    SyT.Variables.Peek().Remove(variable);
                }
            }
        }

        private void InterpretDeclaration(AstDeclaration node, List<string> declaredVariables)
        {
            declaredVariables.Add(node.Variable.Name);
            AddVariable(node.Variable.Name, node.Variable);
        }

        private void InterpretAssignment(AstAssignment node)
        {
            node.Variable.Type = GetVariableType(node.Variable);

            object result = CalculateExpression(node.Expression, node.Variable.Type);
            SetVariableValue(node.Variable, result);
        }

        private void InterpretWrite(AstWrite node)
        {
            string toWrite = "";
            foreach (var item in node.ToWrite)
            {
                switch (item.AstType)
                {
                    case AstType.Literal:
                        var literal = item as AstLiteral;
                        toWrite += literal.Value;
                        break;

                    case AstType.Variable:
                        var variable = item as AstVariable;
                        toWrite += GetVariableValue(variable);
                        break;
                }
            }

            try
            {
                SyT.Write.Invoke(toWrite);
            }
            catch (Exception e)
            {
                throw new RegisteredMethodExternalException("Izpiši", e.Message, node.SourceCodeLine);
            }

        }

        private void InterpretRead(AstRead node)
        {
            string input = "";
            try
            {
                input = SyT.Read.Invoke();
            }
            catch (Exception e)
            {
                throw new RegisteredMethodExternalException("Vnos", e.Message, node.SourceCodeLine);
            }


            AstVariable variable = node.Variable;

            DataType variableType = GetVariableType(variable);

            switch (variableType)
            {
                case DataType.Celo:
                    int iValue;
                    if (!int.TryParse(input, out iValue))
                    {
                        throw new AssignValueException(input as object, variableType, node.SourceCodeLine);
                    }

                    SetVariableValue(variable, iValue);
                    break;

                case DataType.Decimalno:
                    double dValue;
                    if (!double.TryParse(input, out dValue))
                    {
                        throw new AssignValueException(input as object, variableType, node.SourceCodeLine);
                    }

                    SetVariableValue(variable, dValue);
                    break;

                default:
                    SetVariableValue(variable, input);
                    break;
            }
        }

        private void InterpretConditional(AstConditional node)
        {
            bool conditionMet = CheckCondition(node);

            if (conditionMet)
            {
                InterpretBlock(node.IfBlock);
            }

            if (node.ElseBlock != null && !conditionMet)
            {
                InterpretBlock(node.ElseBlock);
            }
        }

        private void InterpretWhile(AstWhile node)
        {
            while (CheckCondition(node))
            {
                InterpretBlock(node.Block);
            }
        }

        private void InterpretFunction(AstFunctionCall node)
        {
            MakeScope();

            Function function = SyT.Functions[node.Name].DeepCopy();
            function.Parameters = node.Parameters;

            foreach (AstParameter parameter in function.Parameters)
            {
                switch (parameter.Value.AstType)
                {
                    case AstType.Literal:
                        var pLiteral = parameter.Value as AstLiteral;
                        AddVariable(parameter.Name, new AstVariable(parameter.SourceCodeLine, parameter.Name, pLiteral.Value, parameter.Type));
                        break;

                    case AstType.Variable:
                        var pVariable = parameter.Value as AstVariable;
                        AddVariable(parameter.Name, SyT.Variables.ElementAt(1)[pVariable.Name].ShallowCopy());
                        SyT.Variables.Peek()[parameter.Name].Name = parameter.Name;
                        break;
                }
            }

            InterpretBlock(function.Block, false);

            if (function.Returns)
            {
                switch (function.ReturnAst.AstType)
                {
                    case AstType.Literal:
                        _functionReturn = function.ReturnAst;
                        break;

                    case AstType.Variable:
                        _functionReturn = GetVariable((function.ReturnAst as AstVariable).Name);
                        break;
                }
            }

            RemoveScope();
        }

        private void InterpretRegisteredMethod(AstRegisteredMethodCall node)
        {
            RegisteredMethod rMethod = SyT.RegisteredMethods[node.Name];

            // List of values passed to a method.
            var argsList = new List<object>();

            // Populates argsList with values, that are later passed to a method as an object[].
            // Method signature is known only at runtime so it has to be invoked dynamicaly, which also requires
            // dynamicaly preparing the passed values.
            for (int pIndex = 0; pIndex < node.PassedValues.Count; pIndex++)
            {
                switch (node.PassedValues[pIndex].AstType)
                {
                    case AstType.Literal:
                        var literal = node.PassedValues[pIndex] as AstLiteral;
                        argsList.Add(literal.Value);
                        break;

                    case AstType.Variable:
                        var variable = GetVariable((node.PassedValues[pIndex] as AstVariable).Name);
                        argsList.Add(variable.Value);
                        break;
                }
            }

            try
            {
                _registeredMethodReturn = rMethod.Method.DynamicInvoke(argsList.ToArray());
            }
            catch (Exception e)
            {
                throw new RegisteredMethodExternalException(node.Name, e.InnerException.Message, node.SourceCodeLine);
            }
        }

        private object CalculateExpression(AstExpression expression, DataType assignedToType)
        {
            object left = null;
            if (expression.Left != null)
            {
                left = CalculateExpressionNode(expression.Left, assignedToType);
            }

            object right = CalculateExpressionNode(expression.Right, assignedToType);
            return CalculateBinary(left, right, expression.Operator, assignedToType, expression.SourceCodeLine);
        }

        private object CalculateExpressionNode(Ast exprNode, DataType assignedToType)
        {
            switch (exprNode.AstType)
            {
                case AstType.Literal:
                    var nLiteral = exprNode as AstLiteral;
                    return nLiteral.Value;

                case AstType.Variable:
                    var nVariable = exprNode as AstVariable;
                    return GetVariableValue(nVariable);

                case AstType.FunctionCall:
                    var nFunction = exprNode as AstFunctionCall;

                    InterpretFunction(nFunction);

                    if (_functionReturn.AstType == AstType.Literal)
                    {
                        return (_functionReturn as AstLiteral).Value;
                    }
                    else
                    {
                        return (_functionReturn as AstVariable).Value;
                    }

                case AstType.RegisteredMethodCall:
                    var nMethod = exprNode as AstRegisteredMethodCall;
                    InterpretRegisteredMethod(nMethod);
                    return _registeredMethodReturn;

                case AstType.Expression:
                    return CalculateExpression((AstExpression)exprNode, assignedToType);

                default:
                    throw new CanNotCalculateException(exprNode.SourceCodeLine);
            }
        }

        private object CalculateBinary(object left, object right, AstOperator op, DataType assignedToType, int sourceCodeLine)
        {
            object result = null;

            if (op == null)
            {
                return right;
            }

            if (left == null)
            {
                switch (assignedToType)
                {
                    case DataType.Celo:
                        return op.Operator == Operator.Plus ? right : 0 - Convert.ToInt32(right);
                    case DataType.Decimalno:
                        return op.Operator == Operator.Plus ? right : 0 - Convert.ToDouble(right);
                }
            }

            if (assignedToType == DataType.Celo)
            {
                switch (op.Operator)
                {
                    case Operator.Plus:
                        result = Convert.ToInt32(left) + Convert.ToInt32(right);
                        break;
                    case Operator.Minus:
                        result = Convert.ToInt32(left) - Convert.ToInt32(right);
                        break;
                    case Operator.Multiplication:
                        result = Convert.ToInt32(left) * Convert.ToInt32(right);
                        break;
                    case Operator.Division:
                        if (Convert.ToInt32(right) == 0)
                        {
                            throw new DivisionByZeroException(sourceCodeLine);
                        }
                        result = Convert.ToInt32(left) / Convert.ToInt32(right);
                        break;
                }
            }
            else if (assignedToType == DataType.Decimalno)
            {
                switch (op.Operator)
                {
                    case Operator.Plus:
                        result = Convert.ToDouble(left) + Convert.ToDouble(right);
                        break;
                    case Operator.Minus:
                        result = Convert.ToDouble(left) - Convert.ToDouble(right);
                        break;
                    case Operator.Multiplication:
                        result = Convert.ToDouble(left) * Convert.ToDouble(right);
                        break;
                    case Operator.Division:
                        if (Convert.ToDouble(right) == 0)
                        {
                            throw new DivisionByZeroException(sourceCodeLine);
                        }
                        result = Convert.ToDouble(left) / Convert.ToDouble(right);
                        break;
                }
            }
            else
            {
                // Strings can be null if they are set to null by a registered method.
                // String is a nullabe type, whereas int and double are not. They are not checked for null.
                string sLeft = left != null ? left.ToString() : "";
                string sRight = right != null ? right.ToString() : "";
                result = sLeft + sRight;
            }

            return result;
        }

        private bool CheckCondition(Ast node)
        {
            AstComparison compare = node.AstType == AstType.Conditional ? (node as AstConditional).Condition : (node as AstWhile).Condition;

            bool conditionMet = false;
            object leftValue, rightValue;
            DataType leftType, rightType;

            if (compare.Left.AstType == AstType.Literal)
            {
                leftType = (compare.Left as AstLiteral).Type;
                leftValue = (compare.Left as AstLiteral).Value;
            }
            else
            {
                var variable = compare.Left as AstVariable;
                leftType = GetVariableType(variable);
                leftValue = GetVariableValue(variable);
            }

            if (compare.Right.AstType == AstType.Literal)
            {
                rightType = (compare.Right as AstLiteral).Type;
                rightValue = (compare.Right as AstLiteral).Value;
            }
            else
            {
                var variable = compare.Right as AstVariable;
                rightType = GetVariableType(variable);
                rightValue = GetVariableValue(variable);
            }

            if (leftType == DataType.Niz && rightType == DataType.Niz)
            {
                // Strings can be null if they are set to null by a registered method.
                // String is a nullabe type, whereas int and double are not. They are not checked for null.
                string left = leftValue != null ? leftValue.ToString() : "";
                string right = rightValue != null ? rightValue.ToString() : "";

                switch (compare.Comparator)
                {
                    case Comparator.Eql:
                        if (left == right) conditionMet = true;
                        break;
                    case Comparator.Neq:
                        if (left != right) conditionMet = true;
                        break;
                    case Comparator.Lss:
                        if (left.CompareTo(right) < 0) conditionMet = true;
                        break;
                    case Comparator.Leq:
                        if (left.CompareTo(right) <= 0) conditionMet = true;
                        break;
                    case Comparator.Gtr:
                        if (left.CompareTo(right) > 0) conditionMet = true;
                        break;
                    case Comparator.Geq:
                        if (left.CompareTo(right) >= 0) conditionMet = true;
                        break;
                }
            }
            else
            {
                double left = Convert.ToDouble(leftValue);
                double right = Convert.ToDouble(rightValue);

                switch (compare.Comparator)
                {
                    case Comparator.Eql:
                        if (left == right) conditionMet = true;
                        break;
                    case Comparator.Neq:
                        if (left != right) conditionMet = true;
                        break;
                    case Comparator.Lss:
                        if (left < right) conditionMet = true;
                        break;
                    case Comparator.Leq:
                        if (left <= right) conditionMet = true;
                        break;
                    case Comparator.Gtr:
                        if (left > right) conditionMet = true;
                        break;
                    case Comparator.Geq:
                        if (left >= right) conditionMet = true;
                        break;
                }
            }

            return conditionMet;
        }

        private void MakeScope()
        {
            var scope = new Dictionary<string, AstVariable>();
            SyT.Variables.Push(scope);
        }

        private void RemoveScope()
        {
            SyT.Variables.Pop();
        }

        private void AddVariable(string variableName, AstVariable variable)
        {
            SyT.Variables.Peek().Add(variableName, variable);
        }

        private AstVariable GetVariable(string variableName)
        {
            return SyT.Variables.Peek()[variableName];
        }

        private DataType GetVariableType(AstVariable variable)
        {
            return SyT.Variables.Peek()[variable.Name].Type;
        }

        private void SetVariableValue(AstVariable variable, object value)
        {
            SyT.Variables.Peek()[variable.Name].Value = value;
        }

        private object GetVariableValue(AstVariable variable)
        {
            return SyT.Variables.Peek()[variable.Name].Value;
        }
    }
}
