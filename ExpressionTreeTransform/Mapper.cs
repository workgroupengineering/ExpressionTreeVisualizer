﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Linq;
using System.Linq.Expressions;
using static Microsoft.CodeAnalysis.LanguageNames;
using static System.Linq.Expressions.ExpressionType;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace ExpressionTreeTransform {
    public class Mapper {
        private string language;
        private Workspace workspace;
        private SyntaxGenerator generator;

        public SyntaxNode GetSyntaxNode(Expression expr, string language) {
            this.language = language;

            // TODO within the visualizer, it may be possible to get the workspace / generator for the current code
            workspace = new AdhocWorkspace();
            generator = SyntaxGenerator.GetGenerator(workspace, language);

            return getSyntaxNode(expr);
        }

        private SyntaxNode getSyntaxNode(Expression expr) {
            switch (expr.NodeType) {

                #region BinaryExpression

                // mathematical operations
                case Add:
                case AddChecked:
                case Divide:
                case Modulo:
                case Multiply:
                case MultiplyChecked:
                case Power:
                case Subtract:
                case SubtractChecked:

                // bitwise / logical operations
                case And:
                case Or:
                case ExclusiveOr:

                // shift operations
                case LeftShift:
                case RightShift:

                // conditional boolean operators
                case AndAlso:
                case OrElse:

                // comparison operators
                case Equal:
                case NotEqual:
                case GreaterThanOrEqual:
                case GreaterThan:
                case LessThan:
                case LessThanOrEqual:

                // coalescing operators
                case Coalesce:

                // array indexing operations
                case ArrayIndex:
                    return getSyntaxNode(expr as BinaryExpression);

                #endregion

                #region UnaryExpression

                case ArrayLength:
                case ExpressionType.Convert:
                case ConvertChecked:
                case Negate:
                case NegateChecked:
                case Not:
                case Quote:
                case TypeAs:
                case UnaryPlus:
                    return getSyntaxNode(expr as UnaryExpression);

                #endregion

                case Lambda:
                    return getSyntaxNode(expr as LambdaExpression);

                case Parameter:
                    return getSyntaxNode(expr as ParameterExpression);

                case Constant:
                    return getSyntaxNode(expr as ConstantExpression);

                default:
                    throw new NotImplementedException();

                    /*case AddAssign:
                        break;
                    case AddAssignChecked:
                        break;
                    case AndAssign:
                        break;
                    case Assign:
                        break;
                    case Block:
                        break;
                    case Call:
                        break;
                    case Conditional:
                        break;
                    case Constant:
                        break;
                    case DebugInfo:
                        break;
                    case Decrement:
                        break;
                    case Default:
                        break;
                    case DivideAssign:
                        break;
                    case Dynamic:
                        break;
                    case ExclusiveOrAssign:
                        break;
                    case Extension:
                        break;
                    case Goto:
                        break;
                    case Increment:
                        break;
                    case Index:
                        break;
                    case Invoke:
                        break;
                    case IsFalse:
                        break;
                    case IsTrue:
                        break;
                    case Label:
                        break;
                    case LeftShiftAssign:
                        break;
                    case ListInit:
                        break;
                    case Loop:
                        break;
                    case MemberAccess:
                        break;
                    case MemberInit:
                        break;
                    case ModuloAssign:
                        break;
                    case MultiplyAssign:
                        break;
                    case MultiplyAssignChecked:
                        break;
                    case New:
                        break;
                    case NewArrayBounds:
                        break;
                    case NewArrayInit:
                        break;
                    case OnesComplement:
                        break;
                    case OrAssign:
                        break;
                    case PostDecrementAssign:
                        break;
                    case PostIncrementAssign:
                        break;
                    case PowerAssign:
                        break;
                    case PreDecrementAssign:
                        break;
                    case PreIncrementAssign:
                        break;
                    case RightShiftAssign:
                        break;
                    case RuntimeVariables:
                        break;
                    case SubtractAssign:
                        break;
                    case SubtractAssignChecked:
                        break;
                    case Switch:
                        break;
                    case Throw:
                        break;
                    case Try:
                        break;
                    case TypeEqual:
                        break;
                    case TypeIs:
                        break;
                    case Unbox:
                        break;
                    default:
                        break;*/
            }
        }

        private SyntaxNode getSyntaxNode(BinaryExpression expr) {
            Func<SyntaxNode, SyntaxNode, SyntaxNode> method = null;
            var left = getSyntaxNode(expr.Right);
            var right = getSyntaxNode(expr.Right);

            switch (expr.NodeType) {
                // binary arithmetic operations
                case Add:
                case AddChecked:
                    method = generator.AddExpression;
                    break;
                case Divide:
                    method = generator.DivideExpression;
                    break;
                case Modulo:
                    method = generator.ModuloExpression;
                    break;
                case Multiply:
                case MultiplyChecked:
                    method = generator.MultiplyExpression;
                    break;

                case Power:
                    if (language == VisualBasic) {
                        return VB.SyntaxFactory.ExponentiateExpression(
                            (VB.Syntax.ExpressionSyntax)left,
                            (VB.Syntax.ExpressionSyntax)right
                        );
                    }
                    throw new NotImplementedException();

                case Subtract:
                case SubtractChecked:
                    method = generator.SubtractExpression;
                    break;

                // bitwise / logical operations
                case And:
                    method = generator.BitwiseAndExpression;
                    break;
                case Or:
                    method = generator.BitwiseOrExpression;
                    break;

                case ExclusiveOr:
                    if (language == CSharp) {
                        return CS.SyntaxFactory.BinaryExpression(
                            CS.SyntaxKind.ExclusiveOrExpression,
                            (CS.Syntax.ExpressionSyntax)left,
                            (CS.Syntax.ExpressionSyntax)right
                        );
                    } else if (language == VisualBasic) {
                        return VB.SyntaxFactory.ExclusiveOrExpression(
                            (VB.Syntax.ExpressionSyntax)left,
                            (VB.Syntax.ExpressionSyntax)right
                        );
                    }
                    throw new NotImplementedException();

                // shift operations
                case LeftShift:
                    throw new NotImplementedException();
                case RightShift:
                    throw new NotImplementedException();

                // conditional boolean operators
                case AndAlso:
                    method = generator.LogicalAndExpression;
                    break;
                case OrElse:
                    method = generator.LogicalOrExpression;
                    break;

                // comparison operators
                case Equal:
                    method = generator.ValueEqualsExpression;
                    break;
                case NotEqual:
                    method = generator.ValueNotEqualsExpression;
                    break;
                case GreaterThanOrEqual:
                    method = generator.GreaterThanOrEqualExpression;
                    break;
                case GreaterThan:
                    method = generator.GreaterThanExpression;
                    break;
                case LessThan:
                    method = generator.LessThanExpression;
                    break;
                case LessThanOrEqual:
                    method = generator.LessThanOrEqualExpression;
                    break;

                // coalescing operators
                case Coalesce:
                    method = generator.CoalesceExpression;
                    break;

                // array indexing operations
                case ArrayIndex:
                    if (language == CSharp) {
                        return CS.SyntaxFactory.ElementAccessExpression(
                            (CS.Syntax.ExpressionSyntax)left,
                            CS.SyntaxFactory.BracketedArgumentList(
                                CS.SyntaxFactory.SingletonSeparatedList(
                                    CS.SyntaxFactory.Argument(
                                        (CS.Syntax.ExpressionSyntax)right
                                    )
                                )
                            )
                        );
                    } else if (language == VisualBasic) {
                        return VB.SyntaxFactory.InvocationExpression(
                            (VB.Syntax.ExpressionSyntax)left,
                            VB.SyntaxFactory.ArgumentList(
                                VB.SyntaxFactory.SeparatedList<VB.Syntax.ArgumentSyntax>(
                                    VB.SyntaxFactory.SingletonSeparatedList(
                                        VB.SyntaxFactory.SimpleArgument(
                                            (VB.Syntax.ExpressionSyntax)right
                                        )
                                    )
                                )
                            )
                        );
                    }
                    throw new NotImplementedException();

                default:
                    throw new NotImplementedException();
            }

            return method(left, right);
        }

        private SyntaxNode getSyntaxNode(UnaryExpression expr) {
            Func<SyntaxNode, SyntaxNode> method = null;
            var operand = getSyntaxNode(expr.Operand);

            Func<SyntaxNode, SyntaxNode, SyntaxNode> typeMethod = null;

            switch (expr.NodeType) {
                case ArrayLength:
                    generator.MemberAccessExpression(operand, "Length");
                    break;
                case ExpressionType.Convert:
                case ConvertChecked:
                    typeMethod = generator.CastExpression;
                    break;
                case Negate:
                case NegateChecked:
                    method = generator.NegateExpression;
                    break;
                case Not:
                    if (expr.Type == typeof(bool)) {
                        method = generator.LogicalNotExpression;
                    } else if (expr.Type.IsNumeric()) {
                        method = generator.BitwiseNotExpression;
                    } else {
                        throw new NotImplementedException();
                    }
                    break;
                case Quote:
                    throw new NotImplementedException();
                case TypeAs:
                    typeMethod = generator.TryCastExpression;
                    break;
                case UnaryPlus:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }

            if (typeMethod != null) {
                return typeMethod( getSyntaxNode(expr.Type), operand);
            }
            return method(operand);
        }

        private SyntaxNode getSyntaxNode(LambdaExpression expr) {
            var parameters = expr.Parameters.Select(x => getSyntaxNode(x));
            // TODO handle multple-statement expressions
            var body = getSyntaxNode(expr.Body);
            if (expr.ReturnType == typeof(void)) {
                return generator.VoidReturningLambdaExpression(parameters, body);
            } else {
                return generator.ValueReturningLambdaExpression(parameters, body);
            }
        }

        private SyntaxNode getSyntaxNode(ParameterExpression expr) =>generator.LambdaParameter(expr.Name);

        // TODO the typename needs to be resolved based on the current imports
        private SyntaxNode getSyntaxNode(Type t) =>  generator.IdentifierName(t.Name);

        private SyntaxNode getSyntaxNode(ConstantExpression expr) {
            switch (expr.Value) {
                case bool b:
                    return b ? generator.TrueLiteralExpression() : generator.FalseLiteralExpression();
                case null:
                    return generator.NullLiteralExpression();
                default:
                    return generator.LiteralExpression(expr.Value);
            }
        }


        //private SyntaxNode getSyntaxNode(BlockExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(ConditionalExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(DebugInfoExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(DefaultExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(DynamicExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(GotoExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(IndexExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(InvocationExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(LabelExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(ListInitExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(LoopExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(MemberExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(MemberInitExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(MethodCallExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(NewArrayExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(NewExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(RuntimeVariablesExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(SwitchExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(TryExpression expr) => throw new NotImplementedException();
        //private SyntaxNode getSyntaxNode(TypeBinaryExpression expr) => throw new NotImplementedException();
    }
}