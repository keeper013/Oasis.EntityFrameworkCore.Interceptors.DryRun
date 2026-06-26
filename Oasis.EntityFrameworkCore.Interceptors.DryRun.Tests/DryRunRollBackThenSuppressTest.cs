namespace Oasis.EntityFrameworkCore.Interceptors.DryRun.Tests;

using Microsoft.EntityFrameworkCore;

public sealed class DryRunRollBackThenSuppressTest : DryRunRollBackThenSupppressTestBase
{
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
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));

        using (var transaction2 = DbContext.Database.BeginTransaction())
        {
            DbContext.Database.ExecuteSqlRaw("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test')");
            transaction2.Commit();
        }

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
        Assert.True(DbContext.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));

        using (var transaction2 = await DbContext.Database.BeginTransactionAsync())
        {
            await DbContext.Database.ExecuteSqlRawAsync("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test')");
            await transaction2.CommitAsync();
        }

        Assert.True(TraceFlags.Match(true, [DryRunEventTrace.TransactionStarted, DryRunEventTrace.TransactionCommissionSuppressed]));
    }
}
