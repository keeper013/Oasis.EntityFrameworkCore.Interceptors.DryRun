namespace Oasis.EntityFrameworkCore.Interceptors.DryRun.Tests;

using Microsoft.EntityFrameworkCore;
using Oasis.EntityFrameworkCore.Interceptors.DryRun;

public sealed class DbContext(DryRunInterceptor intercpetor, DbContextOptions options)
    : DbContext<DryRunInterceptor, IDryRunHandle>(intercpetor, options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entity1>().ToTable(nameof(Entity1));
        modelBuilder.Entity<Entity1>().HasKey(e => e.Id);
        modelBuilder.Entity<Entity1>().Property(e => e.Name).IsRequired();
        base.OnModelCreating(modelBuilder);
    }
}

internal sealed class InitializerDbContext(DbContextOptions options)
    : Microsoft.EntityFrameworkCore.DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entity1>().ToTable(nameof(Entity1));
        modelBuilder.Entity<Entity1>().HasKey(e => e.Id);
        modelBuilder.Entity<Entity1>().Property(e => e.Name).IsRequired();
        base.OnModelCreating(modelBuilder);
    }
}