using System.Net;
using Hestify.Test.Helpers;
using Xunit;

namespace Hestify.Test
{
    public class HeaderTest : HestifyTestBase
    {
        public HeaderTest() : base(true)
        {
        }

        [Fact]
        public void Should_HaveHeaderOnMessage_When_WithHeaderCalledWithCostumeHeader()
        {
            const string customheader = "CustomHeader";
            Client
                .WithHeader(customheader, "Value")
                .BuildRequest()
                .Should().ContainsHeaderValue(customheader, "Value");
        }

        [Fact]
        public void Should_HaveHeaderOnMessage_When_WithHeaderCalledWithStandardEnum()
        {
            Client
                .WithHeader(HttpRequestHeader.Authorization, "Bearer Token")
                .BuildRequest()
                .Should().ContainsHeader(HttpRequestHeader.Authorization)
                .And.ContainsHeaderValue(HttpRequestHeader.Authorization, "Bearer Token");
        }

        [Fact]
        public void Should_HaveMultipleValuesOnHeader_When_CallMultipleTimes()
        {
            const string customeHeader = "SomeHeader";
            Client
                .WithHeader(customeHeader, "Value1")
                .WithHeader(customeHeader, "Value2")
                .BuildRequest()
                .Should().ContainsHeaderValueExactly(customeHeader, "Value1", "Value2");
        }
    }
}