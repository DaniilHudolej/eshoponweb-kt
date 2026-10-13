using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Infrastructure.Data;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.ControlPoints;
public class KT7Hybrid
{
    [Fact, Trait("KT", "7")]
    public async Task KT7_BasketTransferRealServiceDatabaseStubLogger()
    {
        var options = new DbContextOptionsBuilder<CatalogContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new CatalogContext(options);
        var repo = new EfRepository<Basket>(db);
        var service = new BasketService(repo, Substitute.For<IAppLogger<BasketService>>());
        await service.AddItemToBasket("anonymous", 1, 10m, 2);
        await service.AddItemToBasket("buyer", 1, 10m, 1);
        await service.TransferBasketAsync("anonymous", "buyer");
        var target = await repo.FirstOrDefaultAsync(new BasketWithItemsSpecification("buyer"));
        Assert.Equal(3, Assert.Single(target.Items).Quantity);
        Assert.Null(await repo.FirstOrDefaultAsync(new BasketWithItemsSpecification("anonymous")));
    }
    [Fact, Trait("KT", "7")]
    public async Task KT7_MissingBasketReturnsNotFound()
    {
        var options = new DbContextOptionsBuilder<CatalogContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new CatalogContext(options);
        var service = new BasketService(new EfRepository<Basket>(db), Substitute.For<IAppLogger<BasketService>>());
        var result = await service.SetQuantities(999, new System.Collections.Generic.Dictionary<string,int>());
        Assert.Equal(Ardalis.Result.ResultStatus.NotFound, result.Status);
    }
    [Fact, Trait("KT", "7")]
    public async Task KT7_DifferentBuyersHaveIsolatedBaskets()
    {
        var options = new DbContextOptionsBuilder<CatalogContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new CatalogContext(options);
        var service = new BasketService(new EfRepository<Basket>(db), null);
        var one = await service.AddItemToBasket("buyer-one", 1, 10m, 2);
        var two = await service.AddItemToBasket("buyer-two", 2, 5m, 3);
        Assert.NotEqual(one.Id, two.Id);
        Assert.Equal(1, Assert.Single(one.Items).CatalogItemId);
        Assert.Equal(2, Assert.Single(two.Items).CatalogItemId);
    }
}
