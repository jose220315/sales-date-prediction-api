using Xunit;

namespace SalesDatePrediction.Application.Tests.TestUtils
{
    public class TestDataBuildersTests
    {
        #region ShipperTestDataBuilder Tests

        [Fact]
        public void ShipperTestDataBuilder_Should_BuildWithDefaultValues()
        {
            // Act
            var shipper = new ShipperTestDataBuilder().Build();

            // Assert
            Assert.NotNull(shipper);
            Assert.Equal(1, shipper.ShipperId);
            Assert.Equal("Default Shipper Company", shipper.CompanyName);
        }

        [Fact]
        public void ShipperTestDataBuilder_WithId_Should_SetCustomId()
        {
            // Act
            var shipper = new ShipperTestDataBuilder()
                .WithId(99)
                .Build();

            // Assert
            Assert.Equal(99, shipper.ShipperId);
        }

        [Fact]
        public void ShipperTestDataBuilder_WithCompanyName_Should_SetCustomName()
        {
            // Act
            var shipper = new ShipperTestDataBuilder()
                .WithCompanyName("Custom Shipping Inc.")
                .Build();

            // Assert
            Assert.Equal("Custom Shipping Inc.", shipper.CompanyName);
        }

        [Fact]
        public void ShipperTestDataBuilder_WithInternationalShipper_Should_SetInternationalName()
        {
            // Act
            var shipper = new ShipperTestDataBuilder()
                .WithInternationalShipper()
                .Build();

            // Assert
            Assert.Equal("DHL Express International", shipper.CompanyName);
        }

        [Fact]
        public void ShipperTestDataBuilder_WithComplexName_Should_SetComplexName()
        {
            // Act
            var shipper = new ShipperTestDataBuilder()
                .WithComplexName()
                .Build();

            // Assert
            Assert.Equal("Transportes Ñandú & Cía. S.A.S.", shipper.CompanyName);
        }

        [Fact]
        public void ShipperTestDataBuilder_BuildMany_Should_CreateMultipleShippers()
        {
            // Act
            var shippers = new ShipperTestDataBuilder().BuildMany(5);

            // Assert
            Assert.NotNull(shippers);
            Assert.Equal(5, shippers.Count);
            
            for (int i = 0; i < 5; i++)
            {
                Assert.Equal(i + 1, shippers[i].ShipperId);
                Assert.Equal($"Shipper Company {i + 1}", shippers[i].CompanyName);
            }
        }

        [Fact]
        public void ShipperTestDataBuilder_CreateInternationalShippers_Should_CreatePredefinedList()
        {
            // Act
            var shippers = ShipperTestDataBuilder.CreateInternationalShippers();

            // Assert
            Assert.NotNull(shippers);
            Assert.Equal(6, shippers.Count);
            Assert.Contains(shippers, s => s.CompanyName == "FedEx Corporation");
            Assert.Contains(shippers, s => s.CompanyName == "DHL Express");
            Assert.Contains(shippers, s => s.CompanyName == "UPS - United Parcel Service");
            Assert.Contains(shippers, s => s.CompanyName == "TNT Express");
            Assert.Contains(shippers, s => s.CompanyName == "Coordinadora Mercantil S.A.");
            Assert.Contains(shippers, s => s.CompanyName == "Servientrega S.A.");
        }

        [Fact]
        public void ShipperTestDataBuilder_CreateShippersWithComplexNames_Should_CreateComplexNamesList()
        {
            // Act
            var shippers = ShipperTestDataBuilder.CreateShippersWithComplexNames();

            // Assert
            Assert.NotNull(shippers);
            Assert.Equal(5, shippers.Count);
            Assert.Contains(shippers, s => s.CompanyName.Contains("Ñandú"));
            Assert.Contains(shippers, s => s.CompanyName.Contains("O'Reilly's"));
            Assert.Contains(shippers, s => s.CompanyName.Contains("Jean-Pierre's"));
            Assert.Contains(shippers, s => s.CompanyName.Contains("??"));
        }

        #endregion

        #region ProductTestDataBuilder Tests

        [Fact]
        public void ProductTestDataBuilder_Should_BuildWithDefaultValues()
        {
            // Act
            var product = new ProductTestDataBuilder().Build();

            // Assert
            Assert.NotNull(product);
            Assert.Equal(1, product.ProductId);
            Assert.Equal("Default Product", product.ProductName);
        }

        [Fact]
        public void ProductTestDataBuilder_WithId_Should_SetCustomId()
        {
            // Act
            var product = new ProductTestDataBuilder()
                .WithId(42)
                .Build();

            // Assert
            Assert.Equal(42, product.ProductId);
        }

        [Fact]
        public void ProductTestDataBuilder_WithName_Should_SetCustomName()
        {
            // Act
            var product = new ProductTestDataBuilder()
                .WithName("Custom Product Name")
                .Build();

            // Assert
            Assert.Equal("Custom Product Name", product.ProductName);
        }

        [Fact]
        public void ProductTestDataBuilder_WithTechProduct_Should_SetTechProductName()
        {
            // Act
            var product = new ProductTestDataBuilder()
                .WithTechProduct()
                .Build();

            // Assert
            Assert.Equal("Laptop HP EliteBook 840 G9", product.ProductName);
        }

        [Fact]
        public void ProductTestDataBuilder_BuildMany_Should_CreateMultipleProducts()
        {
            // Act
            var products = new ProductTestDataBuilder().BuildMany(3);

            // Assert
            Assert.NotNull(products);
            Assert.Equal(3, products.Count);
            
            for (int i = 0; i < 3; i++)
            {
                Assert.Equal(i + 1, products[i].ProductId);
                Assert.Equal($"Product {i + 1}", products[i].ProductName);
            }
        }

        [Fact]
        public void ProductTestDataBuilder_CreateTechProducts_Should_CreatePredefinedList()
        {
            // Act
            var products = ProductTestDataBuilder.CreateTechProducts();

            // Assert
            Assert.NotNull(products);
            Assert.Equal(6, products.Count);
            Assert.Contains(products, p => p.ProductName.Contains("Laptop HP"));
            Assert.Contains(products, p => p.ProductName.Contains("Mouse"));
            Assert.Contains(products, p => p.ProductName.Contains("Teclado"));
            Assert.Contains(products, p => p.ProductName.Contains("Monitor"));
            Assert.Contains(products, p => p.ProductName.Contains("iPhone"));
            Assert.Contains(products, p => p.ProductName.Contains("Samsung"));
        }

        #endregion

        #region EmployeeTestDataBuilder Tests

        [Fact]
        public void EmployeeTestDataBuilder_Should_BuildWithDefaultValues()
        {
            // Act
            var employee = new EmployeeTestDataBuilder().Build();

            // Assert
            Assert.NotNull(employee);
            Assert.Equal(1, employee.EmpId);
            Assert.Equal("Default Employee", employee.FullName);
        }

        [Fact]
        public void EmployeeTestDataBuilder_WithId_Should_SetCustomId()
        {
            // Act
            var employee = new EmployeeTestDataBuilder()
                .WithId(25)
                .Build();

            // Assert
            Assert.Equal(25, employee.EmpId);
        }

        [Fact]
        public void EmployeeTestDataBuilder_WithName_Should_SetCustomName()
        {
            // Act
            var employee = new EmployeeTestDataBuilder()
                .WithName("John Smith")
                .Build();

            // Assert
            Assert.Equal("John Smith", employee.FullName);
        }

        [Fact]
        public void EmployeeTestDataBuilder_BuildMany_Should_CreateMultipleEmployees()
        {
            // Act
            var employees = new EmployeeTestDataBuilder().BuildMany(4);

            // Assert
            Assert.NotNull(employees);
            Assert.Equal(4, employees.Count);
            
            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(i + 1, employees[i].EmpId);
                Assert.Equal($"Employee {i + 1}", employees[i].FullName);
            }
        }

        [Fact]
        public void EmployeeTestDataBuilder_CreateManagementTeam_Should_CreatePredefinedList()
        {
            // Act
            var employees = EmployeeTestDataBuilder.CreateManagementTeam();

            // Assert
            Assert.NotNull(employees);
            Assert.Equal(5, employees.Count);
            Assert.Contains(employees, e => e.FullName.Contains("Juan Pérez"));
            Assert.Contains(employees, e => e.FullName.Contains("María García"));
            Assert.Contains(employees, e => e.FullName.Contains("Carlos Martínez"));
            Assert.Contains(employees, e => e.FullName.Contains("Ana Fernández"));
            Assert.Contains(employees, e => e.FullName.Contains("Luis Sánchez"));
        }

        #endregion

        #region Fluent Interface Tests

        [Fact]
        public void ShipperTestDataBuilder_FluentInterface_Should_ChainMethodCalls()
        {
            // Act
            var shipper = new ShipperTestDataBuilder()
                .WithId(100)
                .WithCompanyName("Fluent Shipping")
                .Build();

            // Assert
            Assert.Equal(100, shipper.ShipperId);
            Assert.Equal("Fluent Shipping", shipper.CompanyName);
        }

        [Fact]
        public void ProductTestDataBuilder_FluentInterface_Should_ChainMethodCalls()
        {
            // Act
            var product = new ProductTestDataBuilder()
                .WithId(200)
                .WithName("Chained Product")
                .Build();

            // Assert
            Assert.Equal(200, product.ProductId);
            Assert.Equal("Chained Product", product.ProductName);
        }

        [Fact]
        public void EmployeeTestDataBuilder_FluentInterface_Should_ChainMethodCalls()
        {
            // Act
            var employee = new EmployeeTestDataBuilder()
                .WithId(300)
                .WithName("Fluent Employee")
                .Build();

            // Assert
            Assert.Equal(300, employee.EmpId);
            Assert.Equal("Fluent Employee", employee.FullName);
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public void ShipperTestDataBuilder_BuildMany_Should_HandleZeroCount()
        {
            // Act
            var shippers = new ShipperTestDataBuilder().BuildMany(0);

            // Assert
            Assert.NotNull(shippers);
            Assert.Empty(shippers);
        }

        [Fact]
        public void ProductTestDataBuilder_BuildMany_Should_HandleZeroCount()
        {
            // Act
            var products = new ProductTestDataBuilder().BuildMany(0);

            // Assert
            Assert.NotNull(products);
            Assert.Empty(products);
        }

        [Fact]
        public void EmployeeTestDataBuilder_BuildMany_Should_HandleZeroCount()
        {
            // Act
            var employees = new EmployeeTestDataBuilder().BuildMany(0);

            // Assert
            Assert.NotNull(employees);
            Assert.Empty(employees);
        }

        [Fact]
        public void ShipperTestDataBuilder_BuildMany_Should_HandleLargeCount()
        {
            // Act
            var shippers = new ShipperTestDataBuilder().BuildMany(100);

            // Assert
            Assert.NotNull(shippers);
            Assert.Equal(100, shippers.Count);
            Assert.All(shippers, s => Assert.True(s.ShipperId >= 1 && s.ShipperId <= 100));
        }

        [Fact]
        public void ProductTestDataBuilder_BuildMany_Should_HandleLargeCount()
        {
            // Act
            var products = new ProductTestDataBuilder().BuildMany(50);

            // Assert
            Assert.NotNull(products);
            Assert.Equal(50, products.Count);
            Assert.All(products, p => Assert.True(p.ProductId >= 1 && p.ProductId <= 50));
        }

        [Fact]
        public void EmployeeTestDataBuilder_BuildMany_Should_HandleLargeCount()
        {
            // Act
            var employees = new EmployeeTestDataBuilder().BuildMany(75);

            // Assert
            Assert.NotNull(employees);
            Assert.Equal(75, employees.Count);
            Assert.All(employees, e => Assert.True(e.EmpId >= 1 && e.EmpId <= 75));
        }

        #endregion
    }
}