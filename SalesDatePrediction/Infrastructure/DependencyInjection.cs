using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalesDatePrediction.Domain.Employees.Ports;
using SalesDatePrediction.Domain.Orders.Ports;
using SalesDatePrediction.Domain.Predictions.Ports;
using SalesDatePrediction.Domain.Products.Ports;
using SalesDatePrediction.Domain.Shippers.Ports;
using SalesDatePrediction.Infrastructure.Sql;

namespace SalesDatePrediction.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        ArgumentNullException.ThrowIfNull(cfg);
        services.AddScoped<ISalesPredictionReadPort, SqlSalesPredictionReadAdapter>();
        services.AddScoped<IEmployeeReadPort, SqlEmployeeReadAdapter>();
        services.AddScoped<IShipperReadPort, SqlShipperReadAdapter>();
        services.AddScoped<IProductReadPort, SqlProductReadAdapter>();
        services.AddScoped<IOrderWritePort, SqlOrderWriteAdapter>();
        services.AddScoped<IOrderReadPort, SqlOrderReadAdapter>();



        return services;
    }
}
