SELECT
    e.empid       AS EmpId,
    LTRIM(RTRIM(e.firstname)) + ' ' + LTRIM(RTRIM(e.lastname)) AS FullName
FROM HR.Employees AS e
ORDER BY FullName;
