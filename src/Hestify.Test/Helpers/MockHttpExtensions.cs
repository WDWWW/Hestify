using System.Net;
using System.Net.Http;
using FluentAssertions;
using RichardSzalay.MockHttp;

namespace Hestify.Test.Helpers
{
    public static class MockHttpExtensions
    {
        public static MockedRequest Respond(this MockedRequest request, HttpStatusCode code) => request
            .Respond(_ => new HttpResponseMessage(code));

        public static MockedRequest RespondOk(this MockedRequest request) => request
            .Respond(HttpStatusCode.OK);

        public static void MatchAtLeastOne(this MockHttpMessageHandler handler, MockedRequest request) => handler
            .GetMatchCount(request).Should().BeGreaterOrEqualTo(1);
    }


    public static class HestifyExpectationExtensions
    {
        public static void Match(this HestifyClient client, MockedRequest request)
        {
            request.Matches(client.BuildRequest()).Should().BeTrue("Build request does not match with mocked request instance.");
        }

        public static void Match(this HestifyClient client, string uri)
        {

            new MockedRequest(uri).Matches(client.BuildRequest()).Should()
                .BeTrue("Build request does not match with mocked request instance.");
        }
    }
}