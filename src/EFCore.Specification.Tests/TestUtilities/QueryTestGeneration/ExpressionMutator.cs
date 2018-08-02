// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration
{
    public abstract class ExpressionMutator
    {
        protected static MethodInfo WhereMethodInfo;
        protected static MethodInfo SelectMethodInfo;
        protected static MethodInfo OrderByMethodInfo;
        protected static MethodInfo OrderByDescendingMethodInfo;
        protected static MethodInfo ThenByMethodInfo;
        protected static MethodInfo ThenByDescendingMethodInfo;
        protected static MethodInfo TakeMethodInfo;
        protected static MethodInfo JoinMethodInfo;

        protected DbContext Context { get; }

        static ExpressionMutator()
        {
            WhereMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.Where) && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Count() == 2).Single();
            SelectMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.Select) && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Count() == 2).Single();
            OrderByMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.OrderBy) && m.GetParameters().Count() == 2).Single();
            OrderByDescendingMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.OrderByDescending) && m.GetParameters().Count() == 2).Single();
            ThenByMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.ThenBy) && m.GetParameters().Count() == 2).Single();
            ThenByDescendingMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.ThenByDescending) && m.GetParameters().Count() == 2).Single();
            TakeMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.Take)).Single();
            JoinMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.Join) && m.GetParameters().Count() == 5).Single();
        }

        public ExpressionMutator(DbContext context)
        {
            Context = context;
        }

        private static bool IsQueryableType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryable<>);

        protected static bool IsQueryableResult(Expression expression)
            => IsQueryableType(expression.Type)
                || expression.Type.GetInterfaces().Any(i => IsQueryableType(i));

        private static bool IsOrderedQueryableType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IOrderedQueryable<>);

        protected static bool IsOrderedQueryableResult(Expression expression)
            => IsOrderedQueryableType(expression.Type)
                || expression.Type.GetInterfaces().Any(i => IsOrderedQueryableType(i));

        protected static List<PropertyInfo> FilterPropertyInfos(Type type, List<PropertyInfo> properties)
        {
            if (type == typeof(string))
            {
                properties = properties.Where(p => p.Name != "Chars").ToList();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                properties = properties.Where(p => p.Name != "Item" && p.Name != "Capacity").ToList();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>)
                || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>)))
            {
                properties = properties.Where(p => p.Name != "IsReadOnly").ToList();
            }

            if (type.IsArray)
            {
                properties = properties.Where(p => p.Name != "Rank" && p.Name != "IsFixedSize" && p.Name != "IsSynchronized").ToList();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                properties = properties.Where(p => p.Name != "Value").ToList();
            }

            if (type == typeof(Gear)
                || type == typeof(Officer))
            {
                properties = properties.Where(p => p.Name != "IsMarcus").ToList();
            }

            if (type == typeof(Order))
            {
                var ignoredOrderProperties = new[]
                {
                    nameof(Order.Freight),
                    nameof(Order.RequiredDate),
                    nameof(Order.ShipAddress),
                    nameof(Order.ShipCity),
                    nameof(Order.ShipCountry),
                    nameof(Order.ShipName),
                    nameof(Order.ShipPostalCode),
                    nameof(Order.ShipRegion),
                    nameof(Order.ShipVia),
                    nameof(Order.ShippedDate)
                };

                properties = properties.Where(p => !ignoredOrderProperties.Contains(p.Name)).ToList();
            }

            return properties;
        }

        protected bool IsEntityType(Type type)
            => Context.Model.GetEntityTypes().Select(et => et.ClrType).Contains(type);

        public abstract bool IsValid(Expression expression);
        public abstract Expression Apply(Expression expression, Random random);

        protected class ExpressionInjector : ExpressionVisitor
        {
            private Expression _expressionToInject;
            private Func<Expression, Expression> _injectionPattern;

            public ExpressionInjector(Expression expressionToInject, Func<Expression, Expression> injectionPattern)
            {
                _expressionToInject = expressionToInject;
                _injectionPattern = injectionPattern;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _expressionToInject)
                {
                    return _injectionPattern(node);
                }
                else
                {
                    return base.Visit(node);
                }
            }
        }
    }
}
