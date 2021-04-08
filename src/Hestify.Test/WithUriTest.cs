using System;
using Hestify.Test.Helpers;
using Xunit;

namespace Hestify.Test
{
    public class WithUriTest : HestifyTestBase
    {
        [Fact]
        public void AbsoluteUriHasAPath()
        {
            Client
                .WithUri(new Uri("e", UriKind.Relative))
                .WithUri(new Uri("https://test.com/a/"))
                .BuildRequest().Should().MatchUri("https://test.com/a/e");
        }

        [Fact]
        public void MultipleRelativeUriTest()
        {
            Client
                .WithUri(new Uri("/a/b", UriKind.Relative))
                .WithUri(new Uri("c/d", UriKind.Relative))
                .WithUri(new Uri("https://test.com"))
                .BuildRequest().Should().MatchUri("https://test.com/a/b/c/d");
        }

        [Fact]
        public void KeepBaseUriPath_When_SecondaryRelativeUriStartWithRoot()
        {
            Client
                .WithUri(new Uri("/a/b", UriKind.Relative))
                .WithUri(new Uri("/c/d", UriKind.Relative))
                .WithUri(new Uri("https://test.com/b"))
                .BuildRequest().Should().MatchUri("https://test.com/b/c/d");
        }

        [Fact]
        public void ResetRelativePath_When_SecondPathStartWithRoot()
        {
            Client
                .WithUri(new Uri("/a/b", UriKind.Relative))
                .WithUri(new Uri("/c/d", UriKind.Relative))
                .WithUri(new Uri("https://test.com"))
                .BuildRequest().Should().MatchUri("https://test.com/c/d");
        }

        [Fact]
        public void WithUriString()
        {
            Client
                .WithUri("/a/b")
                .WithUri("/c/d")
                .WithUri("e")
                .WithUri("f/g")
                .WithUri("https://test.com/b/")
                .BuildRequest().Should().MatchUri("https://test.com/b/c/d/e/f/g");
        }

        [Fact]
        public void KeepLastNavigation()
        {
            Client
                .WithUri("/a/b/")
                .WithUri("https://test.com")
                .BuildRequest().Should().MatchUri("https://test.com/a/b/");
        }

        [Fact]
        public void HostAndRelativeClientBuild()
        {
            Client
                .WithUri("https://test.com")
                .WithUri("/a/b/")
                .BuildRequest().Should().MatchUri("https://test.com/a/b/");
        }

        [Fact]
        public void ChangeBaseUri()
        {
            Client
                .WithUri(new Uri("/a/b", UriKind.Relative))
                .WithUri(new Uri("/c/d", UriKind.Relative))
                .WithUri(new Uri("https://test.com/b"))
                .WithUri(new Uri("https://changed.com/b"))
                .BuildRequest().Should().MatchUri("https://changed.com/b/c/d");
        }
    }
}