using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Hestify.Test.Helpers;
using Xunit;

namespace Hestify.Test
{
    public class WithContentTest : HestifyTestBase
    {

        [Fact]
        public void Should_IncludeJsonStringContentWithMediaTypeInMessage_When_WithJsonContent()
        {
            Client
                .WithUri("https://www.test.com/api/resource")
                .WithJsonContent(new {A = "B", C = true})
                .BuildRequest()
                .Should().BeMediaType("application/json").And.HasTextContent(@"{""A"":""B"",""C"":true}");
        }

        [Fact]
        public void Should_IncludeXmlStringContentWithMediaTypeInMessage_When_WithXmlContent()
        {
            Client
                .WithUri("https://www.test.com/api/resource")
                .WithXmlContent(new TestBody {A = "B", C = true})
                .BuildRequest()
                .Should().BeMediaType("application/xml")
                .And.HasTextContent(@"<?xml version=""1.0"" encoding=""utf-8""?>
<TestBody xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <A>B</A>
  <C>true</C>
</TestBody>");
        }
    }

    public class TestBody
    {
        public string A { get; set; }
        public bool C { get; set; }
    }
}
