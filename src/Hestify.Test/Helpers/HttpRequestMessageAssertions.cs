using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using RichardSzalay.MockHttp;
using RichardSzalay.MockHttp.Matchers;

namespace Hestify.Test.Helpers
{
    using And = AndConstraint<HttpRequestMessageAssertions>;
    public class HttpRequestMessageAssertions : ReferenceTypeAssertions<HttpRequestMessage, HttpRequestMessageAssertions>
    {
        protected override string Identifier => "httprequestmessage";

        public HttpRequestMessageAssertions(HttpRequestMessage subject) : base(subject)
        {
        }
        
        public And MatchUri(string uri)
        {
            Execute.Assertion
                .ForCondition(new MockedRequest(uri).Matches(Subject))
                .FailWith("Expected request uri to be {0}, but found {1}.", uri, Subject.RequestUri?.ToString());
            
            return new(this);
        }

        public And BeMediaType(string contentType)
        {
            var mockedRequest = new MockedRequest();
            mockedRequest.With(message => message.Content?.Headers.ContentType?.MediaType == contentType);
            Execute.Assertion
                .ForCondition(mockedRequest.Matches(Subject))
                .FailWith("Expected request content media type to be {0}, but found {1}.", contentType, Subject.Content?.Headers.ContentType?.MediaType);

            return new(this);
        }

        public And HasTextContent(string content)
        {
            Execute.Assertion
                .ForCondition(new MockedRequest().With(new ContentMatcher(content)).Matches(Subject))
                .FailWith("Expected request content to be {0}, but found {1}.", content,
                    Subject.Content?.ReadAsStringAsync().Result);

            return new(this);
        }

        public And ContainsHeader(HttpRequestHeader headers)
        {
            return ContainsHeader(headers.ToString());
        }

        public And ContainsHeader(string header)
        {
            Execute.Assertion
                .ForCondition(Subject.Headers.Contains(header))
                .FailWith("Expected request headers to contains {0}, but there are no the header.", header);
            return new(this);
        }

        public And ContainsHeaderValue(string header, string value)
        {
            ContainsHeader(header);
            Execute.Assertion
                .ForCondition(Subject.Headers.GetValues(header).Contains(value))
                .FailWith("Expected request headers to contains header({0}) value({value}), but there are no the header value.", header, value);
            return new(this);
        }

        public And ContainsHeaderValue(HttpRequestHeader header, string value)
        {
            return ContainsHeaderValue(header.ToString(), value);
        }

        public And ContainsHeaderValueExactly(string header, params string[] values)
        {
            ContainsHeader(header);
            var headerValues = Subject.Headers.GetValues(header).ToList();
            Execute.Assertion
                .ForCondition(headerValues.Union(values).Count() == values.Length)
                .FailWith("Expected request content to be same with ({0}), but the request header value are {1}.", values,
                    headerValues);
            return new(this);
        }

        public And HaveAuthenticationHeaderValue(AuthenticationHeaderValue value)
        {
            ContainsHeader(HttpRequestHeader.Authorization);
            Execute.Assertion
                .ForCondition(Subject.Headers.Authorization!.Equals(value))
                .FailWith("Expected request to have authorization header value ({0}), but the request header value are {1}.",
                    value,
                    Subject.Headers.Authorization);
            return new(this);
        }

        public And HasAuthenticationHeaderScheme(string scheme)
        {
            ContainsHeader(HttpRequestHeader.Authorization);
            Execute.Assertion
                .ForCondition(Subject.Headers.Authorization!.Scheme.Equals(scheme))
                .FailWith(
                    "Expected request to have authorization header scheme ({0}), but the request scheme value are {1}.",
                    scheme,
                    Subject.Headers.Authorization);
            return new(this);
        }

        public And HasAuthenticationHeaderParameter(string parameter)
        {
            ContainsHeader(HttpRequestHeader.Authorization);
            Execute.Assertion
                .ForCondition(Subject.Headers.Authorization!.Parameter.Equals(parameter))
                .FailWith(
                    "Expected request to have authorization header parameter ({0}), but the request scheme value are {1}.",
                    parameter,
                    Subject.Headers.Authorization);
            return new(this);
        }

        public And BeOfHttpMethod(HttpMethod method)
        {
            Execute.Assertion
                .ForCondition(Subject.Method == method)
                .FailWith("Expected request method to be {0}, but request method is {1}.", method.Method, Subject.Method.Method);
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