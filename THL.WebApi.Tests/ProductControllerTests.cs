using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using THL.Bll.Services;
using THL.Domain;
using THL.WebApi.Controllers;
using THL.WebApi.Dtos;

namespace THL.WebApi.Tests
{
    [TestFixture]
    public class ProductControllerTests
    {
        private Mock<IProductService> _productServiceMock;
        private Mock<ILogger<ProductController>> _loggerMock;
        private Fixture _fixture;
        private ProductController _subject;

        [SetUp]
        public void Setup()
        {
            _productServiceMock = new Mock<IProductService>();
            _loggerMock = new Mock<ILogger<ProductController>>();
            _fixture = new Fixture();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MapperProfile>());
            var mapper = config.CreateMapper();

            _subject = new ProductController(_loggerMock.Object, _productServiceMock.Object, mapper);
        }

        [TearDown]
        public void TearDown()
        {
            _productServiceMock.VerifyAll();
            _loggerMock.VerifyAll();
        }

        [Test, AutoData]
        public async Task GetAsync_ReturnExpected(string searchTerm, int page, int pageSize)
        {
            // Arrange
            var products = _fixture.CreateMany<Product>().ToList();
            var expected = products.Select(x => new ProductDto()
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price
            });
            _productServiceMock.Setup(x => x.GetProducts(searchTerm, page, pageSize)).ReturnsAsync(products).Verifiable();

            // Act
            var result = await _subject.GetAsync(searchTerm, page, pageSize);
            
            // Assert
            result.Value.Should().BeEquivalentTo(expected);
        }
        
        [Test]
        public async Task GetByIdAsync_ReturnBadRequest_WhenEmptyId()
        {
            // Act
            var result = await _subject.GetByIdAsync(Guid.Empty);
            
            // Assert
            var statusResult = result.Result as StatusCodeResult;
            statusResult.Should().NotBeNull();
            statusResult?.StatusCode.Should().Be(400);
        }

        [Test, AutoData]
        public async Task GetByIdAsync_ReturnNotFound_WhenNull(Guid id)
        {
            // Arrange
            _productServiceMock.Setup(x => x.GetProductById(id)).ReturnsAsync((Product) null);
            
            // Act
            var result = await _subject.GetByIdAsync(id);
            
            // Assert
            var statusResult = result.Result as StatusCodeResult;
            statusResult.Should().NotBeNull();
            statusResult?.StatusCode.Should().Be(404);
        }
        
        [Test, AutoData]
        public async Task GetByIdAsync_ReturnExpected_WhenNotNull(Guid id)
        {
            // Arrange
            var product = _fixture
                .Build<Product>()
                .With(x => x.Id, id)
                .Create();
            var expected = new ProductDto()
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price
            };
            _productServiceMock.Setup(x => x.GetProductById(id)).ReturnsAsync(product);
            
            // Act
            var result = await _subject.GetByIdAsync(id);
            
            // Assert
            result.Value.Should().BeEquivalentTo(expected);
        }
    }
}