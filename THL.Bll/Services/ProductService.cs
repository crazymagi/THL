using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using THL.Domain;

namespace THL.Bll.Services
{
    public class ProductService : IProductService
    {
        public async Task<IEnumerable<Product>> GetProducts(string searchTerm, int page, int pageSize)
        {
            return await Task.FromResult(new[] {new Product(), new Product()});
        }

        public Task<Product> GetProductById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Product> CreateProduct(Product product)
        {
            throw new NotImplementedException();
        }

        public Task UpdateProduct(Product product)
        {
            throw new NotImplementedException();
        }

        public Task DeleteProduct(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}