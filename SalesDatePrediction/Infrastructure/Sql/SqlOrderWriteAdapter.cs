using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SalesDatePrediction.Domain.Orders;
using SalesDatePrediction.Domain.Orders.Ports;

namespace SalesDatePrediction.Infrastructure.Sql;

public sealed class SqlOrderWriteAdapter(IConfiguration cfg) : IOrderWritePort
{
    private readonly string _cs = cfg.GetConnectionString("StoreSample")
           ?? throw new InvalidOperationException("Missing connection string 'StoreSample'.");

    public async Task<int> AddAsync(CreateOrder order, CreateOrderDetail detail, CancellationToken ct = default)
    {
        await using var cn = new SqlConnection(_cs);
        await cn.OpenAsync(ct);
        await using var tx = await cn.BeginTransactionAsync(ct) as SqlTransaction;

        try
        {
            decimal unitPrice = detail.UnitPrice ?? await cn.ExecuteScalarAsync<decimal>(
                new CommandDefinition(
                    "SELECT unitprice FROM Production.Products WHERE productid=@ProductId",
                    new { detail.ProductId }, tx, cancellationToken: ct));

            // Insert Orders
            const string insertOrder = @"
INSERT INTO Sales.Orders
(custid, empid, orderdate, requireddate, shippeddate, shipperid, freight, shipname, shipaddress, shipcity, shipcountry)
VALUES
(@CustId, @EmpId, @OrderDate, @RequiredDate, @ShippedDate, @ShipperId, @Freight, @ShipName, @ShipAddress, @ShipCity, @ShipCountry);
SELECT CAST(SCOPE_IDENTITY() AS int);";

            int orderId = await cn.ExecuteScalarAsync<int>(
                new CommandDefinition(insertOrder, new
                {
                    order.CustId,
                    order.EmpId,
                    order.OrderDate,
                    order.RequiredDate,
                    order.ShippedDate,
                    order.ShipperId,
                    order.Freight,
                    order.ShipName,
                    order.ShipAddress,
                    order.ShipCity,
                    order.ShipCountry
                }, tx, cancellationToken: ct));

            // Insert OrderDetails
            const string insertDetail = @"
INSERT INTO Sales.OrderDetails (orderid, productid, unitprice, qty, discount)
VALUES (@OrderId, @ProductId, @UnitPrice, @Qty, @Discount);";

            await cn.ExecuteAsync(
                new CommandDefinition(insertDetail, new
                {
                    OrderId = orderId,
                    detail.ProductId,
                    UnitPrice = unitPrice,
                    detail.Qty,
                    detail.Discount
                }, tx, cancellationToken: ct));

            await tx.CommitAsync(ct);
            return orderId;
        }
        catch
        {
            if (tx.Connection is not null) await tx.RollbackAsync(ct);
            throw;
        }
    }
}
