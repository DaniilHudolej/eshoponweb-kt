using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.Pages.Basket;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.ControlPoints;
public class KT5Controller
{
    [Fact, Trait("KT", "5"), Trait("Stage", "1")]
    public async Task KT5_Stage1_PageModelWithStubServices()
    {
        var viewService = Substitute.For<IBasketViewModelService>();
        viewService.GetOrCreateBasketForUser("buyer").Returns(new BasketViewModel { BuyerId = "buyer", Id = 42 });
        var page = new IndexModel(Substitute.For<IBasketService>(), viewService, Substitute.For<IRepository<CatalogItem>>());
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "buyer") }, "Test")) };
        page.PageContext = new PageContext { HttpContext = context };
        await page.OnGet();
        Assert.Equal(42, page.BasketModel.Id);
        Assert.Equal("buyer", page.BasketModel.BuyerId);
        await viewService.Received(1).GetOrCreateBasketForUser("buyer");
    }
}
