namespace Oasis.EntityFrameworkCore.Interceptors.DryRun.Tests;

using Microsoft.EntityFrameworkCore;
using Oasis.EntityFrameworkCore.Interceptors.DryRun;

public sealed class DryRunSetTest : TestBase
{
    [Fact]
    public void DryRunMustBeSetBeforeDatabaseOperations_ExecuteCommand_Negative()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => DbContext.Database.ExecuteSqlRaw("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test')"));
        Assert.Equal(Common.DryRunMustBeSet, exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DryRunMustBeSetBeforeDatabaseOperations_ExecuteCommand_Positive(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        DbContext.Database.ExecuteSqlRaw("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test')");
    }

    [Fact]
    public void DryRunMustBeSetBeforeDatabaseOperations_ExecuteRawQuery_Negative()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => DbContext.Database.ExecuteSqlRaw("SELECT * FROM Entity1 WHERE Id = 1"));
        Assert.Equal(Common.DryRunMustBeSet, exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DryRunMustBeSetBeforeDatabaseOperations_ExecuteRawQuery_Positive(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        DbContext.Database.ExecuteSqlRaw("SELECT * FROM Entity1 WHERE Id = 1");
    }

    [Fact]
    public void DryRunNotNeededToBeSetBeforeDatabaseOperations_ReadOnly()
    {
        DbContext.Database.SqlQuery<int>($"SELECT Id FROM Entity1").ToList();
    }

    [Fact]
    public async Task DryRunMustBeSetBeforeDatabaseOperations_ExecuteCommandAsync_Negative()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await DbContext.Database.ExecuteSqlRawAsync("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test')"));
        Assert.Equal(Common.DryRunMustBeSet, exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DryRunMustBeSetBeforeDatabaseOperations_ExecuteCommandAsync_Positive(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        await DbContext.Database.ExecuteSqlRawAsync("INSERT INTO Entity1 (Id, Name) VALUES (1, 'Test')");
    }

    [Fact]
    public async Task DryRunMustBeSetBeforeDatabaseOperations_ExecuteRawQueryAsync_Negative()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await DbContext.Database.ExecuteSqlRawAsync("SELECT * FROM Entity1 WHERE Id = 1"));
        Assert.Equal(Common.DryRunMustBeSet, exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DryRunMustBeSetBeforeDatabaseOperations_ExecuteRawQueryAsync_Positive(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        await DbContext.Database.ExecuteSqlRawAsync("SELECT * FROM Entity1 WHERE Id = 1");
    }

    [Fact]
    public async Task DryRunNotNeededToBeSetBeforeDatabaseOperations_ReadOnlyAsync()
    {
        await DbContext.Database.SqlQuery<int>($"SELECT Id FROM Entity1").ToListAsync();
    }

    [Fact]
    public void DryRunMustBeSetBeforeDatabaseOperations_SaveChanges_Negative()
    {
        var entity = new Entity1 { Id = 1, Name = "Test" };
        DbContext.Add(entity);
        var exception = Assert.Throws<InvalidOperationException>(() => DbContext.SaveChanges());
        Assert.Equal(Common.DryRunMustBeSet, exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DryRunMustBeSetBeforeDatabaseOperations_SaveChanges_Positive(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        var entity = new Entity1 { Id = 1, Name = "Test" };
        DbContext.Add(entity);
        DbContext.SaveChanges();
    }

    [Fact]
    public async Task DryRunMustBeSetBeforeDatabaseOperations_SaveChangesAsync_Negative()
    {
        var entity = new Entity1 { Id = 1, Name = "Test" };
        await DbContext.AddAsync(entity);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await DbContext.SaveChangesAsync());
        Assert.Equal(Common.DryRunMustBeSet, exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DryRunMustBeSetBeforeDatabaseOperations_SaveChangesAsync_Positive(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        var entity = new Entity1 { Id = 1, Name = "Test" };
        await DbContext.AddAsync(entity);
        await DbContext.SaveChangesAsync();
    }

    [Fact]
    public void DryRunNotNeededToBeSetBeforeDatabaseQuery_SelectFirstOrDefault()
    {
        DbContext.Set<Entity1>().FromSqlRaw("SELECT * FROM Entity1 WHERE Id = 1").FirstOrDefault();
    }

    [Fact]
    public async Task DryRunNotNeededToBeSetBeforeDatabaseQuery_SelectFirstOrDefaultAsync()
    {
        await DbContext.Set<Entity1>().FromSqlRaw("SELECT * FROM Entity1 WHERE Id = 1").FirstOrDefaultAsync();
    }

    [Fact]
    public void DryRunMustBeSetBeforeDatabaseOperations_TransactionStarts_Negative()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => DbContext.Database.BeginTransaction());
        Assert.Equal(Common.DryRunMustBeSet, exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DryRunMustBeSetBeforeDatabaseOperations_TransactionStarts_Positive(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        using var transaction = DbContext.Database.BeginTransaction();
        var entity = new Entity1 { Id = 1, Name = "Test" };
        DbContext.Add(entity);
        DbContext.SaveChanges();
        transaction.Commit();
    }

    [Fact]
    public async Task DryRunMustBeSetBeforeDatabaseOperations_TransactionStartsAsync_Negative()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await DbContext.Database.BeginTransactionAsync());
        Assert.Equal(Common.DryRunMustBeSet, exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DryRunMustBeSetBeforeDatabaseOperations_TransactionStartsAsync_Positive(bool dryRun)
    {
        DbContext.GetHandle<IDryRunHandle>().DryRun = dryRun;
        using var transaction = await DbContext.Database.BeginTransactionAsync();
        var entity = new Entity1 { Id = 1, Name = "Test" };
        await DbContext.AddAsync(entity);
        await DbContext.SaveChangesAsync();
        transaction.Commit();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void DryRunCanOnlyBeSetOnce(bool value1, bool value2)
    {
        var handle = DbContext.GetHandle<IDryRunHandle>();
        handle.DryRun = value1;
        var exception = Assert.Throws<InvalidOperationException>(() => handle.DryRun = value2);
        Assert.Equal(Common.DryRunCanOnlyBeSetOnce, exception.Message);
    }
}
