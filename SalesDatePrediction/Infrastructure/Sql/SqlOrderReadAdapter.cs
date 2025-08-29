using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SalesDatePrediction.Domain.Orders;
using SalesDatePrediction.Domain.Orders.Ports;
using System.Linq;

namespace SalesDatePrediction.Infrastructure.Sql;

public sealed class SqlOrderReadAdapter(IConfiguration cfg) : IOrderReadPort
{
    private readonly string _cs = cfg.GetConnectionString("StoreSample")
           ?? throw new InvalidOperationException("Missing connection string 'StoreSample'.");

    public async Task<OrderRead?> GetByIdAsync(int orderId, CancellationToken ct = default)
    {
        const string sql = @"
SELECT o.orderid   AS OrderId,
       o.custid    AS CustId,
       o.empid     AS EmpId,
       o.shipperid AS ShipperId,
       o.orderdate AS OrderDate,
       o.requireddate AS RequiredDate,
       o.shippeddate  AS ShippedDate,
       o.freight   AS Freight,
       o.shipname  AS ShipName,
       o.shipaddress AS ShipAddress,
       o.shipcity  AS ShipCity,
       o.shipcountry AS ShipCountry
FROM Sales.Orders o
WHERE o.orderid = @OrderId;

SELECT od.productid    AS ProductId,
       p.productname   AS ProductName,
       od.unitprice    AS UnitPrice,
       od.qty          AS Qty,
       od.discount     AS Discount
FROM Sales.OrderDetails od
JOIN Production.Products p ON p.productid = od.productid
WHERE od.orderid = @OrderId
ORDER BY p.productname;";

        await using var cn = new SqlConnection(_cs);
        using var grid = await cn.QueryMultipleAsync(sql, new { OrderId = orderId });

        var order = await grid.ReadFirstOrDefaultAsync<OrderHeaderRow>();
        if (order is null) return null;

        var details = (await grid.ReadAsync<OrderDetailRow>()).ToList();

        return new OrderRead(
            order.OrderId, order.CustId, order.EmpId, order.ShipperId,
            order.OrderDate, order.RequiredDate, order.ShippedDate,
            order.Freight, order.ShipName, order.ShipAddress, order.ShipCity, order.ShipCountry,
            details.Select(d => new OrderDetailRead(d.ProductId, d.ProductName, d.UnitPrice, d.Qty, d.Discount)).ToList()
        );
    }

    private sealed record OrderHeaderRow(
        int OrderId, int? CustId, int EmpId, int ShipperId,
        DateTime OrderDate, DateTime RequiredDate, DateTime? ShippedDate,
        decimal Freight, string ShipName, string ShipAddress, string ShipCity, string ShipCountry);

    private sealed record OrderDetailRow(int ProductId, string ProductName, decimal UnitPrice, short Qty, decimal Discount);
}
