;WITH OrdersNorm AS (       
    SELECT o.custid,
           CAST(CAST(o.orderdate AS date) AS datetime) AS orderdate
    FROM Sales.Orders AS o
),
O AS (  
    SELECT
        custid,
        orderdate,
        LEAD(orderdate) OVER (PARTITION BY custid ORDER BY orderdate) AS next_orderdate
    FROM OrdersNorm
),
Diffs AS (   
    SELECT
        custid,
        DATEDIFF(DAY, orderdate, next_orderdate) AS DaysBetween
    FROM O
    WHERE next_orderdate IS NOT NULL
),
AvgPerCustomer AS (   
    SELECT
        custid,
        CAST(ROUND(AVG(CAST(DaysBetween AS float)), 0) AS int) AS AvgDaysBetween
    FROM Diffs
    GROUP BY custid
),
LastOrder AS (            
    SELECT
        custid,
        MAX(orderdate) AS LastOrderDate
    FROM OrdersNorm
    GROUP BY custid
),
GlobalAvg AS (   
    SELECT CAST(ROUND(AVG(CAST(DaysBetween AS float)), 0) AS int) AS AvgDaysBetween
    FROM Diffs
)
SELECT
    c.companyname            AS CustomerName,
    lo.LastOrderDate         AS LastOrderDate,
    DATEADD(DAY, COALESCE(apc.AvgDaysBetween, ga.AvgDaysBetween), lo.LastOrderDate)
                             AS NextPredictedOrder
FROM Sales.Customers AS c
JOIN LastOrder      AS lo  ON lo.custid = c.custid
LEFT JOIN AvgPerCustomer apc ON apc.custid = c.custid
CROSS JOIN GlobalAvg AS ga

ORDER BY c.companyname;
