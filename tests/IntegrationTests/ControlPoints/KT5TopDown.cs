using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Web.Services;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.ControlPoints;

public class KT5TopDown
{
    [Fact, Trait("KT", "5"), Trait("Stage", "2")]
    public async Task KT5_Stage2_ViewServiceWithStubRepositories()
    {
        var baskets = Substitute.For<IRepository<Basket>>();
        var items = Substitute.For<IRepository<CatalogItem>>();
        var uri = Substitute.For<IUriComposer>();
        var basket = new Basket("buyer");
        basket.AddItem(0, 12m, 2);
        items.ListAsync(Arg.Any<CatalogItemsSpecification>(), default).Returns(new List<CatalogItem> { new CatalogItem(1, 1, "Test", "Book", 12m, "book.png") });
        uri.ComposePicUri("book.png").Returns("/images/book.png");
        var service = new BasketViewModelService(baskets, items, uri, Substitute.For<IBasketQueryService>());
        var view = await service.Map(basket);
        Assert.Equal(24m, view.Total());
        Assert.Equal("Book", Assert.Single(view.Items).ProductName);
    }

    [Fact, Trait("KT", "5"), Trait("Stage", "3")]
    public async Task KT5_Stage3_ViewServiceRealRepositoriesDatabase()
    {
        var options = new DbContextOptionsBuilder<CatalogContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new CatalogContext(options);
        var items = new EfRepository<CatalogItem>(db);
        var baskets = new EfRepository<Basket>(db);
        var item = await items.AddAsync(new CatalogItem(1, 1, "Test", "Book", 12m, "book.png"));
        var basket = new Basket("buyer");
        basket.AddItem(item.Id, item.Price, 3);
        await baskets.AddAsync(basket);
        var service = new BasketViewModelService(baskets, items, Substitute.For<IUriComposer>(), Substitute.For<IBasketQueryService>());
        var view = await service.GetOrCreateBasketForUser("buyer");
        Assert.Equal(36m, view.Total());
        Assert.Equal(item.Id, Assert.Single(view.Items).CatalogItemId);
    }
}
