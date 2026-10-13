using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.eShopWeb.FunctionalTests.Web;
using Xunit;
namespace Microsoft.eShopWeb.FunctionalTests.ControlPoints;
[Collection("Sequential")]
public class KT6Http : IClassFixture<TestApplication>
{
    private readonly TestApplication _factory;
    public KT6Http(TestApplication factory) { _factory = factory; }
    [Fact, Trait("KT", "6"), Trait("Stage", "4")]
    public async Task KT6_Stage4_WebUiReadsCatalogDatabase()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var response = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(".NET Bot Black Sweatshirt", await response.Content.ReadAsStringAsync());
    }
}
