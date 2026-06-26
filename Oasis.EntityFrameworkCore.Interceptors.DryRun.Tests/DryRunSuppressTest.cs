namespace Oasis.EntityFrameworkCore.Interceptors.DryRun.Tests;

using Microsoft.EntityFrameworkCore;

public sealed class DryRunSuppressTest : DryRunSuppressAllTestBase
{
    [Fact]
    public void DryRunSuppressTest_ExecuteCommand()
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = true;
        DbContext.Database.ExecuteSqlRaw("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test')");
        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.CommandSuppressed]));
        Assert.False(DbContext.Set<Entity1>().Any());
    }

    [Fact]
    public async Task DryRunSuppressTest_ExecuteCommandAsync()
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = true;
        await DbContext.Database.ExecuteSqlRawAsync("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test')");
        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.CommandSuppressed]));
        Assert.False(DbContext.Set<Entity1>().Any());
    }

    [Fact]
    public void DryRunSuppressTest_SaveChanges()
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = true;
        var entity = new Entity1 { Id = 1, Name = "Test" };
        DbContext.Add(entity);
        DbContext.SaveChanges();
        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.SaveChangesSuppressed]));
        Assert.False(DbContext.Set<Entity1>().Any());
        Assert.Single(DbContext.ChangeTracker.Entries());
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Added));
    }

    [Fact]
    public async Task DryRunSuppressTest_SaveChangesAsync()
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = true;
        var entity = new Entity1 { Id = 1, Name = "Test" };
        await DbContext.AddAsync(entity);
        await DbContext.SaveChangesAsync();
        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.SaveChangesSuppressed]));
        Assert.False(await DbContext.Set<Entity1>().AnyAsync());
        Assert.Single(DbContext.ChangeTracker.Entries());
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Added));
    }

    [Fact]
    public void DryRunSuppressTest_TransactionSaveChanges()
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = true;
        using (var transaction = DbContext.Database.BeginTransaction())
        {
            var entity = new Entity1 { Id = 1, Name = "Test" };
            DbContext.Add(entity);
            DbContext.SaveChanges();
            transaction.Commit();
        }

        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommissionSuppressed]));
        Assert.False(DbContext.Set<Entity1>().Any());
        Assert.Single(DbContext.ChangeTracker.Entries());
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Added));
    }

    [Fact]
    public async Task DryRunSuppressTest_TransactionSaveChangesAsync()
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = true;
        using (var transaction = await DbContext.Database.BeginTransactionAsync())
        {
            var entity = new Entity1 { Id = 1, Name = "Test" };
            await DbContext.AddAsync(entity);
            await DbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommissionSuppressed]));
        Assert.False(await DbContext.Set<Entity1>().AnyAsync());
        Assert.Single(DbContext.ChangeTracker.Entries());
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Added));
    }

    [Fact]
    public void DryRunSuppressTest_TransactionDbCommand()
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = true;
        using (var transaction = DbContext.Database.BeginTransaction())
        {
            DbContext.Database.ExecuteSqlRaw("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test')");
            transaction.Commit();
        }

        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommissionSuppressed]));
    }

    [Fact]
    public async Task DryRunSuppressTest_TransactionDbCommandAsync()
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = true;
        using (var transaction = await DbContext.Database.BeginTransactionAsync())
        {
            await DbContext.Database.ExecuteSqlRawAsync("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test')");
            await transaction.CommitAsync();
        }

        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommissionSuppressed]));
    }

    [Fact]
    public void DryRunSuppressAllTest()
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
    public async Task DryRunSuppressAllTestAsync()
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
    public void DryRunSuppressTest_MultipleTransactions()
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = true;
        using (var transaction1 = DbContext.Database.BeginTransaction())
        {
            var entity = new Entity1 { Id = 1, Name = "Test" };
            DbContext.Add(entity);
            DbContext.SaveChanges();
            transaction1.Commit();
        }

        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommissionSuppressed]));
        Assert.False(DbContext.Set<Entity1>().Any());
        Assert.Single(DbContext.ChangeTracker.Entries());
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Added));

        using var transaction2 = DbContext.Database.BeginTransaction();
        DbContext.Database.ExecuteSqlRaw("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test')");
        transaction2.Commit();

        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommissionSuppressed]));
    }

    [Fact]
    public async Task DryRunSuppressTest_MultipleTransactionsAsync()
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = true;
        using (var transaction1 = await DbContext.Database.BeginTransactionAsync())
        {
            var entity = new Entity1 { Id = 1, Name = "Test" };
            await DbContext.AddAsync(entity);
            await DbContext.SaveChangesAsync();
            await transaction1.CommitAsync();
        }

        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommissionSuppressed]));
        Assert.False(await DbContext.Set<Entity1>().AnyAsync());
        Assert.Single(DbContext.ChangeTracker.Entries());
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Added));

        using (var transaction2 = await DbContext.Database.BeginTransactionAsync())
        {
            await DbContext.Database.ExecuteSqlRawAsync("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test')");
            await transaction2.CommitAsync();
        }

        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommissionSuppressed]));
    }
}
