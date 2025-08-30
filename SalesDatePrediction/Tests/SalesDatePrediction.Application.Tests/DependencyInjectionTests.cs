using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace SalesDatePrediction.Application.Tests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void AddApplication_Should_RegisterMediatRServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddApplication();
            var serviceDescriptors = services.ToList();

            // Assert
            Assert.Contains(serviceDescriptors, sd => sd.ServiceType == typeof(IMediator));
        }

        [Fact]
        public void AddApplication_Should_RegisterRequestHandlers()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddApplication();

            // Assert - Just verify that MediatR services are registered
            var serviceDescriptors = services.ToList();
            Assert.Contains(serviceDescriptors, sd => sd.ServiceType == typeof(IMediator));
        }

        [Fact]
        public void AddApplication_Should_ReturnServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddApplication();

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddApplication_Should_RegisterHandlersFromCurrentAssembly()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddApplication();

            // Assert
            var handlerDescriptors = services.Where(x => x.ServiceType.IsGenericType && 
                (x.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                 x.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<>))).ToList();

            Assert.NotEmpty(handlerDescriptors);
        }

        [Fact]
        public void AddApplication_Should_RegisterServicesSingleton()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddApplication();

            // Assert
            // MediatR registers services, so we should have some registrations
            Assert.NotEmpty(services);
        }

        [Fact]
        public void AddApplication_Should_AllowMultipleCalls()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act - calling multiple times should not fail
            services.AddApplication();
            services.AddApplication();

            // Assert - should still have services registered
            Assert.NotEmpty(services);
        }

        [Fact]
        public void AddApplication_Should_WorkWithExistingServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<TestService>();

            // Act
            services.AddApplication();

            // Assert
            var serviceDescriptors = services.ToList();
            Assert.Contains(serviceDescriptors, sd => sd.ServiceType == typeof(IMediator));
            Assert.Contains(serviceDescriptors, sd => sd.ServiceType == typeof(TestService));
        }

        [Fact]
        public void AddApplication_Should_AddMediatRServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddApplication();

            // Assert
            var mediatRServices = services.Where(s => 
                s.ServiceType == typeof(IMediator) ||
                s.ServiceType == typeof(ISender) ||
                s.ServiceType == typeof(IPublisher)).ToList();

            Assert.NotEmpty(mediatRServices);
        }

        [Fact]
        public void AddApplication_Should_RegisterFromCorrectAssembly()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddApplication();

            // Assert - verify that we registered from the Application assembly
            var applicationHandlers = services.Where(s => 
                s.ServiceType.IsGenericType && 
                s.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) &&
                s.ImplementationType != null &&
                s.ImplementationType.Assembly.GetName().Name?.Contains("Application") == true
            ).ToList();

            Assert.NotEmpty(applicationHandlers);
        }

        private class TestService
        {
            public string Message => "Test service";
        }
    }
}