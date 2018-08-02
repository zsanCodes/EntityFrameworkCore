// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration
{
    public class AppendOrderByIdentityExpressionMutator : ExpressionMutator
    {
        public AppendOrderByIdentityExpressionMutator(DbContext context)
            : base(context)
        {
        }

        public override bool IsValid(Expression expression)
            => IsQueryableResult(expression)
            && expression.Type.GetGenericArguments()[0].GetInterfaces().Any(i => i == typeof(IComparable));

        public override Expression Apply(Expression expression, Random random)
        {
            var typeArgument = expression.Type.GetGenericArguments()[0];

            var isDescending = random.Next(3) == 0;
            var orderBy = isDescending
                ? OrderByDescendingMethodInfo.MakeGenericMethod(typeArgument, typeArgument)
                : OrderByMethodInfo.MakeGenericMethod(typeArgument, typeArgument);

            var prm = Expression.Parameter(typeArgument, "prm");
            var lambda = Expression.Lambda(prm, prm);
            var resultExpression = Expression.Call(orderBy, expression, lambda);

            return resultExpression;
        }
    }
}
