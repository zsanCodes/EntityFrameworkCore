// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryNavigationsInMemoryTest : QueryNavigationsTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public QueryNavigationsInMemoryTest(
            NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        public override void Collection_where_nav_prop_all()
        {
            base.Collection_where_nav_prop_all();
        }


        public override void Select_collection_navigation_simple()
        {
            base.Select_collection_navigation_simple();
        }

        public override void Collection_select_nav_prop_sum()
        {
            base.Collection_select_nav_prop_sum();
        }



        public override void Navigation_projection_on_groupjoin_qsre()
        {
            base.Navigation_projection_on_groupjoin_qsre();
        }


        public override void Where_subquery_on_navigation2()
        {
            base.Where_subquery_on_navigation2();
        }




    }
}
