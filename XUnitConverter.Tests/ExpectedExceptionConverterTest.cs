// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using Xunit;

namespace XUnitConverter.Tests
{
    public class ExpectedExceptionConverterTest : ConverterTestBase
    {
        protected override XUnitConverter.ConverterBase CreateConverter()
        {
            return new XUnitConverter.ExpectedExceptionConverter();
        }

        [Fact]
        public async Task TestExpectedExceptionToAssertThrows()
        {
            string source = @"
public class Tests
{
    [ExpectedException(typeof(InvalidOperationException))]
    public void TestA()
    {
        int actual = 1;
    }
}
";
            string expected = @"
public class Tests
{
    //HACK: Naive implementation of ExpectedException in XUnit
    public void TestA()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            int actual = 1;
        });
    }
}
";

            await Verify(source, expected);
        }

        [Fact]
        public async Task TestExpectedExceptionToAssertThrowsOnSameAttributeList()
        {
            string source = @"
public class Tests
{
    [Something,ExpectedException(typeof(InvalidOperationException))]
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
    //HACK: Naive implementation of ExpectedException in XUnit
    public void TestA()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            int actual = 1;
        });
    }
}
";

            await Verify(source, expected);
        }

        [Fact]
        public async Task TestIgnoreNoneExpectedException()
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
