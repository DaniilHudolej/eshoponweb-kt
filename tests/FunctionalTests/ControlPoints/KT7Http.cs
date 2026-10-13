using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.eShopWeb.FunctionalTests.Web;
using Xunit;
namespace Microsoft.eShopWeb.FunctionalTests.ControlPoints;
[Collection("Sequential")]
public class KT7Http : IClassFixture<TestApplication>
{
    private readonly TestApplication _factory;
    public KT7Http(TestApplication factory) { _factory = factory; }
    [Fact, Trait("KT", "7")]
    public async Task KT7_WebUiBusinessLogicDatabaseAddAndRemove()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var html = await client.GetStringAsync("/");
        var added = await client.PostAsync("/basket/index", new FormUrlEncodedContent(new Dictionary<string,string> { ["id"] = "2", ["__RequestVerificationToken"] = WebPageHelpers.GetRequestVerificationToken(html) }));
        Assert.Equal(HttpStatusCode.OK, added.StatusCode);
        var basket = await added.Content.ReadAsStringAsync();
        Assert.Contains(".NET Black &amp; White Mug", basket);
        var removed = await client.PostAsync("/basket/update", new FormUrlEncodedContent(new Dictionary<string,string> { ["Items[0].Id"] = WebPageHelpers.GetId(basket), ["Items[0].Quantity"] = "0", ["__RequestVerificationToken"] = WebPageHelpers.GetRequestVerificationToken(basket) }));
        Assert.Equal(HttpStatusCode.OK, removed.StatusCode);
        Assert.Contains("Basket is empty", await removed.Content.ReadAsStringAsync());
    }
    [Fact, Trait("KT", "7")]
    public async Task KT7_CsrfProtectionRejectsMissingToken()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var response = await client.PostAsync("/basket/index", new FormUrlEncodedContent(new Dictionary<string,string> { ["id"] = "2" }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
