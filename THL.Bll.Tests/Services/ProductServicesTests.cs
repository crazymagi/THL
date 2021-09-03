using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using THL.Bll.Services;
using THL.DataAccess;
using THL.Domain;

namespace THL.Bll.Tests.Services
{
    public class ProductServicesTests
    {
        private Mock<ILogger<ProductService>> _loggerMock;

        private readonly IFixture _fixture;
        private readonly IList<Product> _seedData;
        private readonly DbContextOptions<ThlDbContext> _options;

        public ProductServicesTests()
        {
            _fixture = new Fixture();
            
            _seedData = Seed();
            
            _options = new DbContextOptionsBuilder<ThlDbContext>()
                .UseInMemoryDatabase(databaseName: "MyDataBase")
                .Options;

            using var context = new ThlDbContext(_options);
            context.Products.AddRange(_seedData);
            context.SaveChanges();
        }

        private IList<Product> Seed()
        {
            return (new[] {"red", "yellow", "blue", "white", "green"})
                .SelectMany(x => 
                    _fixture
                        .Build<Product>()
                        .With(p => p.Name, $"{x} {_fixture.Create<string>()}")
                        .CreateMany(20)
                )
                .OrderBy(x => x.Name)
                .ToArray();
        }

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<ProductService>>();

        }

        [TearDown]
        public void TearDown()
        {
            _loggerMock.VerifyAll();
        }

        [TestCase(-1, 10)]
        [TestCase(1, 0)]
        [TestCase(1, -1)]
        public void GetProducts_Throws_WhenInvalidParams(int page, int pageSize)
        {
            // Arrange
            using var context = new ThlDbContext(_options);
            var subject = new ProductService(_loggerMock.Object, context);
            
            // Act
            Func<Task> action = async () => await subject.GetProducts("abc", page, pageSize);
            
            // Assert
            action.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
        }

        [TestCase("red", 0, 10)]
        [TestCase("red", 0, 5)]
        [TestCase("Yellow", 0, 5)]
        [TestCase("yellow", 1, 5)]
        [TestCase("green", 0, 25)]
        [TestCase("Green", 0, 15)]
        [TestCase("green", 1, 15)]
        public async Task GetProducts_ReturnExpected_WithSearchTerm(string searchTerm, int page, int pageSize)
        {
            // Arrange
            await using var context = new ThlDbContext(_options);
            var subject = new ProductService(_loggerMock.Object, context);

            var expected = _seedData
                .Where(x => x.Name.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase))
                .Skip(page * pageSize)
                .Take(pageSize);
            
            // Act
            var result = await subject.GetProducts(searchTerm, page, pageSize);
            
            // Assert
            result.Should().BeEquivalentTo(expected);
        }
        
        [TestCase("", 0, 5)]
        [TestCase(null, 1, 5)]
        [TestCase("  ", 1, 5)]
        public async Task GetProducts_ReturnExpected_WithoutSearchTerm(string emptyTerm, int page, int pageSize)
        {
            // Arrange// Arrange
            await using var context = new ThlDbContext(_options);
            var subject = new ProductService(_loggerMock.Object, context);

            var expected = _seedData
                .Skip(page * pageSize)
                .Take(pageSize);
            
            // Act
            var result = await subject.GetProducts(emptyTerm, page, pageSize);
            
            // Assert
            result.Should().BeEquivalentTo(expected);
        }
        
        [Test]
        public void GetProductById_Throws_WhenIdIsEmpty()
        {
            // Arrange// Arrange
            using var context = new ThlDbContext(_options);
            var subject = new ProductService(_loggerMock.Object, context);

            // Act
            Func<Task> action = async() => await subject.GetProductById(Guid.Empty);
            
            // Assert
            action.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
        }
        
        [Test]
        public async Task GetProductById_ReturnExpected()
        {
            // Arrange// Arrange
            await using var context = new ThlDbContext(_options);
            var subject = new ProductService(_loggerMock.Object, context);

            var product = _seedData[20];
            
            // Act
            var result = await subject.GetProductById(product.Id);
            
            // Assert
            result.Should().BeEquivalentTo(product);
        }
        
        [Test]
        public void CreateProduct_Throws_WhenProductIsNull()
        {
            // Arrange// Arrange
            using var context = new ThlDbContext(_options);
            var subject = new ProductService(_loggerMock.Object, context);

            // Act
            Func<Task> action = async() => await subject.CreateProduct(null);
            
            // Assert
            action.Should().ThrowExactlyAsync<ArgumentNullException>();
        }
        
        [Test]
        public async Task CreateProduct_ReturnExpected()
        {
            // Arrange// Arrange
            await using var context = new ThlDbContext(_options);
            var subject = new ProductService(_loggerMock.Object, context);

            var product = _fixture.Create<Product>();
            product.Id = Guid.Empty;
            
            // Act
            var result = await subject.CreateProduct(product);
            
            // Assert
            result.Should().BeSameAs(product);
            product.Id.Should().NotBeEmpty();
            context.Products.Contains(product).Should().BeTrue();
        }
        
        [Test]
        public void UpdateProduct_Throws_WhenProductIsNull()
        {
            // Arrange// Arrange
            using var context = new ThlDbContext(_options);
            var subject = new ProductService(_loggerMock.Object, context);

            // Act
            Func<Task> action = async() => await subject.UpdateProduct(null);
            
            // Assert
            action.Should().ThrowExactlyAsync<ArgumentNullException>();
        }
        
        [Test]
        public async Task UpdateProduct_ReturnExpected()
        {
            // Arrange// Arrange
            await using var context = new ThlDbContext(_options);
            var subject = new ProductService(_loggerMock.Object, context);

            var product = _fixture.Create<Product>();
            context.Products.Add(product);
            context.SaveChanges();

            var newName = _fixture.Create<string>();
            product.Name = newName;
            
            // Act
            await subject.UpdateProduct(product);
            
            // Assert
            var expected = context.Products.Find(product.Id);
            expected.Name.Should().Be(newName);
        }
        
        [Test]
        public void DeleteProduct_Throws_WhenIdIsEmpty()
        {
            // Arrange// Arrange
            using var context = new ThlDbContext(_options);
            var subject = new ProductService(_loggerMock.Object, context);

            // Act
            Func<Task> action = async() => await subject.DeleteProduct(Guid.Empty);
            
            // Assert
            action.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
        }
        
        [Test]
        public void DeleteProduct_Throws_WhenProductNotExist()
        {
            // Arrange// Arrange
            using var context = new ThlDbContext(_options);
            var subject = new ProductService(_loggerMock.Object, context);

            // Act
            Func<Task> action = async() => await subject.DeleteProduct(Guid.NewGuid());
            
            // Assert
            action.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
        }
        
        [Test]
        public async Task DeleteProduct_ReturnExpected()
        {
            // Arrange// Arrange
            await using var context = new ThlDbContext(_options);
            var subject = new ProductService(_loggerMock.Object, context);

            var product = _fixture.Create<Product>();
            context.Products.Add(product);
            context.SaveChanges();

            // Act
            await subject.DeleteProduct(product.Id);
            
            // Assert
            var expected = context.Products.Find(product.Id);
            expected.Should().BeNull();
        }
    }
}