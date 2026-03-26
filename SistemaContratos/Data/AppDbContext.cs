using Microsoft.EntityFrameworkCore;
using SistemaContratos.Models;

namespace SistemaContratos.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Contrato> Contratos { get; set; }
    }
}
