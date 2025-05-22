using Diplom.Models;
using Microsoft.EntityFrameworkCore;

namespace Diplom.Data;

public class ApplicationDbContext : DbContext
{
    // Конструктор
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSet для каждой из моделей
    public DbSet<Accounts> Accounts { get; set; }
    public DbSet<ExpChanges> ExpChanges { get; set; }
    public DbSet<ExpUsersWallets> ExpUsersWallets { get; set; }
    public DbSet<Config> Config { get; set; }
    public DbSet<Orders> Orders { get; set; }
    public DbSet<OrdersHistory> OrdersHistory { get; set; }
    public DbSet<ProductsStore> ProductsStore { get; set; }
    public DbSet<CategoriesStore> CategoriesStore { get; set; }
    public DbSet<Discounts> Discounts { get; set; }
    public DbSet<UserDiscounts> UserDiscounts { get; set; }
    public DbSet<UserDiscountsHistory> UserDiscountsHistory { get; set; }
    public DbSet<UserDiscountsActivated> UserDiscountsActivated { get; set; }
    public DbSet<UserDiscountsActivatedHistory> UserDiscountsActivatedHistory { get; set; }
    public DbSet<ExchangeDiscounts> ExchangeDiscounts { get; set; }
    public DbSet<AccountPasswords> AccountPasswords { get; set; }
    public DbSet<Roles> Role { get; set; }
    public DbSet<AccountRole> AccountRole { get; set; }


    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Accounts>(z => { z.HasKey(e => e.Id); }
        );


        
        modelBuilder.Entity<ExpUsersWallets>(z =>
        {
            z.HasKey(e => e.Id);
            z.HasOne(eu => eu.Account)
                .WithOne()
                .HasForeignKey<ExpUsersWallets>(eu => eu.AccountId);
        });
        
        modelBuilder.Entity<ExpChanges>(z =>
        {
            z.HasKey(e => e.Id);
            z.HasOne<Accounts>(ec => ec.Accounts)
                .WithMany()
                .HasForeignKey(ec => ec.AccountId);
            z.HasOne<ExpUsersWallets>(ec => ec.ExpUsersWallets)
                .WithMany()
                .HasForeignKey(ec => ec.ExpUserId);
        });

        modelBuilder.Entity<Config>(z => { z.HasNoKey(); });

        modelBuilder.Entity<Orders>(z =>
        {
            z.HasKey(e => e.Id);
            z.HasOne<Accounts>(ec => ec.Accounts)
                .WithMany()
                .HasForeignKey(ec => ec.AccountId);
        });

        modelBuilder.Entity<OrdersHistory>(z =>
        {
            z.HasKey(e => e.Id);
            z.HasOne<Accounts>(ec => ec.Accounts)
                .WithMany()
                .HasForeignKey(ec => ec.AccountId);
        });

        modelBuilder.Entity<ProductsStore>(z => { z.HasKey(e => e.Id); });

        modelBuilder.Entity<CategoriesStore>(z => { z.HasKey(e => e.Id); });
        
        modelBuilder.Entity<Discounts>(z =>
        {
            z.HasKey(e => e.Id);
            z.HasMany<ProductsStore>(ec => ec.ProductsId)
                .WithMany()
                .UsingEntity(j => j.ToTable("DiscountsProductsStore"));
                    
            z.HasMany<CategoriesStore>(ec => ec.CategoriesId)
                .WithMany()
                .UsingEntity(j => j.ToTable("CategoriesStoreDiscounts"));


        });

        modelBuilder.Entity<UserDiscounts>(z =>
        {
            z.HasKey(e => e.Id);
            z.HasOne<Accounts>(ec => ec.Accounts)
                .WithMany()
                .HasForeignKey(ec => ec.AccountId);
            z.HasOne<Discounts>(ec => ec.Discounts)
                .WithMany()
                .HasForeignKey(ec => ec.DiscountId);
        });

        modelBuilder.Entity<UserDiscountsActivated>(z =>
        {
            z.HasKey(e => e.Id);
            z.HasOne<Accounts>(ec => ec.Accounts)
                .WithMany()
                .HasForeignKey(ec => ec.AccountId);
            z.HasOne<Discounts>(ec => ec.Discounts)
                .WithMany()
                .HasForeignKey(ec => ec.DiscountId);
        });

        modelBuilder.Entity<UserDiscountsHistory>(z =>
        {
            z.HasKey(e => e.Id);
            z.HasOne<Accounts>(ec => ec.Account)
                .WithMany()
                .HasForeignKey(ec => ec.AccountId);
            z.HasOne<Discounts>(ec => ec.Discount)
                .WithMany()
                .HasForeignKey(ec => ec.DiscountId);
                
        });


        modelBuilder.Entity<UserDiscountsActivatedHistory>(z =>
        {
            z.HasNoKey();
            z.HasOne<Accounts>(ec => ec.Accounts)
                .WithMany()
                .HasForeignKey(ec => ec.AccountId);
            z.HasOne<Discounts>(ec => ec.Discounts)
                .WithMany()
                .HasForeignKey(ec => ec.DiscountId);
        });

        modelBuilder.Entity<ExchangeDiscounts>(z =>
        {
            z.HasKey(e => e.Id);
            z.HasOne<Discounts>(ec => ec.Discount)
                .WithMany()
                .HasForeignKey(ec => ec.DiscountId);
            z.HasOne<Discounts>(ec => ec.DiscountOne)
                .WithMany()
                .HasForeignKey(ec => ec.DiscountExchangeOneId);
            z.HasOne<Discounts>(ec => ec.DiscountTwo)
                .WithMany()
                .HasForeignKey(ec => ec.DiscountExchangeTwoId);
        });

        modelBuilder.Entity<Roles>(z =>
        {
            z.HasKey(e => e.Id); 
        });

        modelBuilder.Entity<AccountRole>(z =>
        {
            z.HasKey(e => e.Id);
            z.HasOne<Accounts>(ec => ec.Account)
                .WithOne()
                .HasForeignKey<AccountRole>(ec => ec.AccountId);
            z.HasOne<Roles>(ec => ec.Role)
                .WithOne()
                .HasForeignKey<AccountRole>(ec => ec.RoleId);
        });

        modelBuilder.Entity<AccountPasswords>(z =>
        {
            z.HasKey(e => e.Id);
            z.HasOne<Accounts>(ec => ec.Account)
                .WithOne()
                .HasForeignKey<AccountPasswords>(ec => ec.AccountId);
        });
        
        
    }
}