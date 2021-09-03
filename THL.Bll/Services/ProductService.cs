using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using THL.DataAccess;
using THL.Domain;

namespace THL.Bll.Services
{
    public class ProductService : IProductService
    {
        private readonly ILogger<ProductService> _logger;
        private readonly IThlDbContext _dbContext;

        public ProductService(ILogger<ProductService> logger, IThlDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Product>> GetProducts(string searchTerm, int page, int pageSize)
        {
            if (page < 0)
            {
                _logger.LogError("Page cannot be less than 0");
                throw new ArgumentOutOfRangeException(nameof(page));
            }

            if (pageSize <= 0)
            {
                _logger.LogError("Page cannot be equal to or less than 0");
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _dbContext.Products.Skip(page * pageSize).Take(pageSize).ToListAsync();
            }
            else
            {
                return await _dbContext.Products
                    .Where(x => x.Name.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase))
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
        }

        public Task<Product> GetProductById(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogError("Id cannot be empty");
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            return _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Product> CreateProduct(Product product)
        {
            if (product == null)
            {
                _logger.LogError("Product cannot be null");
                throw new ArgumentNullException(nameof(product));
            }

            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync(CancellationToken.None);

            return product;
        }

        public async Task UpdateProduct(Product product)
        {
            if (product == null)
            {
                _logger.LogError("Product cannot be null");
                throw new ArgumentNullException(nameof(product));
            }

            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync(CancellationToken.None);
        }

        public async Task DeleteProduct(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogError("Id cannot be empty");
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogError("Product does not exist");
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync(CancellationToken.None);
        }
    }
}