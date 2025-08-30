# Sales Date Prediction API

API REST desarrollada en .NET 9 que predice las próximas fechas de pedidos de los clientes basándose en su historial de compras.

## 🔧 Información para Build y Ejecución del Proyecto

### Requisitos Previos
- .NET 9 SDK
- SQL Server Express (o LocalDB)

### Configuración de Base de Datos
Actualizar la cadena de conexión en `API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "StoreSample": "Server=localhost\\SQLEXPRESS01;Database=StoreSample;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### Comandos para Build y Ejecución

```bash
# Restaurar todas las dependencias
dotnet restore

# Compilar toda la solución
dotnet build

# Ejecutar la API
cd API
dotnet run
```

### URLs de Acceso
- API: `https://localhost:5083`
- Documentación Swagger: `https://localhost:5083/swagger`

## 🧪 Información Relevante sobre las Pruebas

### Ejecutar Pruebas Unitarias
```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar pruebas con detalle
dotnet test --verbosity normal

# Ejecutar pruebas de un proyecto específico
dotnet test Tests/SalesDatePrediction.Application.Tests/
```

### Cobertura de Pruebas
El proyecto incluye pruebas unitarias para:
- **Handlers de Predicciones**: Validación del algoritmo de predicción
- **Handlers de Órdenes**: Creación y consulta de órdenes
- **Handlers de Empleados**: Gestión de empleados
- **Handlers de Productos**: Gestión de productos
- **Casos de uso con paginación**: Verificación de parámetros de paginación

### Pruebas Manuales de la API
1. Ejecutar la aplicación con `dotnet run` desde la carpeta API
2. Abrir Swagger UI en `https://localhost:5083/swagger`
3. Probar los endpoints principales:
   - `GET /api/predictions` - Obtener predicciones
   - `GET /api/customers/{id}/orders` - Órdenes por cliente
   - `GET /api/orders` - Lista de órdenes
   - `GET /api/products` - Lista de productos
   - `GET /api/employees` - Lista de empleados

## 📝 Breve Explicación sobre Cómo se Ejecutó la Prueba

### Algoritmo de Predicción
El sistema implementa un algoritmo estadístico que:

1. **Análisis Histórico**: Examina todas las órdenes previas de cada cliente
2. **Cálculo de Intervalos**: Determina los días transcurridos entre órdenes consecutivas
3. **Promedio Personalizado**: Calcula el promedio de días entre órdenes para cada cliente
4. **Fallback Global**: Si un cliente no tiene suficiente historial, usa el promedio global de todos los clientes
5. **Predicción Final**: Suma el promedio calculado a la fecha del último pedido

### Implementación Técnica
- **Query SQL Complejo**: Utiliza CTEs (Common Table Expressions) y Window Functions
- **Funciones de Ventana**: `LEAD()` para obtener la siguiente fecha de pedido
- **Agregaciones**: Cálculo de promedios por cliente y globales
- **Manejo de Casos Edge**: Clientes nuevos o con pocos pedidos históricos

### Validación de Resultados
Las pruebas unitarias verifican:
- Mapeo correcto de datos desde la base de datos
- Funcionamiento de la paginación
- Manejo de casos sin datos
- Propagación correcta de parámetros y tokens de cancelación
