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
        public DbSet<PendingSale> PendingSales { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<ExpenseAccount> ExpenseAccounts { get; set; }
        public DbSet<PurchaseBill> PurchaseBills { get; set; }
        public DbSet<PurchaseBillPayment> PurchaseBillPayments { get; set; }
        public DbSet<PrinterConfiguration> PrinterConfigurations { get; set; }
        public DbSet<CashRegister> CashRegisters { get; set; }
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
                entity.Property(e => e.RTN).HasMaxLength(20);
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
                entity.Property(e => e.RTN).HasMaxLength(20);
                entity.Property(e => e.Address1).HasMaxLength(300);
                entity.Property(e => e.Address2).HasMaxLength(300);
                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.PhoneNumber1).HasMaxLength(20);
                entity.Property(e => e.PhoneNumber2).HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Logo).HasColumnType("varbinary(max)");
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
                entity.Property(e => e.MinStock).HasDefaultValue(0);
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
                entity.Property(e => e.TaxedAt15Percent).HasPrecision(18, 2);
                entity.Property(e => e.TaxesAt15Percent).HasPrecision(18, 2);
                entity.Property(e => e.TaxedAt18Percent).HasPrecision(18, 2);
                entity.Property(e => e.TaxesAt18Percent).HasPrecision(18, 2);
                entity.Property(e => e.Exempt).HasPrecision(18, 2);
                entity.Property(e => e.OutstandingAmount).HasPrecision(18, 2);
                entity.Property(e => e.Status).IsRequired();
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
                entity.HasOne(e => e.CashRegister)
                      .WithMany()
                      .HasForeignKey(e => e.CashRegisterId)
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

            // PendingSale
            modelBuilder.Entity<PendingSale>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.JsonData).IsRequired();
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Shift
            modelBuilder.Entity<Shift>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ShiftType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.InitialAmount).HasPrecision(18, 2);
                entity.Property(e => e.FinalCashAmount).HasPrecision(18, 2);
                entity.Property(e => e.FinalCardAmount).HasPrecision(18, 2);
                entity.Property(e => e.FinalAmount).HasPrecision(18, 2);
                entity.Property(e => e.ExpectedAmount).HasPrecision(18, 2);
                entity.Property(e => e.Difference).HasPrecision(18, 2);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.CashRegister)
                      .WithMany()
                      .HasForeignKey(e => e.CashRegisterId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Provider
            modelBuilder.Entity<Provider>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RTN).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ContactPerson).HasMaxLength(100);
                entity.Property(e => e.ContactPhoneNumber).HasMaxLength(20);
                entity.Property(e => e.ContactEmail).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.WebsiteUrl).HasMaxLength(200);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ModificatedBy).HasMaxLength(100);
            });
            // ExpenseAccount
            modelBuilder.Entity<ExpenseAccount>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });
            // PurchaseBill
            modelBuilder.Entity<PurchaseBill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PurchaseBillCode).IsRequired().HasMaxLength(8);
                entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Cai).IsRequired().HasMaxLength(19);
                entity.Property(e => e.Exempt).HasPrecision(18, 2);
                entity.Property(e => e.Exonerated).HasPrecision(18, 2);
                entity.Property(e => e.TaxedAt15Percent).HasPrecision(18, 2);
                entity.Property(e => e.TaxedAt18Percent).HasPrecision(18, 2);
                entity.Property(e => e.Taxes15Percent).HasPrecision(18, 2);
                entity.Property(e => e.Taxes18Percent).HasPrecision(18, 2);
                entity.Property(e => e.Total).HasPrecision(18, 2);
                entity.Property(e => e.OutstandingAmount).HasPrecision(18, 2);
                entity.HasOne(e => e.Provider)
                      .WithMany()
                      .HasForeignKey(e => e.ProviderId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ExpenseAccount)
                      .WithMany()
                      .HasForeignKey(e => e.ExpenseAccountId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            // PurchaseBillPayment
            modelBuilder.Entity<PurchaseBillPayment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.PaymentType)
                      .WithMany()
                      .HasForeignKey(e => e.PaymentTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.PurchaseBill)
                      .WithMany()
                      .HasForeignKey(e => e.PurchaseBillId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PrinterConfiguration
            modelBuilder.Entity<PrinterConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PrinterType).IsRequired();
                entity.Property(e => e.PrinterName).HasMaxLength(200);
                entity.Property(e => e.FontSize).HasDefaultValue(8);
                entity.Property(e => e.CopyStrategy).IsRequired();
                entity.Property(e => e.CopiesCount).HasDefaultValue(1);
                entity.Property(e => e.PrintWidth).HasDefaultValue(80);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // CashRegister
            modelBuilder.Entity<CashRegister>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasOne(e => e.PrinterConfiguration)
                      .WithMany()
                      .HasForeignKey(e => e.PrinterConfigurationId)
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
            //Default Expense Accounts
            modelBuilder.Entity<ExpenseAccount>().HasData(
                new ExpenseAccount { Id = 1, Name = "Alquiler" },
                new ExpenseAccount { Id = 2, Name = "Servicios Publicos" },
                new ExpenseAccount { Id = 3, Name = "Sueldos y Salarios" },
                new ExpenseAccount { Id = 4, Name = "Materiales y Suministros" },
                new ExpenseAccount { Id = 5, Name = "Publicidad y Marketing" },
                new ExpenseAccount { Id = 6, Name = "Gastos de Viaje" },
                new ExpenseAccount { Id = 7, Name = "Gastos de Oficina" },
                new ExpenseAccount { Id = 8, Name = "Mantenimiento y Reparaciones" },
                new ExpenseAccount { Id = 9, Name = "Gastos Financieros" },
                new ExpenseAccount { Id = 10, Name = "Otros Gastos" }
            );

            // Default PrinterConfiguration
            modelBuilder.Entity<PrinterConfiguration>().HasData(
                new PrinterConfiguration
                {
                    Id = 1,
                    Name = "Impresora Termica 80mm",
                    PrinterType = 1, // Thermal
                    FontSize = 8,
                    CopyStrategy = 2, // DoublePrint
                    CopiesCount = 2,
                    PrintWidth = 80,
                    IsActive = true,
                    CreationDate = new DateTime(2024, 1, 1)
                }
            );

            // Default CashRegister
            modelBuilder.Entity<CashRegister>().HasData(
                new CashRegister
                {
                    Id = 1,
                    Name = "Caja Principal",
                    Code = "C001",
                    IsActive = true,
                    CreationDate = new DateTime(2024, 1, 1),
                    PrinterConfigurationId = 1
                }
            );
        }
    }
}
