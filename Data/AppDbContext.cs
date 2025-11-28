using Microsoft.EntityFrameworkCore;
using Hulujan_Iulia_Petruta_lab4M.Models;

namespace Hulujan_Iulia_Petruta_lab4M.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<PredictionHistory> PredictionHistories { get; set; }
    }
}
