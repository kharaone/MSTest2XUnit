// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using Xunit;

namespace XUnitConverter.Tests
{
    public class IgnoreToSkipConverterTest : ConverterTestBase
    {
        protected override ConverterBase CreateConverter()
        {
            return new IgnoreToSkipConverter();
        }

        [Fact]
        public async Task TestIgnoreOnSeparateAttributeList()
        {
            string source = @"
public class Tests
{
    [Fact]
    [Ignore]
    public void TestA()
    {
        int actual = 1;
    }
}
";
            string expected = @"
public class Tests
{
    [Fact(Skip = ""Ignored in MSTest"")]
    public void TestA()
    {
        int actual = 1;
    }
}
";

            await Verify(source, expected);
        }

        [Fact]
        public async Task TestIgnoreOnSameAttributeList()
        {
            string source = @"
public class Tests
{
    [Fact,Ignore]
    public void TestA()
    {
        int actual = 1;
    }
}
";
            string expected = @"
public class Tests
{
    [Fact(Skip = ""Ignored in MSTest"")]
    public void TestA()
    {
        int actual = 1;
    }
}
";

            await Verify(source, expected);
        }


        [Fact]
        public async Task TestIgnoreOnSomething()
        {
            string source = @"
public class Tests
{
    [Something,Ignore]
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


        [Fact]
        public async Task TestIgnoreOnClass()
        {
            string source = @"
[Ignore]
public class Tests
{
    [Fact]
    public void TestA()
    {
        int actual = 1;
    }
}
";
            string expected = @"
public class Tests
{
    [Fact(Skip = ""Ignored in MSTest"")]
    public void TestA()
    {
        int actual = 1;
    }
}
";

            await Verify(source, expected);
        }

        [Fact]
        public async Task TestIgnoreOnClassAttributeList()
        {
            string source = @"
[Something,Ignore]
public class Tests
{
    [Fact]
    public void TestA()
    {
        int actual = 1;
    }
}
";
            string expected = @"
[Something]
public class Tests
{
    [Fact(Skip = ""Ignored in MSTest"")]
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
