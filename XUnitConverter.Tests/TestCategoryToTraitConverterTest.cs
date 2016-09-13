// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using Xunit;

namespace XUnitConverter.Tests
{
    public class TestCategoryToTraitConverterTest : ConverterTestBase
    {
        protected override ConverterBase CreateConverter()
        {
            return new TestCategoryToTraitConverter();
        }

        [Fact]
        public async Task TestCategoryToTraitOnSeparateAttributeList()
        {
            string source = @"
public class Tests
{
    [Something]
    [TestCategory(""SomeCategoryName"")]
    public void TestA()
    {
        int actual = 1;
    }
}
";
            string expected = @"
public class Tests
{
    [Something]
    [Trait(""Category"", ""SomeCategoryName"")]
    public void TestA()
    {
        int actual = 1;
    }
}
";

            await Verify(source, expected);
        }

        [Fact]
        public async Task TestCategoryToTraitOnSameAttributeList()
        {
            string source = @"
public class Tests
{
    [Something, SomeOther, TestCategory(""SomeCategoryName"")]
    public void TestA()
    {
        int actual = 1;
    }
}
";
            string expected = @"
public class Tests
{
    [Trait(""Category"", ""SomeCategoryName"")]
    [Something, SomeOther]
    public void TestA()
    {
        int actual = 1;
    }
}
";

            await Verify(source, expected);
        }


        //[Fact(Skip="Didn't account for this case")]
        [Fact]
        public async Task TestMultipleTestCategoriesOnSameAttributeList()
        {
            string source = @"
public class Tests
{
    [TestCategory(""SomeCategoryName1""),TestCategory(""SomeCategoryName2"")]
    public void TestA()
    {
        int actual = 1;
    }
}
";
            string expected = @"
public class Tests
{
    [Trait(""Category"", ""SomeCategoryName1"")]
    [Trait(""Category"", ""SomeCategoryName2"")]
    public void TestA()
    {
        int actual = 1;
    }
}
";

            await Verify(source, expected);
        }

        [Fact]
        public async Task TestMultipleTestCategoriesOnDifferentAttributeList()
        {
            string source = @"
public class Tests
{
    [TestCategory(""SomeCategoryName1"")]
    [TestCategory(""SomeCategoryName2"")]
    public void TestA()
    {
        int actual = 1;
    }
}
";
            string expected = @"
public class Tests
{
    [Trait(""Category"", ""SomeCategoryName1"")]
    [Trait(""Category"", ""SomeCategoryName2"")]
    public void TestA()
    {
        int actual = 1;
    }
}
";

            await Verify(source, expected);
        }

        [Fact]
        public async Task TestCategoryToTraitIgnoreNonCategoryAttributes()
        {
            string source = @"
public class Tests
{
    [Something]
    public void TestA()
    {
        int actual = 1;
    }
}
";
            string expected = @"
public class Tests
{
    [Something]
    public void TestA()
    {
        int actual = 1;
    }
}
";

            await Verify(source, expected);
        }
    }
}
