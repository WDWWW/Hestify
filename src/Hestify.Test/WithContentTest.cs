using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Hestify.Test
{
    public class WithContentTest : HestifyTestBase
    {

        [Fact]
        public async Task Should_IncludeJsonStringContentWithMediaTypeInMessage_When_WithJsonContent()
        {
            // Given
            // When
            var content = Client.WithJsonContent(new {A = "B", C = true})
                .RequestMessage
                .Content;

            // Then
            content!.Headers.ContentType!.MediaType.Should().Be("application/json");
            var json = await content.ReadAsStringAsync();
            json.Should().Be("{\"A\":\"B\",\"C\":true}");
        }

        [Fact]
        public async Task Should_IncludeXmlStringContentWithMediaTypeInMessage_When_WithXmlContent()
        {
            // Given
            // When
            var content = Client.WithXmlContent(new {A = "B", C = true})
                .RequestMessage
                .Content;

            // Then
            content!.Headers.ContentType!.MediaType.Should().Be("application/xml");
            var json = await content.ReadAsStringAsync();
            json.Should().Be("{\"A\":\"B\",\"C\":true}");
        }
    }
}
