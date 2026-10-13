using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Web.Features.OrderDetails;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.ControlPoints;

public class KT4Ownership
{
    [Fact]
    [Trait("KT", "4")]
    public async Task KT4_MissingOrderReturnsNoDetails()
    {
        var options = new DbContextOptionsBuilder<CatalogContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new CatalogContext(options);
        var handler = new GetOrderDetailsHandler(new EfRepository<Order>(db));
        Assert.Null(await handler.Handle(new GetOrderDetails("owner", 999), CancellationToken.None));
    }

    [Fact]
    [Trait("KT", "4")]
    public async Task KT4_AnotherBuyerMustNotReadOrderDetails()
    {
        var options = new DbContextOptionsBuilder<CatalogContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new CatalogContext(options);
        var repo = new EfRepository<Order>(db);
        var order = new Order("owner", new Address("Street", "City", "State", "Country", "12345"),
            new List<OrderItem> { new OrderItem(new CatalogItemOrdered(1, "Book", "pic"), 10m, 2) });
        await repo.AddAsync(order);
        var handler = new GetOrderDetailsHandler(repo);
        var result = await handler.Handle(new GetOrderDetails("another-buyer", order.Id), CancellationToken.None);
        Assert.Null(result);
    }

    [Fact]
    [Trait("KT", "4")]
    public async Task KT4_OwnerCanReadOrderDetailsAndTotal()
    {
        var options = new DbContextOptionsBuilder<CatalogContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new CatalogContext(options);
        var repo = new EfRepository<Order>(db);
        var order = new Order("owner", new Address("Street", "City", "State", "Country", "12345"),
            new List<OrderItem> { new OrderItem(new CatalogItemOrdered(1, "Book", "pic"), 10m, 2) });
        await repo.AddAsync(order);
        var result = await new GetOrderDetailsHandler(repo).Handle(new GetOrderDetails("owner", order.Id), CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal(20m, result.Total);
    }
}
