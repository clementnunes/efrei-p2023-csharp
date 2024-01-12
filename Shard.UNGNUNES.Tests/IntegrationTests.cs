using Microsoft.AspNetCore.Mvc.Testing;
using Shard.Shared.Web.IntegrationTests;
using Xunit.Abstractions;

namespace Shard.UNGNUNES.Tests
{
    public class IntegrationTests : BaseIntegrationTests<Startup, WebApplicationFactory<Startup>>
    {
        public IntegrationTests(WebApplicationFactory<Startup> factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
        {
            
        }
    }
    
}