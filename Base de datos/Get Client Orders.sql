DECLARE @CustomerId int = 1;

SELECT
    o.orderid      AS OrderId,
    o.requireddate AS RequiredDate,
    o.shippeddate  AS ShippedDate,
    o.shipname     AS ShipName,
    o.shipaddress  AS ShipAddress,
    o.shipcity     AS ShipCity
FROM Sales.Orders o
WHERE o.custid = @CustomerId
ORDER BY o.orderdate DESC;
