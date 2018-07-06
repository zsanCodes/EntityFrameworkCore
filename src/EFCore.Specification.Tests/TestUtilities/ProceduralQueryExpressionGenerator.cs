// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public abstract class ExpressionMutator
    {
        protected static MethodInfo WhereMethodInfo;
        protected static MethodInfo SelectMethodInfo;
        protected static MethodInfo OrderByMethodInfo;
        protected static MethodInfo OrderByDescendingMethodInfo;
        protected static MethodInfo ThenByMethodInfo;
        protected static MethodInfo ThenByDescendingMethodInfo;

        static ExpressionMutator()
        {
            WhereMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.Where) && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Count() == 2).Single();
            SelectMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.Select) && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Count() == 2).Single();
            OrderByMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.OrderBy) && m.GetParameters().Count() == 2).Single();
            OrderByDescendingMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.OrderByDescending) && m.GetParameters().Count() == 2).Single();
            ThenByMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.ThenBy) && m.GetParameters().Count() == 2).Single();
            ThenByDescendingMethodInfo = typeof(Queryable).GetMethods().Where(m => m.Name == nameof(Queryable.ThenByDescending) && m.GetParameters().Count() == 2).Single();
        }

        private bool IsQueryableType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryable<>);

        protected bool IsQueryableResult(Expression expression)
            => IsQueryableType(expression.Type)
                || expression.Type.GetInterfaces().Any(i => IsQueryableType(i));

        public abstract Expression Apply(Expression expression, Random random);
        public virtual bool IsValid(Expression expression) => true;
    }

    public class AppendSelectConstantAtTheEnd : ExpressionMutator
    {
        private List<(Type type, Expression expression)> _expressions = new List<(Type type, Expression expression)>
        {
            (type: typeof(int), expression: Expression.Constant(42, typeof(int))),
            (type: typeof(int?), expression: Expression.Constant(7, typeof(int?))),
            (type: typeof(int?), expression: Expression.Constant(null, typeof(int?))),
            (type: typeof(string), expression: Expression.Constant("Foo", typeof(string))),
            (type: typeof(string), expression: Expression.Constant(null, typeof(string))),
        };

        public override bool IsValid(Expression expression) => IsQueryableResult(expression);

        public override Expression Apply(Expression expression, Random random)
        {
            var i = random.Next(_expressions.Count);

            var typeArgument = expression.Type.GetGenericArguments()[0];
            var select = SelectMethodInfo.MakeGenericMethod(typeArgument, _expressions[i].type);
            var lambda = Expression.Lambda(_expressions[i].expression, Expression.Parameter(typeArgument, "prm"));
            var resultExpression = Expression.Call(select, expression, lambda);

            return resultExpression;
        }
    }

    public class AppendIdentitySelectAtTheEnd : ExpressionMutator
    {
        public override bool IsValid(Expression expression) => IsQueryableResult(expression);

        public override Expression Apply(Expression expression, Random random)
        {
            var typeArgument = expression.Type.GetGenericArguments()[0];
            var select = SelectMethodInfo.MakeGenericMethod(typeArgument, typeArgument);
            var prm = Expression.Parameter(typeArgument, "prm");
            var lambda = Expression.Lambda(prm, prm);
            var resultExpression = Expression.Call(select, expression, lambda);

            return resultExpression;
        }
    }

    public class AppendSelectSinglePropertyAtTheEnd : ExpressionMutator
    {
        private bool HasValidPropertyToSelect(Expression expression)
            => expression.Type.GetGenericArguments()[0].GetProperties().Any();

        public override bool IsValid(Expression expression)
            => IsQueryableResult(expression)
                && HasValidPropertyToSelect(expression);

        public override Expression Apply(Expression expression, Random random)
        {
            var typeArgument = expression.Type.GetGenericArguments()[0];
            var properties = typeArgument.GetProperties().ToList();

            if (typeArgument == typeof(string))
            {
                properties = properties.Where(p => p.Name != "Chars").ToList();
            }

            var i = random.Next(properties.Count);

            var select = SelectMethodInfo.MakeGenericMethod(typeArgument, properties[i].PropertyType);
            var prm = Expression.Parameter(typeArgument, "prm");
            var lambda = Expression.Lambda(Expression.Property(prm, properties[i]), prm);
            var resultExpression = Expression.Call(select, expression, lambda);

            return resultExpression;
        }
    }

    public class ProceduralQueryExpressionGenerator
    {
        private List<ExpressionMutator> _mutators;

        public ProceduralQueryExpressionGenerator()
        {
            _mutators = new List<ExpressionMutator>
            {
                //new AppendSelectConstantAtTheEnd(),
                //new AppendIdentitySelectAtTheEnd(),
                new AppendSelectSinglePropertyAtTheEnd(),
            };
        }

        public Expression Generate(Expression expression, int seed)
        {
            var random = new Random(seed);

            var validMutators = _mutators.Where(m => m.IsValid(expression)).ToList();
            if (validMutators.Any())
            {
                var i = random.Next(validMutators.Count);
                var result = validMutators[i].Apply(expression, random);

                return result;
            }

            return expression;
        }
    }


    public class ProcedurallyGeneratedQueryExecutor
    {
        private static Dictionary<string, List<string>> _knownFailingTests = new Dictionary<string, List<string>>();

        static ProcedurallyGeneratedQueryExecutor()
        {
            AddExpectedFailure("Default_if_empty_top_level", "Object reference not set to an instance of an object."); // 12567
            AddExpectedFailure("Default_if_empty_top_level_positive", "Object reference not set to an instance of an object."); // 12567
            AddExpectedFailure("Default_if_empty_top_level_projection", "Object reference not set to an instance of an object."); // 12567

            AddExpectedFailure("Except_simple", "cannot be used for"); // 12568
            AddExpectedFailure("Except_dbset", "cannot be used for"); // 12568
            AddExpectedFailure("Except_nested", "cannot be used for"); // 12568

            AddExpectedFailure("GroupBy_aggregate_Pushdown", "Invalid column name 'c'."); // 12569
            AddExpectedFailure("GroupBy_with_orderby_take_skip_distinct", "Invalid column name 'c'."); // 12569

            AddExpectedFailure("Default_if_empty_top_level_arg", "Expression of type 'Microsoft.EntityFrameworkCore.TestModels.Northwind.Employee' cannot be used for parameter of type"); // 12572

            AddExpectedFailure("GroupBy_Select_First_GroupBy", "Query source (from Customer c in [g]) has already been associated with an expression."); // 12573

            AddExpectedFailure("Join_Customers_Orders_Skip_Take", "Object reference not set to an instance of an object."); // 12574
            AddExpectedFailure("Join_Customers_Orders_Projection_With_String_Concat_Skip_Take", "Object reference not set to an instance of an object."); // 12574
            AddExpectedFailure("Join_Customers_Orders_Orders_Skip_Take_Same_Properties", "Object reference not set to an instance of an object."); // 12574

            AddExpectedFailure("SelectMany_navigation_property", "The property '' on entity type 'Level2' could not be found. Ensure that the property exists and has been included in the model."); // 12575
            AddExpectedFailure("SelectMany_navigation_property_and_filter_before", "The property '' on entity type 'Level2' could not be found. Ensure that the property exists and has been included in the model."); // 12575
            AddExpectedFailure("SelectMany_navigation_property_and_filter_after", "The property '' on entity type 'Level2' could not be found. Ensure that the property exists and has been included in the model."); // 12575
            AddExpectedFailure("SelectMany_where_with_subquery", "The property '' on entity type 'Level2' could not be found. Ensure that the property exists and has been included in the model."); // 12575

            AddExpectedFailure("GroupBy_with_orderby_take_skip_distinct", "Unable to cast object of type 'Remotion.Linq.Clauses.ResultOperators.DistinctResultOperator' to type 'Remotion.Linq.Clauses.ResultOperators.GroupResultOperator'."); // 12576

            AddExpectedFailure("GroupBy_aggregate_Pushdown", "Incorrect syntax near the keyword 'AS'."); // 12577

            AddExpectedFailure("Select_nav_prop_reference_optional1", "Nullable object must have a value."); // 12578
            AddExpectedFailure("Select_nav_prop_reference_optional1_via_DefaultIfEmpty", "Nullable object must have a value."); // 12578






            

















        }

        private static void AddExpectedFailure(string testName, string expectedException)
        {
            if (_knownFailingTests.ContainsKey(testName))
            {
                _knownFailingTests[testName].Add(expectedException);
            }
            else
            {
                _knownFailingTests[testName] = new List<string> { expectedException };
            }
        }

        public void Execute<TElement>(IQueryable<TElement> query)
        {
            var seed = new Random().Next();

            seed = 1847891685;

            var expression = query.Expression;
            var queryGenerator = new ProceduralQueryExpressionGenerator();
            var newExpression = queryGenerator.Generate(expression, seed);

            var newQuery = query.Provider.CreateQuery(newExpression);

            try
            {
                foreach (var r in newQuery)
                {
                }
            }
            catch (Exception exception)
            {
                if (exception.Message == @"Invalid column name 'Key'.") // 12564
                {
                }
                else if (exception.Message.StartsWith(@"Error generated for warning 'Microsoft.EntityFrameworkCore.Query.IncludeIgnoredWarning"))
                {
                }
                else if (exception.Message.Contains(@"The expected type was 'System.Int64' but the actual value was of type")) // 12570
                {
                }
                else
                {
                    var stackTrace = new StackTrace();

                    var testMethodFrameIndex = stackTrace.GetFrames().Select((f, i) => new { i, assertQuery = f.GetMethod().Name.StartsWith("AssertQuery") }).Where(r => r.assertQuery).Last().i;
                    var frame = stackTrace.GetFrame(testMethodFrameIndex + 1);
                    var testName = frame.GetMethod().Name;

                    if (_knownFailingTests.ContainsKey(testName)
                        && _knownFailingTests[testName].Any(e => exception.Message.Contains(e)))
                    {
                    }
                    else
                    {
                        Console.WriteLine("SEED: " + seed + " TEST: " + testName);

                        throw;
                    }
                }
            }
        }
    }
}
