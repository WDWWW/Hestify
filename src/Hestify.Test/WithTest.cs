using System;
using FluentAssertions;
using Xunit;

namespace Hestify.Test
{
    public class WithTest : IDisposable
    {
        private HestifyClient _client;

        public WithTest()
        {
            _client = new HestifyClient();
        }

        [Fact]
        public void Should_CreatedMessageHaveSomeHeader_When_WithSomeHeader()
        {
            // Given
            // When
            var message = _client.With(_ => _.Headers.Add("Some", "Value"))
                .RequestMessage;

            // Then
            message.Headers.GetValues("Some").Should().Contain("Value");
        }

        [Fact]
        public void Should_MessageHaveMultipleValueOnSomeHeader_When_CallMultipleWithMethodToSetSomeHeader()
        {
            // Given
            // When
            var message = _client
                .With(_ => _.Headers.Add("Some", "Value1"))
                .With(_ => _.Headers.Add("Some", "Value2"))
                .RequestMessage;

            // Then
            message.Headers.GetValues("Some").Should().Contain("Value1")
                .And.Contain("Value2")
                .And.HaveCount(2);
        }

        [Fact]
        public void Should_AllClientHaveDifferenceReference_When_WithIsCalledMultipleTimes()
        {
            // Given
            // When
            var client1 = _client.With(_ => { });
            var client2 = client1.With(_ => { });
            var client3 = client2.With(_ => { });

            // Then
            client1.Should().NotBeSameAs(_client);
            client2.Should().NotBeSameAs(client1);
            client3.Should().NotBeSameAs(client2);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}