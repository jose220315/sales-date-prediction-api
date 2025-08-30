# Configuración para Mutation Testing y mejores prácticas

## Packages recomendados para agregar al proyecto de tests:

```xml
<PackageReference Include="Stryker.Core" Version="4.0.4" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Bogus" Version="35.4.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="Testcontainers" Version="3.6.0" />
```

## Comandos útiles:

### Para ejecutar Mutation Testing:
```bash
dotnet stryker --project "Tests/SalesDatePrediction.Application.Tests"
```

### Para generar reporte de cobertura:
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

### Para ejecutar tests por categoría:
```bash
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

## Estructura recomendada adicional:

```
Tests/
??? SalesDatePrediction.Application.Tests/     (Unit Tests - ya existe)
??? SalesDatePrediction.Integration.Tests/     (Integration Tests)
??? SalesDatePrediction.Performance.Tests/     (Performance Tests)
??? SalesDatePrediction.Contract.Tests/        (Contract Tests)
```