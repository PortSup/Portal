using Microsoft.EntityFrameworkCore;
using VendorPortalAPI.Models;



namespace VendorPortalAPI.Data
{
    public class VendorDbContext : DbContext
    {
        public VendorDbContext(DbContextOptions<VendorDbContext> options)
            : base(options)
        {
        }

        public DbSet<VendorProfile> VendorProfiles { get; set; }
        public DbSet<TaxCompliance> TaxCompliance { get; set; }
        public DbSet<BankInformation> BankInformation { get; set; }
        public DbSet<BBBEEInformation> BBBEEInformation { get; set; }
        public DbSet<FinancialStatement> FinancialStatements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VendorProfile>()
                .HasOne(v => v.TaxCompliance)
                .WithOne(t => t.VendorProfile)
                .HasForeignKey<TaxCompliance>(t => t.VendorProfileId);

            modelBuilder.Entity<VendorProfile>()
                .HasOne(v => v.BankInformation)
                .WithOne(b => b.VendorProfile)
                .HasForeignKey<BankInformation>(b => b.VendorProfileId);

            modelBuilder.Entity<VendorProfile>()
                .HasOne(v => v.BBBEEInformation)
                .WithOne(b => b.VendorProfile)
                .HasForeignKey<BBBEEInformation>(b => b.VendorProfileId);

            modelBuilder.Entity<VendorProfile>()
                .HasOne(v => v.FinancialStatement)
                .WithOne(f => f.VendorProfile)
                .HasForeignKey<FinancialStatement>(f => f.VendorProfileId);
        }
    }
}