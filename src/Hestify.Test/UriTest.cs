using System;
using FluentAssertions;
using Xunit;

namespace Hestify.Test
{
    public class UriTest
    {
        [Fact]
        public void Should_RESULT_When_CONDITION()
        {
            var uri = new Uri("http://www.server.com/api/path/to?a=b&c=true");

            uri.Should().NotBe(default);
        }
    }
}