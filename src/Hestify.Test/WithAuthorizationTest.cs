using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace Hestify.Test
{
    public class WithAuthorizationTest : HestifyTestBase
    {
        [Fact]
        public void Should_IncludeBearerTokenInRequest_When_WithBearerToken()
        {
            var headerValue = Client.WithBearerToken("TOKEN").BuildRequest()
                .Headers
                .Authorization;

            headerValue!.Scheme.Should().Be("Bearer");
            headerValue.Parameter.Should().Be("TOKEN");
        }

        [Fact]
        public void Should_IncludeBasicTokenInRequest_When_WithBasicToken()
        {
            // Given
            // When
            var headerValue = Client.WithBasicToken("TOKEN").BuildRequest()
                .Headers
                .Authorization;

            // Then
            headerValue!.Scheme.Should().Be("Basic");
            headerValue.Parameter.Should().Be("TOKEN");
        }

        [Fact]
        public void Should_IncludeSchemeAndToken_When_WithAuthorization()
        {
            // Given
            // When
            var headerValue = Client.WithAuthorization("SCHEME", "TOKEN").BuildRequest()
                .Headers
                .Authorization;

            // Then
            headerValue!.Scheme.Should().Be("SCHEME");
            headerValue.Parameter.Should().Be("TOKEN");
        }
    }
}