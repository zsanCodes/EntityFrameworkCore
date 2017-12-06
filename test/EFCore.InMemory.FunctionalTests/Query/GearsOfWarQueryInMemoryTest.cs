// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQueryInMemoryTest : GearsOfWarQueryTestBase<GearsOfWarQueryInMemoryFixture>
    {
        public GearsOfWarQueryInMemoryTest(GearsOfWarQueryInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        public override void Concat_with_collection_navigations()
        {
            base.Concat_with_collection_navigations();
        }


        public override void Select_subquery_distinct_firstordefault()
        {
            base.Select_subquery_distinct_firstordefault();
        }



        public override void Select_correlated_filtered_collection()
        {
            base.Select_correlated_filtered_collection();
        }


        public override void Where_subquery_distinct_singleordefault_boolean()
        {
            base.Where_subquery_distinct_singleordefault_boolean();
        }
    }
}
