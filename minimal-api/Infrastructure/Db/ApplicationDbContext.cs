using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entities;

namespace minimal_api.Infrastructure.Db
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public ApplicationDbContext(IConfiguration configuration)
        {
            _configuration = configuration; 
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Administrator>().HasData(
                new Administrator
                {
                    Id = 1,
                    Email = "administrador@teste.com",
                    Password = "123456",
                    Profile = "Adm"
                }    
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //base.OnConfiguring(optionsBuilder);
            var conectionString = _configuration.GetConnectionString("MySql")?.ToString();

            if (!string.IsNullOrEmpty(conectionString))
            {
                optionsBuilder.UseMySql(conectionString, ServerVersion.AutoDetect(conectionString));
            }
        }

        public DbSet<Administrator> Administrators { get; set; }
    }
}
