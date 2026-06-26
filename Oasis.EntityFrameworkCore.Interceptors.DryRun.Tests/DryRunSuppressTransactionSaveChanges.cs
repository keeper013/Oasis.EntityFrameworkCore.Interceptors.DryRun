namespace Oasis.EntityFrameworkCore.Interceptors.DryRun.Tests;

using Microsoft.EntityFrameworkCore;

public sealed class DryRunSuppressTransactionSaveChanges : DryRunSuppressTransactionSaveChangesTestBase
{
    [Fact]
    public void DryRunSuppressTest()
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = true;
        using (var transaction = DbContext.Database.BeginTransaction())
        {
            var entity = new Entity1 { Id = 1, Name = "Test1" };
            DbContext.Add(entity);
            DbContext.SaveChanges();
            DbContext.Database.ExecuteSqlRaw("INSERT INTO Entity1 (Id, Name) VALUES (2, 'Test2')");
            transaction.Commit();
        }

        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommissionSuppressed]));
        Assert.False(DbContext.Set<Entity1>().Any());
        Assert.Single(DbContext.ChangeTracker.Entries());
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Added));
    }

    [Fact]
    public async Task DryRunSuppressTestAsync()
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = true;
        using (var transaction = await DbContext.Database.BeginTransactionAsync())
        {
            var entity = new Entity1 { Id = 1, Name = "Test2" };
            await DbContext.AddAsync(entity);
            await DbContext.SaveChangesAsync();
            await DbContext.Database.ExecuteSqlRawAsync("INSERT INTO Entity1 (Id, Name) VALUES (2, 'Test2')");
            await transaction.CommitAsync();
        }

        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommissionSuppressed]));
        Assert.False(await DbContext.Set<Entity1>().AnyAsync());
        Assert.Single(DbContext.ChangeTracker.Entries());
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Added));
    }

    [Fact]
    public void DryRunSuppressNoTransactionTest()
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = true;
        var entity = new Entity1 { Id = 1, Name = "Test1" };
        DbContext.Add(entity);
        DbContext.SaveChanges();
        DbContext.Database.ExecuteSqlRaw("INSERT INTO Entity1 (Id, Name) VALUES (2, 'Test2')");
        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.CommandExecution, DryRunEventTrace.SaveChangesSuppressed]));
        Assert.False(DbContext.Set<Entity1>().Any());
        Assert.Single(DbContext.ChangeTracker.Entries());
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Added));
    }

    [Fact]
    public async Task DryRunSuppressNoTransactionTestAsync()
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = true;
        var entity = new Entity1 { Id = 1, Name = "Test2" };
        await DbContext.AddAsync(entity);
        await DbContext.SaveChangesAsync();
        await DbContext.Database.ExecuteSqlRawAsync("INSERT INTO Entity1 (Id, Name) VALUES (2, 'Test2')");
        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.CommandExecution, DryRunEventTrace.SaveChangesSuppressed]));
        Assert.False(await DbContext.Set<Entity1>().AnyAsync());
        Assert.Single(DbContext.ChangeTracker.Entries());
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Added));
    }
}
