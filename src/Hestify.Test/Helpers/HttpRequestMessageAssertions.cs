using System.Net.Http;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using RichardSzalay.MockHttp;

namespace Hestify.Test.Helpers
{
    public class HttpRequestMessageAssertions : ReferenceTypeAssertions<HttpRequestMessage, HttpRequestMessageAssertions>
    {
        protected override string Identifier => "httprequestmessage";

        public HttpRequestMessageAssertions(HttpRequestMessage subject) : base(subject)
        {
        }
        
        public AndConstraint<HttpRequestMessageAssertions> MatchUri(string uri)
        {
            Execute.Assertion
                .ForCondition(new MockedRequest(uri).Matches(Subject))
                .FailWith("Expected request uri to be {0}, but found {1}.", uri, Subject.RequestUri?.ToString());
            
            return new(this);
        }
    }

    public static class HttpRequestMessageAssertionsExtensions
    {
        public static HttpRequestMessageAssertions Should(this HttpRequestMessage message)
        {
            return new(message);
        }
    }
}