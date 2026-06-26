namespace Oasis.EntityFrameworkCore.Interceptors.DryRun.Tests;

using Microsoft.EntityFrameworkCore;

public sealed class DryRunEventTest : DryRunEventTestBase
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DryRunEventTest_ExecuteCommand(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        DbContext.Database.ExecuteSqlRaw("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test')");
        Assert.True(TraceFlags.Match(dryRun, [DryRunEventTrace.CommandExecution]));
        Assert.NotEqual(dryRun, DbContext.Set<Entity1>().Any());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DryRunEventTest_ExecuteCommandAsync(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        await DbContext.Database.ExecuteSqlRawAsync("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test')");
        Assert.True(TraceFlags.Match(dryRun, [DryRunEventTrace.CommandExecution]));
        Assert.NotEqual(dryRun, DbContext.Set<Entity1>().Any());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DryRunEventTest_SaveChanges(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        var entity = new Entity1 { Id = 1, Name = "Test" };
        DbContext.Add(entity);
        DbContext.SaveChanges();
        Assert.True(TraceFlags.Match(dryRun, [DryRunEventTrace.SavingChanges, DryRunEventTrace.SavedChanges]));
        Assert.NotEqual(dryRun, DbContext.Set<Entity1>().Any());
        Assert.Single(DbContext.ChangeTracker.Entries());
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DryRunEventTest_SaveChangesAsync(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        var entity = new Entity1 { Id = 1, Name = "Test" };
        DbContext.Add(entity);
        await DbContext.SaveChangesAsync();
        Assert.True(TraceFlags.Match(dryRun, [DryRunEventTrace.SavingChanges, DryRunEventTrace.SavedChanges]));
        Assert.NotEqual(dryRun, await DbContext.Set<Entity1>().AnyAsync());
        Assert.Single(DbContext.ChangeTracker.Entries());
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DryRunEventTest_TransactionSaveChanges(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        using (var transaction = DbContext.Database.BeginTransaction())
        {
            var entity = new Entity1 { Id = 1, Name = "Test" };
            DbContext.Add(entity);
            DbContext.SaveChanges();
            transaction.Commit();
        }

        Assert.True(TraceFlags.Match(dryRun, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommitting, DryRunEventTrace.TransactionCommitted]));
        Assert.NotEqual(dryRun, DbContext.Set<Entity1>().Any());
        Assert.Single(DbContext.ChangeTracker.Entries());
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DryRunEventTest_TransactionExecuteCommand(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        using (var transaction = DbContext.Database.BeginTransaction())
        {
            DbContext.Database.ExecuteSqlRaw("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test1')");
            transaction.Commit();
        }

        Assert.True(TraceFlags.Match(dryRun, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommitting, DryRunEventTrace.TransactionCommitted]));
        Assert.NotEqual(dryRun, DbContext.Set<Entity1>().Any());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DryRunEventTest_TransactionSaveChangesAsync(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        using (var transaction = await DbContext.Database.BeginTransactionAsync())
        {
            var entity = new Entity1 { Id = 2, Name = "Test2" };
            await DbContext.AddAsync(entity);
            await DbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        Assert.True(TraceFlags.Match(dryRun, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommitting, DryRunEventTrace.TransactionCommitted]));
        Assert.NotEqual(dryRun, await DbContext.Set<Entity1>().AnyAsync());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DryRunEventTest_TransactionExecuteCommandAsync(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        using (var transaction = await DbContext.Database.BeginTransactionAsync())
        {
            await DbContext.Database.ExecuteSqlRawAsync("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test1')");
            await transaction.CommitAsync();
        }

        Assert.True(TraceFlags.Match(dryRun, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommitting, DryRunEventTrace.TransactionCommitted]));
        Assert.NotEqual(dryRun, DbContext.Set<Entity1>().Any());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DryRunEventTest_CommandAndSaveChanges(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        DbContext.Database.ExecuteSqlRaw("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test1')");
        var entity = new Entity1 { Id = 2, Name = "Test2" };
        DbContext.Add(entity);
        DbContext.SaveChanges();
        Assert.True(TraceFlags.Match(dryRun, [DryRunEventTrace.CommandExecution, DryRunEventTrace.SavingChanges, DryRunEventTrace.SavedChanges]));
        Assert.NotEqual(dryRun, DbContext.Set<Entity1>().Any());
        Assert.Single(DbContext.ChangeTracker.Entries());
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DryRunEventTest_CommandAndSaveChangesAsync(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        await DbContext.Database.ExecuteSqlRawAsync("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test1')");
        var entity = new Entity1 { Id = 2, Name = "Test2" };
        await DbContext.AddAsync(entity);
        await DbContext.SaveChangesAsync();
        Assert.True(TraceFlags.Match(dryRun, [DryRunEventTrace.CommandExecution, DryRunEventTrace.SavingChanges, DryRunEventTrace.SavedChanges]));
        Assert.NotEqual(dryRun, DbContext.Set<Entity1>().Any());
        Assert.Single(DbContext.ChangeTracker.Entries());
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DryRunEventTest_MultileTransactions(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        using (var transaction1 = DbContext.Database.BeginTransaction())
        {
            DbContext.Database.ExecuteSqlRaw("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test1')");
            transaction1.Commit();
        }
            
        Assert.True(TraceFlags.Match(dryRun, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommitting, DryRunEventTrace.TransactionCommitted]));
        Assert.NotEqual(dryRun, DbContext.Set<Entity1>().Any());

        using (var transaction2 = DbContext.Database.BeginTransaction())
        {
            var entity = new Entity1 { Id = 2, Name = "Test2" };
            DbContext.Add(entity);
            DbContext.SaveChanges();
            transaction2.Commit();
        }
            
        Assert.True(TraceFlags.Match(dryRun, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommitting, DryRunEventTrace.TransactionCommitted]));
        Assert.NotEqual(dryRun, DbContext.Set<Entity1>().Any());
        Assert.Single(DbContext.ChangeTracker.Entries());
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DryRunEventTest_MultileTransactionsAsync(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        using (var transaction1 = await DbContext.Database.BeginTransactionAsync())
        {
            await DbContext.Database.ExecuteSqlRawAsync("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test1')");
            await transaction1.CommitAsync();
            Assert.True(TraceFlags.Match(dryRun, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommitting, DryRunEventTrace.TransactionCommitted]));
            Assert.NotEqual(dryRun, DbContext.Set<Entity1>().Any());
        }

        using (var transaction2 = await DbContext.Database.BeginTransactionAsync())
        {
            var entity = new Entity1 { Id = 2, Name = "Test2" };
            await DbContext.AddAsync(entity);
            await DbContext.SaveChangesAsync();
            await transaction2.CommitAsync();
        }
            
        Assert.True(TraceFlags.Match(dryRun, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommitting, DryRunEventTrace.TransactionCommitted]));
        Assert.NotEqual(dryRun, await DbContext.Set<Entity1>().AnyAsync());
    }
}
