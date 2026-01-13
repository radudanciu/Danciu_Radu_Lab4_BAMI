using Microsoft.EntityFrameworkCore;
using Danciu_Radu_Lab4_BAMI.Models;

namespace Danciu_Radu_Lab4_BAMI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<PredictionHistory> PredictionHistories { get; set; }
    }
}