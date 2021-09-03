using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using THL.Domain;

namespace THL.DataAccess
{
    public interface IThlDbContext
    {
        int SaveChanges();
        int SaveChanges(bool acceptAllChangesOnSuccess);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken);
        
        DbSet<Product> Products { get; set; }
    }
}