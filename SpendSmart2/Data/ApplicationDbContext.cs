using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore; // ✅ ADD
using Microsoft.EntityFrameworkCore;
using SpendSmart2.Models;

namespace SpendSmart2.Data
{
    public class ApplicationDbContext : DbContext, IDataProtectionKeyContext // ✅ ADD INTERFACE
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Expense> Expenses { get; set; }

        // ✅ ADD THIS: For storing encryption keys
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasColumnType("numeric(18,2)");
        }
    }
}