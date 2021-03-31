using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace Hestify.Test
{
    public class ScenarioTest : HestifyTestBase
    {
        [Fact]
        public async Task Should_RequestWithHeaderAndParams_When_ConfigureServeralOptions()
        {
            // Given
            var content = new {A = "b"};
            var request = MockHttp.When(HttpMethod.Post, "http://localhost:8080/api/index")
                .WithHeaders(HttpRequestHeader.Authorization.ToString(), "Bearer AccessToken")
                .WithQueryString("a", "b")
                .WithQueryString("c", "d")
                .WithContent(JsonConvert.SerializeObject(content))
                .Respond(HttpStatusCode.OK, new StringContent("Hello"));
            
            // When
            var message = await Client.WithUri("http://localhost:8080/api/index")
                .WithHeader(HttpRequestHeader.Authorization, "Bearer AccessToken")
                .WithQuery("a", "b")
                .WithQuery("c", "d")
                .WithJsonContent(content)
                .PostAsync();

            // Then
            message.StatusCode.Should().Be(HttpStatusCode.OK);
            var response = await message.Content.ReadAsStringAsync();
            response.Should().Be("Hello");
            MockHttp.GetMatchCount(request).Should().Be(1);
        }
    }
}