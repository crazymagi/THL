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
        private IMapper _mapper;
        private Fixture _fixture;
        private ProductController _subject;

        [SetUp]
        public void Setup()
        {
            _productServiceMock = new Mock<IProductService>();
            _loggerMock = new Mock<ILogger<ProductController>>();
            _fixture = new Fixture();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MapperProfile>());
            _mapper = config.CreateMapper();

            _subject = new ProductController(_loggerMock.Object, _productServiceMock.Object, _mapper);
        }

        [TearDown]
        public void TearDown()
        {
            _productServiceMock.VerifyAll();
            _loggerMock.VerifyAll();
        }

        [TestCase(1, 0)]
        [TestCase(-1, 1)]
        [TestCase(1, -1)]
        public async Task GetAsync_ReturnBadRequest_WhenPageOrPageSizeAreInvalid(int page, int pageSize)
        {
            // Arrange
            var searchTerm = _fixture.Create<string>();

            // Act
            var result = await _subject.GetAsync(searchTerm, page, pageSize);
            
            // Assert
            var statusResult = result.Result as StatusCodeResult;
            statusResult.Should().NotBeNull();
            statusResult?.StatusCode.Should().Be(400);
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
            _productServiceMock.Setup(x => x.GetProductById(id)).ReturnsAsync((Product) null).Verifiable();
            
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
            _productServiceMock.Setup(x => x.GetProductById(id)).ReturnsAsync(product).Verifiable();
            
            // Act
            var result = await _subject.GetByIdAsync(id);
            
            // Assert
            result.Value.Should().BeEquivalentTo(expected);
        }
        
        [Test]
        public async Task Delete_ReturnBadRequest_WhenEmptyId()
        {
            // Act
            var result = await _subject.DeleteAsync(Guid.Empty);
            
            // Assert
            var statusResult = result.Result as StatusCodeResult;
            statusResult.Should().NotBeNull();
            statusResult?.StatusCode.Should().Be(400);
        }
        
        [Test, AutoData]
        public async Task Delete_ReturnNotFound_WhenProductNotExist(Guid id)
        {
            // Arrange
            _productServiceMock.Setup(x => x.GetProductById(id)).ReturnsAsync((Product)null).Verifiable();
            
            // Act
            var result = await _subject.DeleteAsync(id);
            
            // Assert
            var statusResult = result.Result as StatusCodeResult;
            statusResult.Should().NotBeNull();
            statusResult?.StatusCode.Should().Be(404);
        }

        [Test, AutoData]
        public async Task Delete_ReturnOK_WhenDeleteSuccess(Guid id)
        {
            // Arrange
            _productServiceMock.Setup(x => x.GetProductById(id)).ReturnsAsync(_fixture.Create<Product>()).Verifiable();
            _productServiceMock.Setup(x => x.DeleteProduct(id)).Returns(Task.CompletedTask).Verifiable();
            
            // Act
            var result = await _subject.DeleteAsync(id);
            
            // Assert
            var statusResult = result.Result as StatusCodeResult;
            statusResult.Should().NotBeNull();
            statusResult?.StatusCode.Should().Be(200);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public async Task Create_ReturnBadRequest_WhenNameIsNullOrWhitespaces(string name)
        {
            // Arrange
            var productDto = new ProductInfoDto()
            {
                Name = name,
                Description = _fixture.Create<string>(),
                Price = _fixture.Create<double>()
            };
            
            // Act
            var result = await _subject.CreateAsync(productDto);
            
            // Assert
            var statusResult = result.Result as StatusCodeResult;
            statusResult.Should().NotBeNull();
            statusResult?.StatusCode.Should().Be(400);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public async Task Create_ReturnBadRequest_WhenPriceIsNegativeOrZero(double price)
        {
            // Arrange
            var productDto = new ProductInfoDto()
            {
                Name = _fixture.Create<string>(),
                Description = _fixture.Create<string>(),
                Price = price
            };
            
            // Act
            var result = await _subject.CreateAsync(productDto);
            
            // Assert
            var statusResult = result.Result as StatusCodeResult;
            statusResult.Should().NotBeNull();
            statusResult?.StatusCode.Should().Be(400);
        }
        

        [Test, AutoData]
        public async Task Create_ReturnExpected_WhenDtoIsValid(ProductInfoDto productDto)
        {
            // Arrange
            var product = _mapper.Map<Product>(productDto);
            _productServiceMock
                .Setup(x => x.CreateProduct(It.IsAny<Product>()))
                .Callback<Product>(p =>
                {
                    p.Should().BeEquivalentTo(product);
                })
                .ReturnsAsync(product)
                .Verifiable();
            
            var expected = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price
            };
            
            // Act
            var result = await _subject.CreateAsync(productDto);
            
            // Assert
            result.Value.Should().BeEquivalentTo(expected);
        }
        
        [Test, AutoData]
        public async Task Update_ReturnBadRequest_WhenIdIsEmpty(ProductInfoDto productDto)
        {
            // Act
            var result = await _subject.UpdateAsync(Guid.Empty, productDto);
            
            // Assert
            var statusResult = result.Result as StatusCodeResult;
            statusResult.Should().NotBeNull();
            statusResult?.StatusCode.Should().Be(400);
        }
        
        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public async Task Update_ReturnBadRequest_WhenNameIsNullOrWhitespaces(string name)
        {
            // Arrange
            var id = _fixture.Create<Guid>();
            var productDto = new ProductInfoDto()
            {
                Name = name,
                Description = _fixture.Create<string>(),
                Price = _fixture.Create<double>()
            };
            
            // Act
            var result = await _subject.UpdateAsync(id, productDto);
            
            // Assert
            var statusResult = result.Result as StatusCodeResult;
            statusResult.Should().NotBeNull();
            statusResult?.StatusCode.Should().Be(400);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public async Task Update_ReturnBadRequest_WhenPriceIsNegativeOrZero(double price)
        {
            // Arrange
            var id = _fixture.Create<Guid>();
            var productDto = new ProductInfoDto()
            {
                Name = _fixture.Create<string>(),
                Description = _fixture.Create<string>(),
                Price = price
            };
            
            // Act
            var result = await _subject.UpdateAsync(id, productDto);
            
            // Assert
            var statusResult = result.Result as StatusCodeResult;
            statusResult.Should().NotBeNull();
            statusResult?.StatusCode.Should().Be(400);
        }
        
        [Test, AutoData]
        public async Task Update_ReturnNotFound_WhenIdNotExists(Guid id,ProductInfoDto productDto)
        {
            // Arrange
            var product = _mapper.Map<Product>(productDto);
            product.Id = id;
            
            _productServiceMock
                .Setup(x => x.GetProductById(id))
                .ReturnsAsync((Product)null)
                .Verifiable();
            
            // Act
            var result = await _subject.UpdateAsync(id, productDto);
            
            // Assert
            var statusResult = result.Result as StatusCodeResult;
            statusResult.Should().NotBeNull();
            statusResult?.StatusCode.Should().Be(404);
        }

        [Test, AutoData]
        public async Task Update_ReturnExpected_WhenDtoIsValid(Guid id, ProductInfoDto productDto)
        {
            // Arrange
            var product = _fixture.Create<Product>();
            product.Id = id;
            
            _productServiceMock
                .Setup(x => x.GetProductById(id))
                .ReturnsAsync(product)
                .Verifiable();
            
            _productServiceMock
                .Setup(x => x.UpdateProduct(product))
                .Returns(Task.CompletedTask)
                .Verifiable();
            
            var expected = new ProductDto
            {
                Id = id,
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price
            };
            
            // Act
            var result = await _subject.UpdateAsync(id, productDto);
            
            // Assert
            result.Value.Should().BeEquivalentTo(expected);
            product.Name.Should().Be(productDto.Name);
            product.Description.Should().Be(productDto.Description);
            product.Price.Should().Be(productDto.Price);
        }
    }
}