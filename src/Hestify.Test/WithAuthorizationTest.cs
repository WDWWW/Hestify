using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using Hestify.Test.Helpers;
using Xunit;

namespace Hestify.Test
{
    public class WithAuthorizationTest : HestifyTestBase
    {
        public WithAuthorizationTest() : base(true)
        {
        }

        [Fact]
        public void Should_IncludeBearerTokenInRequest_When_WithBearerToken()
        {
            Client.WithBearerToken("TOKEN")
                .BuildRequest()
                .Should().HaveAuthenticationHeaderValue(new ("Bearer", "TOKEN"));
        }

        [Fact]
        public void Should_IncludeBasicTokenInRequest_When_WithBasicToken()
        {
            Client.WithBasicToken("TOKEN")
                .BuildRequest()
                .Should().HaveAuthenticationHeaderValue(new("Basic", "TOKEN"));
        }

        [Fact]
        public void Should_IncludeSchemeAndToken_When_WithAuthorization()
        {
            Client.WithAuthorization("SCHEME", "TOKEN")
                .BuildRequest()
                .Should().HaveAuthenticationHeaderValue(new("SCHEME", "TOKEN"));
        }
    }
}