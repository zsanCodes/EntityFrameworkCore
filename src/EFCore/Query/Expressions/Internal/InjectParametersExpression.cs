// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Utilities;
using JetBrains.Annotations;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Query.Expressions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InjectParametersExpression : Expression
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InjectParametersExpression(
            [NotNull] IReadOnlyList<ParameterExpression> parameters,
            [NotNull] IReadOnlyList<Expression> parameterValues,
            [NotNull] Expression query)
        {
            Check.NotEmpty(parameters, nameof(parameters));
            Check.NotEmpty(parameterValues, nameof(parameterValues));
            Check.NotNull(query, nameof(query));

            Debug.Assert(parameters.Count == parameterValues.Count);

            Parameters = parameters;
            ParameterValues = parameterValues;
            Query = query;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IReadOnlyList<ParameterExpression> Parameters { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IReadOnlyList<Expression> ParameterValues { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Expression Query { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type Type => Query.Type;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool CanReduce => true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override ExpressionType NodeType => ExpressionType.Extension;

        private static readonly MethodInfo _setParameterMethodInfo
            = typeof(QueryContext).GetTypeInfo().GetDeclaredMethod(nameof(QueryContext.SetParameter));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression Reduce()
        {
            //var foo = typeof(Query.Internal.LinqOperatorProvider).GetTypeInfo().GetDeclaredMethod(nameof(EntityFrameworkCore.Query.Internal.LinqOperatorProvider._InjectParameters));


            var foo = typeof(EntityQueryModelVisitor).GetTypeInfo().GetDeclaredMethod(nameof(EntityQueryModelVisitor._InjectParameters2));

            var generic = foo.MakeGenericMethod(Query.Type);








            var expressions = new List<Expression>();

            var prmNames = Parameter(typeof(string[]), "names");
            var prmValues = Parameter(typeof(object[]), "values");

            for (var i = 0; i < Parameters.Count; i++)
            {
                expressions.Add(
                    Call(
                        EntityQueryModelVisitor.QueryContextParameter,
                        _setParameterMethodInfo,
                        Expression.ArrayIndex(prmNames, Expression.Constant(i)),
                        Expression.ArrayIndex(prmValues, Expression.Constant(i))));
            }

            expressions.Add(Query);

            var lambda = Lambda(
                Block(expressions),
                prmNames,
                prmValues);
                //Parameter(typeof(string[]), "names"),
                //Parameter(typeof(object[]), "values"));

            var methodcall = Expression.Call(
                generic,
                EntityQueryModelVisitor.QueryContextParameter,
                lambda,
                Expression.NewArrayInit(typeof(string), Parameters.Select(p => Constant(p.Name))),
                Expression.NewArrayInit(typeof(object), ParameterValues));

            return methodcall;








            //InjectParametersMethod


            //var expressions = new List<Expression>();

            //for (var i = 0; i < Parameters.Count; i++)
            //{
            //    expressions.Add(
            //        Call(
            //            EntityQueryModelVisitor.QueryContextParameter,
            //            _setParameterMethodInfo,
            //            Constant(Parameters[i].Name),
            //            ParameterValues[i]));
            //}

            //expressions.Add(Query);

            //return Block(expressions);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }
    }
}
