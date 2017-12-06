// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SimpleQueryInMemoryTest : SimpleQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public SimpleQueryInMemoryTest(
            NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
            // ReSharper disable once UnusedParameter.Local
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        public override void Outer_parameter_in_selector()
        {
            base.Outer_parameter_in_selector();
        }


        public override void SelectMany_Joined_DefaultIfEmpty2()
        {
            base.SelectMany_Joined_DefaultIfEmpty2();
        }



        public override void Contains_with_DateTime_Date()
        {
            base.Contains_with_DateTime_Date();
        }


        public override void Complex_query_with_repeated_nested_query_model_compiles_correctly()
        {
            base.Complex_query_with_repeated_nested_query_model_compiles_correctly();
        }

        public override void Select_nested_collection_deep()
        {
            base.Select_nested_collection_deep();
        }



    }
}
