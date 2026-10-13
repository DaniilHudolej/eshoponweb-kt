using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Infrastructure.Data;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.ControlPoints;
public class KT6BottomUp
{
    [Fact, Trait("KT", "6"), Trait("Stage", "1")]
    public void KT6_Stage1_DomainBasketMergesQuantities()
    {
        var basket = new Basket("buyer");
        basket.AddItem(1, 10m, 2);
        basket.AddItem(1, 10m, 3);
        Assert.Equal(5, Assert.Single(basket.Items).Quantity);
    }
    [Fact, Trait("KT", "6"), Trait("Stage", "2")]
    public async Task KT6_Stage2_RepositoryDatabaseReadAfterWrite()
    {
        var options = new DbContextOptionsBuilder<CatalogContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        int id;
        using (var db = new CatalogContext(options))
        {
            var repo = new EfRepository<Basket>(db);
            var basket = new Basket("buyer"); basket.AddItem(1, 10m, 2);
            await repo.AddAsync(basket); id = basket.Id;
        }
        using var fresh = new CatalogContext(options);
        var loaded = await new EfRepository<Basket>(fresh).FirstOrDefaultAsync(new BasketWithItemsSpecification(id));
        Assert.NotNull(loaded);
        Assert.Equal(2, Assert.Single(loaded.Items).Quantity);
    }
    [Fact, Trait("KT", "6"), Trait("Stage", "3")]
    public async Task KT6_Stage3_ServiceRepositoryDatabaseRemoveZero()
    {
        var options = new DbContextOptionsBuilder<CatalogContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new CatalogContext(options);
        var repo = new EfRepository<Basket>(db);
        var service = new BasketService(repo, null);
        var basket = await service.AddItemToBasket("buyer", 1, 10m, 2);
        var itemId = Assert.Single(basket.Items).Id;
        var result = await service.SetQuantities(basket.Id, new Dictionary<string,int> { [itemId.ToString()] = 0 });
        Assert.Empty(result.Value.Items);
        var loaded = await repo.FirstOrDefaultAsync(new BasketWithItemsSpecification(basket.Id));
        Assert.Empty(loaded.Items);
    }
}
