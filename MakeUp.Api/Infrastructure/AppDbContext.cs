using Application.Interfaces;
using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Entities.Product;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class AppDbContext : IdentityDbContext<
        UserEntity,
        RoleEntity,
        long,
        IdentityUserClaim<long>,
        UserRoleEntity,
        UserLoginEntity,
        IdentityRoleClaim<long>,
        IdentityUserToken<long>
    >, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options) { }

    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<BrandEntity> Brands { get; set; }
    public DbSet<ProductImageEntity> ProductImages { get; set; }
    public DbSet<ProductAttributeEntity> ProductAttributes { get; set; }
    public DbSet<AttributeValueEntity> AttributeValues { get; set; }
    public DbSet<ProductVariantEntity> ProductVariants { get; set; }
    public DbSet<ReviewEntity> Reviews { get; set; }
    public DbSet<CartEntity> Carts { get; set; }
    public DbSet<FavoriteEntity> Favorites { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRoleEntity>(ur =>
        {
            ur.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(r => r.RoleId)
                .IsRequired();

            ur.HasOne(ur => ur.User)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(u => u.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<UserLoginEntity>(b =>
        {
            b.HasOne(l => l.User)
                .WithMany(u => u.UserLogins)
                .HasForeignKey(l => l.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<CartEntity>(c =>
        {
            c.HasIndex(c => new { c.UserId, c.ProductId })
                .IsUnique();
        });

        modelBuilder.Entity<FavoriteEntity>(f =>
        {
            f.HasIndex(f => new { f.UserId, f.ProductId })
                .IsUnique();
        });

        modelBuilder.Entity<ReviewEntity>(r =>
        {
            r.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId);

            r.HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId);

            r.HasOne(r => r.Parent)
                .WithMany(r => r.Replies)
                .HasForeignKey(r => r.ParentId);
        });

        modelBuilder.Entity<ProductVariantEntity>(v =>
        {
            v.HasIndex(v => new { v.ProductId, v.Sku })
                .IsUnique();
        });

        modelBuilder.Entity<ProductImageEntity>(i =>
        {
            i.HasOne(i => i.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(i => i.ProductId);
        });

        modelBuilder.Entity<ProductAttributeValueEntity>(av =>
        {
            av.HasKey(pav => new { pav.ProductId, pav.AttributeValueId });

            av.HasOne(av => av.Product)
                .WithMany(p => p.AttributeValues)
                .HasForeignKey(av => av.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            av.HasOne(av => av.AttributeValue)
                .WithMany(a => a.ProductAttributeValues)
                .HasForeignKey(av => av.AttributeValueId);
        });

        modelBuilder.Entity<ProductAttributeEntity>(a =>
        {
            a.HasMany(a => a.Values)
                .WithOne(av => av.ProductAttribute)
                .HasForeignKey(av => av.ProductAttributeId);
        });

        modelBuilder.Entity<BrandEntity>(b =>
        {
            b.HasMany(b => b.Products)
                .WithOne(p => p.Brand)
                .HasForeignKey(p => p.BrandId);
        });

        modelBuilder.Entity<AttributeValueEntity>(av =>
        {
            av.HasMany(av => av.ProductAttributeValues)
                .WithOne(pav => pav.AttributeValue)
                .HasForeignKey(pav => pav.AttributeValueId);

            av.HasOne(av => av.ProductAttribute)
                .WithMany(pa => pa.Values)
                .HasForeignKey(av => av.ProductAttributeId);
        });
    }
}
