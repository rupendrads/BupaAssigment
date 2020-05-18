using Microsoft.EntityFrameworkCore;

namespace EnrollmentAPI.Models
{
    public class EnrollmentContext : DbContext
    {
        public EnrollmentContext(DbContextOptions<EnrollmentContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FirstName = "admin",
                    LastName = "admin",
                    Username = "admin",
                    Password = "admin",
                    Role = Role.Admin,
                    Token = null
                },
                new User {
                    Id = 2,
                    FirstName = "test",
                    LastName = "test",
                    Username = "test",
                    Password = "test",
                    Role = Role.User,
                    Token = null
                }
            );
        }
    }
}