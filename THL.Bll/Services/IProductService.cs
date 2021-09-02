using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using THL.Domain;

namespace THL.Bll.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProducts(string searchTerm, int page, int pageSize);

        Task<Product> GetProductById(Guid id);

        Task<Product> CreateProduct(Product product);

        Task UpdateProduct(Product product);

        Task DeleteProduct(Guid id);
    }
}