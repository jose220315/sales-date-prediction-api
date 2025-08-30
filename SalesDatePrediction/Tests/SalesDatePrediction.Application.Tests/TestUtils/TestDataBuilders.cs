using System.Collections.Generic;
using System.Linq;
using SalesDatePrediction.Domain.Shippers;
using SalesDatePrediction.Domain.Products;
using SalesDatePrediction.Domain.Employees;

namespace SalesDatePrediction.Application.Tests.TestUtils
{
    /// <summary>
    /// Builder para crear datos de prueba de Shippers de manera fluida
    /// </summary>
    public class ShipperTestDataBuilder
    {
        private int _shipperId = 1;
        private string _companyName = "Default Shipper Company";

        public ShipperTestDataBuilder WithId(int id)
        {
            _shipperId = id;
            return this;
        }

        public ShipperTestDataBuilder WithCompanyName(string companyName)
        {
            _companyName = companyName;
            return this;
        }

        public ShipperTestDataBuilder WithInternationalShipper()
        {
            _companyName = "DHL Express International";
            return this;
        }

        public ShipperTestDataBuilder WithComplexName()
        {
            _companyName = "Transportes Ñandú & Cía. S.A.S.";
            return this;
        }

        public Shipper Build() => new(_shipperId, _companyName);

        public List<Shipper> BuildMany(int count)
        {
            return Enumerable.Range(1, count)
                .Select(i => new ShipperTestDataBuilder()
                    .WithId(i)
                    .WithCompanyName($"Shipper Company {i}")
                    .Build())
                .ToList();
        }

        public static List<Shipper> CreateInternationalShippers()
        {
            return new List<Shipper>
            {
                new(1, "FedEx Corporation"),
                new(2, "DHL Express"),
                new(3, "UPS - United Parcel Service"),
                new(4, "TNT Express"),
                new(5, "Coordinadora Mercantil S.A."),
                new(6, "Servientrega S.A.")
            };
        }

        public static List<Shipper> CreateShippersWithComplexNames()
        {
            return new List<Shipper>
            {
                new(100, "Transportes Ñandú & Cía. S.A.S."),
                new(101, "O'Reilly's Global Shipping Ltd. (Europe)"),
                new(102, "Envíos Rápidos™ - Soluciones Logísticas®"),
                new(103, "Jean-Pierre's Express Delivery Service Inc."),
                new(104, "Express Delivery ?? - China Internacional")
            };
        }
    }

    /// <summary>
    /// Builder para crear datos de prueba de Products
    /// </summary>
    public class ProductTestDataBuilder
    {
        private int _productId = 1;
        private string _productName = "Default Product";

        public ProductTestDataBuilder WithId(int id)
        {
            _productId = id;
            return this;
        }

        public ProductTestDataBuilder WithName(string name)
        {
            _productName = name;
            return this;
        }

        public ProductTestDataBuilder WithTechProduct()
        {
            _productName = "Laptop HP EliteBook 840 G9";
            return this;
        }

        public Product Build() => new(_productId, _productName);

        public List<Product> BuildMany(int count)
        {
            return Enumerable.Range(1, count)
                .Select(i => new ProductTestDataBuilder()
                    .WithId(i)
                    .WithName($"Product {i}")
                    .Build())
                .ToList();
        }

        public static List<Product> CreateTechProducts()
        {
            return new List<Product>
            {
                new(1, "Laptop HP EliteBook 840"),
                new(2, "Mouse Inalámbrico Logitech MX Master 3"),
                new(3, "Teclado Mecánico Corsair K95 RGB"),
                new(4, "Monitor Dell UltraSharp 27\""),
                new(5, "iPhone 15 Pro Max"),
                new(6, "Samsung Galaxy S24 Ultra")
            };
        }
    }

    /// <summary>
    /// Builder para crear datos de prueba de Employees
    /// </summary>
    public class EmployeeTestDataBuilder
    {
        private int _empId = 1;
        private string _fullName = "Default Employee";

        public EmployeeTestDataBuilder WithId(int id)
        {
            _empId = id;
            return this;
        }

        public EmployeeTestDataBuilder WithName(string name)
        {
            _fullName = name;
            return this;
        }

        public Employee Build() => new(_empId, _fullName);

        public List<Employee> BuildMany(int count)
        {
            return Enumerable.Range(1, count)
                .Select(i => new EmployeeTestDataBuilder()
                    .WithId(i)
                    .WithName($"Employee {i}")
                    .Build())
                .ToList();
        }

        public static List<Employee> CreateManagementTeam()
        {
            return new List<Employee>
            {
                new(1, "Juan Pérez Rodríguez"),
                new(2, "María García López"),
                new(3, "Carlos Martínez Silva"),
                new(4, "Ana Fernández Torres"),
                new(5, "Luis Sánchez Moreno")
            };
        }
    }
}