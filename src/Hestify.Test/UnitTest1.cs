using System;
using FluentAssertions;
using Xunit;

namespace Hestify.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var uri = new Uri("http://www.naver.com/index.html");


            uri.Should().NotBe(default);
        }
    }
}