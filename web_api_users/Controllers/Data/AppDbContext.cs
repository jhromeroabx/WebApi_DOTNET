using Microsoft.EntityFrameworkCore;
using web_api_users.Controllers.Models;

namespace web_api_users.Controllers.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {

        }

        public DbSet<Categoria> Categorias { get; set; }
    }
}
