using Microsoft.EntityFrameworkCore;
using Homebites.Models;

namespace Homebites.Data
{
    public class HomebitesDbContext : DbContext
    {
        public HomebitesDbContext(DbContextOptions<HomebitesDbContext> options)
            : base(options) { }

        public DbSet<User>             Users              { get; set; }
        public DbSet<UserAddress>      UserAddresses      { get; set; }
        public DbSet<OtpVerification>  OtpVerifications   { get; set; }
        public DbSet<Meal>             Meals              { get; set; }
        public DbSet<Order>            Orders             { get; set; }
        public DbSet<OrderItem>        OrderItems         { get; set; }
        public DbSet<Payment>          Payments           { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}