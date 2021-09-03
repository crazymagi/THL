using Microsoft.EntityFrameworkCore;
using THL.Domain;

namespace THL.DataAccess
{
    public class ThlDbContext : DbContext, IThlDbContext
    {
        public ThlDbContext(DbContextOptions<ThlDbContext> options)
            : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ThlDbContext).Assembly);
        }
        
        public DbSet<Product> Products { get; set; }
    }
}