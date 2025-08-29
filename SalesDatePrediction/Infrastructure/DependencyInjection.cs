using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalesDatePrediction.Domain.Employees.Ports;
using SalesDatePrediction.Domain.Predictions.Ports;
using SalesDatePrediction.Domain.Shippers.Ports;
using SalesDatePrediction.Infrastructure.Sql;

namespace SalesDatePrediction.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddScoped<ISalesPredictionReadPort, SqlSalesPredictionReadAdapter>();
        services.AddScoped<IEmployeeReadPort, SqlEmployeeReadAdapter>();
        services.AddScoped<IShipperReadPort, SqlShipperReadAdapter>();


        return services;
    }
}
