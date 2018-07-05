// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class ProceduralQueryExpressionGenerator
    {
        private Expression _originalExpression;
        private int _seed;

        private MethodInfo _whereMethodInfo;
        private MethodInfo _selectMethodInfo;
        private MethodInfo _orderByMethodInfo;
        private MethodInfo _orderByDescendingMethodInfo;
        private MethodInfo _thenByMethodInfo;
        private MethodInfo _thenByDescendingMethodInfo;

        public ProceduralQueryExpressionGenerator()
        {
            _whereMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.Where) && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Count() == 2).Single();
            _selectMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.Select) && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Count() == 2).Single();
            _orderByMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.OrderBy) && m.GetParameters().Count() == 2).Single();
            _orderByDescendingMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.OrderByDescending) && m.GetParameters().Count() == 2).Single();
            _thenByMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.ThenBy) && m.GetParameters().Count() == 2).Single();
            _thenByDescendingMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.ThenByDescending) && m.GetParameters().Count() == 2).Single();
        }

        public Expression Generate(Expression expression, int seed)
        {

            _originalExpression = expression;
            _seed = seed;

            return expression;
        }



        // add new stuff









        // modify current stuff


        






        // add stuff that doesnt change the result???









    }
}
