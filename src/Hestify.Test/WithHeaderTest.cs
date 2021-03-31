using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using RichardSzalay.MockHttp;
using Xunit;

namespace Hestify.Test
{
    public abstract class HestifyTestBase : IDisposable
    {
        protected HestifyClient Client
        {
            get
            {
                HestifyClient client = new(MockHttp.ToHttpClient());
                _disposables.Add(client);
                return client;
            }
        }

        private readonly IList<IDisposable> _disposables = new List<IDisposable>();

        protected MockHttpMessageHandler MockHttp;

        protected HestifyTestBase()
        {
            MockHttp = new MockHttpMessageHandler();
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables) disposable?.Dispose();

            MockHttp?.Dispose();
        }
    }

    public class HeaderTest : HestifyTestBase
    {
        [Fact]
        public void Should_HaveHeaderOnMessage_When_WithHeaderCalledWithCostumeHeader()
        {
            // Given
            var customheader = "CustomHeader";

            // When
            var message = Client.WithHeader(customheader, "Value").BuildRequest();

            // Then
            message.Headers.GetValues(customheader).Should().Contain("Value");
        }

        [Fact]
        public void Should_HaveHeaderOnMessage_When_WithHeaderCalledWithStandardEnum()
        {
            // Given
            // When
            var message = Client.WithHeader(HttpRequestHeader.Authorization, "Bearer Token").BuildRequest();

            // Then
            var headerValue = message.Headers.Authorization;
            headerValue!.Parameter.Should().Be("Token");
            headerValue!.Scheme.Should().Be("Bearer");
        }

        [Fact]
        public void Should_HaveMultipleValuesOnHeader_When_CallMultipleTimes()
        {
            // Given
            // When
            var customeHeader = "SomeHeader";
            var message = Client
                .WithHeader(customeHeader, "Value")
                .WithHeader(customeHeader, "Value").BuildRequest();

            // Then
            message.Headers.GetValues(customeHeader).Should().HaveCount(2);

        }
    }
}