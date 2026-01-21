using EDA.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace EDA.INFRAESTRUCTURE
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Family> Families { get; set; }
        public DbSet<Tax> Taxes { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Cai> Cais { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<SoldProduct> SoldProducts { get; set; }
        public DbSet<InvoicePayment> InvoicePayments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            });

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(256);
                entity.HasOne(e => e.Role)
                      .WithMany()
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Company).HasMaxLength(200);
                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // Company
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Owner).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Address1).HasMaxLength(300);
                entity.Property(e => e.Address2).HasMaxLength(300);
                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.PhoneNumber1).HasMaxLength(20);
                entity.Property(e => e.PhoneNumber2).HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // Family
            modelBuilder.Entity<Family>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // Tax
            modelBuilder.Entity<Tax>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Percentage).HasPrecision(5, 2);
            });

            // Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Barcode).HasMaxLength(50);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.HasOne(e => e.Family)
                      .WithMany()
                      .HasForeignKey(e => e.FamilyId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Tax)
                      .WithMany()
                      .HasForeignKey(e => e.TaxId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Discount
            modelBuilder.Entity<Discount>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Percentage).HasPrecision(5, 2);
            });

            // Cai
            modelBuilder.Entity<Cai>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Prefix).IsRequired().HasMaxLength(20);
            });

            // PaymentType
            modelBuilder.Entity<PaymentType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            });

            // Invoice
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Subtotal).HasPrecision(18, 2);
                entity.Property(e => e.Total).HasPrecision(18, 2);
                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Cai)
                      .WithMany()
                      .HasForeignKey(e => e.CaiId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Discount)
                      .WithMany()
                      .HasForeignKey(e => e.DiscountId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.SoldProducts)
                      .WithOne()
                      .HasForeignKey(e => e.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.InvoicePayments)
                      .WithOne(e => e.Invoice)
                      .HasForeignKey(e => e.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SoldProduct
            modelBuilder.Entity<SoldProduct>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).HasMaxLength(300);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.TotalLine).HasPrecision(18, 2);
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Tax)
                      .WithMany()
                      .HasForeignKey(e => e.TaxId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Discount)
                      .WithMany()
                      .HasForeignKey(e => e.DiscountId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // InvoicePayment
            modelBuilder.Entity<InvoicePayment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.HasOne(e => e.PaymentType)
                      .WithMany()
                      .HasForeignKey(e => e.PaymentTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed Data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Customer por defecto
            modelBuilder.Entity<Customer>().HasData(
                new Customer
                {
                    Id = 1,
                    Name = "Consumidor Final"
                }
            );
            modelBuilder.Entity<PaymentType>().HasData(
                new PaymentType { Id = 1, Name = "Efectivo" }
            );

            // Impuestos de Honduras
            modelBuilder.Entity<Tax>().HasData(
                new Tax { Id = 1, Name = "Exento", Percentage = 0m },
                new Tax { Id = 2, Name = "ISV 15%", Percentage = 15m },
                new Tax { Id = 3, Name = "ISV 18%", Percentage = 18m }
            );
            modelBuilder.Entity<Discount>().HasData(
                new Discount { Id = 1, Name = "Tercera Edad", Percentage = 25m }
            );
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" }
            );
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Name = "Admin",
                    LastName = "User",
                    Password = "WakeUpNe0",
                    RoleId = 1
                }
            );
        }
    }
}
