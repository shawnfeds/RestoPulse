using Microsoft.EntityFrameworkCore;
using RestoPulse.BillingService.Domain.Entities;
using RestoPulse.BillingService.Domain.Enums;

namespace RestoPulse.BillingService.Infrastructure.Persistence;

public class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
{
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillItem> BillItems => Set<BillItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bill>(e =>
        {
            e.ToTable("Bills");
            e.HasKey(x => x.Id);
            e.Property(x => x.BillNo).IsRequired().HasMaxLength(30);
            e.Property(x => x.OrderNo).IsRequired().HasMaxLength(30);
            e.Property(x => x.Subtotal).HasColumnType("decimal(10,2)");
            e.Property(x => x.DiscountAmount).HasColumnType("decimal(10,2)");
            e.Property(x => x.TaxableAmount).HasColumnType("decimal(10,2)");
            e.Property(x => x.TaxAmount).HasColumnType("decimal(10,2)");
            e.Property(x => x.Total).HasColumnType("decimal(10,2)");
            e.Property(x => x.TaxRate).HasColumnType("decimal(5,2)");
            e.Property(x => x.AmountTendered).HasColumnType("decimal(10,2)");
            e.Property(x => x.ChangeReturned).HasColumnType("decimal(10,2)");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.BillNo).IsUnique();
            e.HasIndex(x => x.OrderNo);
            e.Ignore(x => x.Events);

            e.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(x => x.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Navigation(x => x.Items)
             .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<BillItem>(e =>
        {
            e.ToTable("BillItems");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Price).HasColumnType("decimal(10,2)");
        });
    }
}