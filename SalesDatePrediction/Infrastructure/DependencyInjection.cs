using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalesDatePrediction.Domain.Predictions.Ports;
using SalesDatePrediction.Infrastructure.Sql;

namespace SalesDatePrediction.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddScoped<ISalesPredictionReadPort, SqlSalesPredictionReadAdapter>();
        return services;
    }
}
