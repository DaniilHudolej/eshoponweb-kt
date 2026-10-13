using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.eShopWeb.FunctionalTests.Web;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.ControlPoints;
[Collection("Sequential")]
public class KT4BigBang : IClassFixture<TestApplication>
{
    private readonly TestApplication _factory;
    public KT4BigBang(TestApplication factory) { _factory = factory; }

    [Fact, Trait("KT", "4")]
    public async Task KT4_AllModulesCatalogBasketAuthentication()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        var catalog = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, catalog.StatusCode);
        var html = await catalog.Content.ReadAsStringAsync();
        Assert.Contains(".NET Black &amp; White Mug", html);
        var token = WebPageHelpers.GetRequestVerificationToken(html);
        var added = await client.PostAsync("/basket/index", new FormUrlEncodedContent(new Dictionary<string,string> { ["id"] = "2", ["__RequestVerificationToken"] = token }));
        Assert.Equal(HttpStatusCode.Redirect, added.StatusCode);
        var basket = await client.GetAsync("/basket");
        Assert.Equal(HttpStatusCode.OK, basket.StatusCode);
        Assert.Contains(".NET Black &amp; White Mug", await basket.Content.ReadAsStringAsync());
        var checkout = await client.GetAsync("/basket/checkout");
        Assert.Equal(HttpStatusCode.Redirect, checkout.StatusCode);
        Assert.Contains("Login", checkout.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
