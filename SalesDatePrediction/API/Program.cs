using MediatR;
using SalesDatePrediction.Application;
using SalesDatePrediction.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SalesDatePrediction API", Version = "v1" });
    c.EnableAnnotations();
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options => {
    options.AddPolicy("AngularApp", policy => {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configuración para desarrollo - solo HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SalesDatePrediction API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AngularApp");

app.MapControllers();
app.Run();
