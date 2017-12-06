// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DefaultQueryExpressionVisitor : ExpressionVisitorBase
    {
        private readonly EntityQueryModelVisitor _entityQueryModelVisitor;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DefaultQueryExpressionVisitor([NotNull] EntityQueryModelVisitor entityQueryModelVisitor)
        {
            Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor));

            _entityQueryModelVisitor = entityQueryModelVisitor;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityQueryModelVisitor QueryModelVisitor => _entityQueryModelVisitor;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var queryModelVisitor = CreateQueryModelVisitor();

            queryModelVisitor.VisitQueryModel(expression.QueryModel);

            return queryModelVisitor.Expression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual EntityQueryModelVisitor CreateQueryModelVisitor()
            => QueryModelVisitor.QueryCompilationContext
                .CreateQueryModelVisitor(_entityQueryModelVisitor);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Name.StartsWith(CompiledQueryCache.CompiledQueryParameterPrefix, StringComparison.Ordinal))
            {
                return Expression.Call(
                    GetParameterValueMethodInfo.MakeGenericMethod(node.Type),
                    EntityQueryModelVisitor.QueryContextParameter,
                    Expression.Constant(node.Name));
            }

            // TODO: ???
            if (node.Name.StartsWith("_outer_", StringComparison.Ordinal))
            {
                return Expression.Call(
                    GetParameterValueMethodInfo.MakeGenericMethod(node.Type),
                    EntityQueryModelVisitor.QueryContextParameter,
                    Expression.Constant(node.Name));
            }

            return node;
        }

        private Dictionary<ParameterExpression, Expression> _parameterMapping = new Dictionary<ParameterExpression, Expression>();


        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name.Contains("_InjectParameters"))
            {
                var newArguments = new List<Expression>();
                foreach (var argument in node.Arguments)
                {
                    var newArgument = Visit(argument);
                    newArguments.Add(newArgument);
                }

                if (newArguments[1].Type != node.Arguments[1].Type)
                {
                    var newType = newArguments[1].Type.GenericTypeArguments[0];

                    var newMethod = _entityQueryModelVisitor.QueryCompilationContext.LinqOperatorProvider.InjectParametersMethod.MakeGenericMethod(newType);

                    return Expression.Call(newMethod, newArguments);
                }
            }

            if (node.Method.Name == "_ToQueryable")
            {
                var newArguments = new List<Expression>();
                foreach (var argument in node.Arguments)
                {
                    var newArgument = Visit(argument);
                    newArguments.Add(newArgument);
                }

                if (newArguments[0].Type != node.Arguments[0].Type)
                {
                    var newType = newArguments[0].Type.GenericTypeArguments[0];

                    var newMethod = _entityQueryModelVisitor.QueryCompilationContext.LinqOperatorProvider.ToQueryable.MakeGenericMethod(newType);

                    return Expression.Call(newMethod, newArguments);
                }
            }

            return base.VisitMethodCall(node);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitExtension(Expression node)
        {
            if (node is NullConditionalExpression nullConditionalExpression)
            {
                var newCaller = Visit(nullConditionalExpression.Caller);
                var newAccessOperation = Visit(nullConditionalExpression.AccessOperation);

                return newCaller != nullConditionalExpression.Caller
                       || newAccessOperation != nullConditionalExpression.AccessOperation
                    ? new NullConditionalExpression(newCaller, newAccessOperation)
                    : node;
            }

            if (node is InjectParametersExpression injectParametersExpression)
            {
                var modified = false;

                var newParameters = injectParametersExpression.Parameters;
                //var newParameters = new List<ParameterExpression>();
                //foreach (var parameter in injectParametersExpression.Parameters)
                //{
                //    var newParameter = (ParameterExpression)Visit(parameter);
                //    newParameters.Add(newParameter);
                //    if (newParameter != parameter)
                //    {
                //        modified = true;
                //    }
                //}

                var newParameterValues = new List<Expression>();
                foreach (var parameterValue in injectParametersExpression.ParameterValues)
                {
                    var newParameterValue = Visit(parameterValue);
                    newParameterValues.Add(newParameterValue);
                    if (newParameterValue != parameterValue)
                    {
                        modified = true;
                    }
                }

                _parameterMapping = newParameters.Zip(newParameterValues, (p, v) => new { p, v }).ToDictionary(e => e.p, e => e.v);

                var newQuery = Visit(injectParametersExpression.Query);

                return modified || newQuery != injectParametersExpression.Query
                    ? new InjectParametersExpression(newParameters, newParameterValues, newQuery)
                    : node;
            }

            return base.VisitExtension(node);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly MethodInfo GetParameterValueMethodInfo
            = typeof(DefaultQueryExpressionVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(GetParameterValue));

        [UsedImplicitly]
        private static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
            => (T)queryContext.ParameterValues[parameterName];
    }
}
