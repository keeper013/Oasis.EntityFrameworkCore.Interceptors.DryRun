namespace Oasis.EntityFrameworkCore.Interceptors.DryRun.Tests;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

internal sealed class SuppressDbCommandInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        return InterceptionResult<int>.SuppressWithResult(0);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        return new ValueTask<InterceptionResult<int>>(InterceptionResult<int>.SuppressWithResult(0));
    }
}

internal sealed class SuppressSaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        return InterceptionResult<int>.SuppressWithResult(0);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        return new ValueTask<InterceptionResult<int>>(InterceptionResult<int>.SuppressWithResult(0));
    }
}

internal sealed class SuppressTransactionInterceptor : DbTransactionInterceptor
{
    public override InterceptionResult TransactionCommitting(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result)
    {
        return InterceptionResult.Suppress();
    }

    public override ValueTask<InterceptionResult> TransactionCommittingAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
    {
        return new ValueTask<InterceptionResult>(InterceptionResult.Suppress());
    }
}

internal sealed class RollBackThenSuppressTransactionInterceptor : DbTransactionInterceptor
{
    public override InterceptionResult TransactionCommitting(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result)
    {
        transaction.Rollback();
        return InterceptionResult.Suppress();
    }

    public override async ValueTask<InterceptionResult> TransactionCommittingAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
    {
        await transaction.RollbackAsync();
        return await new ValueTask<InterceptionResult>(InterceptionResult.Suppress());
    }
}

public abstract class DryRunSuppressAllTestBase : DryRunEventTestBase
{
    protected override DbContextOptionsBuilder<DbContext> ConfigureDbContextOptionsBuilder(DbContextOptionsBuilder<DbContext> optionsBuilder)
    {
        return optionsBuilder.AddInterceptors(new SuppressTransactionInterceptor(), new SuppressDbCommandInterceptor(), new SuppressSaveChangesInterceptor());
    }
}

public abstract class DryRunSuppressTransactionSaveChangesTestBase : DryRunEventTestBase
{
    protected override DbContextOptionsBuilder<DbContext> ConfigureDbContextOptionsBuilder(DbContextOptionsBuilder<DbContext> optionsBuilder)
    {
        return optionsBuilder.AddInterceptors(new SuppressTransactionInterceptor(), new SuppressSaveChangesInterceptor());
    }
}

public abstract class DryRunSuppressTransactionDbCommandTestBase : DryRunEventTestBase
{
    protected override DbContextOptionsBuilder<DbContext> ConfigureDbContextOptionsBuilder(DbContextOptionsBuilder<DbContext> optionsBuilder)
    {
        return optionsBuilder.AddInterceptors(new SuppressTransactionInterceptor(), new SuppressDbCommandInterceptor());
    }
}

public abstract class DryRunSuppressSaveChangesDbCommandTestBase : DryRunEventTestBase
{
    protected override DbContextOptionsBuilder<DbContext> ConfigureDbContextOptionsBuilder(DbContextOptionsBuilder<DbContext> optionsBuilder)
    {
        return optionsBuilder.AddInterceptors(new SuppressDbCommandInterceptor(), new SuppressSaveChangesInterceptor());
    }
}

public abstract class DryRunRollBackThenSupppressTestBase : DryRunEventTestBase
{
    protected override DbContextOptionsBuilder<DbContext> ConfigureDbContextOptionsBuilder(DbContextOptionsBuilder<DbContext> optionsBuilder)
    {
        return optionsBuilder.AddInterceptors(new RollBackThenSuppressTransactionInterceptor());
    }
}