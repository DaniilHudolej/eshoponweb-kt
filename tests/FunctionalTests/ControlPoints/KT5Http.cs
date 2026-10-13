using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.eShopWeb.FunctionalTests.Web;
using Xunit;
namespace Microsoft.eShopWeb.FunctionalTests.ControlPoints;
[Collection("Sequential")]
public class KT5Http : IClassFixture<TestApplication>
{
    private readonly TestApplication _factory;
    public KT5Http(TestApplication factory) { _factory = factory; }
    [Fact, Trait("KT", "5"), Trait("Stage", "4")]
    public async Task KT5_Stage4_IntegratedWebCatalogAndBasket()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var html = await client.GetStringAsync("/");
        var response = await client.PostAsync("/basket/index", new FormUrlEncodedContent(new Dictionary<string,string> { ["id"] = "2", ["__RequestVerificationToken"] = WebPageHelpers.GetRequestVerificationToken(html) }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(".NET Black &amp; White Mug", await response.Content.ReadAsStringAsync());
    }
}
