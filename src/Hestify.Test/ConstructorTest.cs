using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace Hestify.Test
{
    public class ConstructorTest
    {

        [Fact]
        public void Should_CreateClientWithOutAnyBaseAddress_When_ThereAreNoParams()
        {
            // Given
            // When
            using var client = new HestifyClient();

            // Then
            client.Client.BaseAddress.Should().Be(default);
        }

        [Fact]
        public void Should_CreateClientWithBaseAddress_When_CreateWithBaseAddressParam()
        {
            // Given
            var baseAddress = "http://localhost:8080";

            // When
            using var client = new HestifyClient(baseAddress);

            // Then
            var uri = client.Client.BaseAddress;
            uri.Should().NotBe(default);
            uri!.Host.Should().Be("localhost");
            uri.Port.Should().Be(8080);
            uri.Scheme.Should().Be("http");
        }

        [Fact]
        public void Should_UseCustomClient_When_ParamIsCustomHttpClient()
        {
            // Given
            var customClient = new CustomClient();

            // When
            var hestifyClient = new HestifyClient(customClient);

            // Then
            hestifyClient.Client.Should().BeOfType<CustomClient>();
        }

        [Fact]
        public void Should_UseBaseAddress_When_HttpClientAndBaseAddressIsProvided()
        {
            // Given
            // When
            var baseAddress = "http://localhost:8080/api";
            var hestifyClient = new HestifyClient(new CustomClient(), baseAddress);

            // Then
            var baseUri = hestifyClient.Client.BaseAddress;
            baseUri.Should().NotBe(default);
            baseUri!.AbsoluteUri.Should().Be(baseAddress);
        }
        
        class CustomClient : HttpClient
        {
        }
    }
}