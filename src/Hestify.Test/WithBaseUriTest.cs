using System.Threading.Tasks;
using Hestify.Test.Helpers;
using RichardSzalay.MockHttp;
using Xunit;

namespace Hestify.Test
{
    public class WithBaseUriTest : HestifyTestBase
    {
        [Fact]
        public async Task WithBaseUri()
        {
            Client
                .WithBaseUri("http://test.com/api/auth")
                .Match(MockHttp.When("http://test.com/api/auth"));
        }
    }
}