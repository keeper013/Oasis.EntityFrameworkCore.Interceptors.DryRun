namespace Oasis.EntityFrameworkCore.Interceptors.DryRun;

using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

internal interface IDryRunTransactionMonitor
{
    /// <summary>
    /// Gets a value indicating whether a dry run transaction is active.
    /// for db commands and save changes, if a transaction is active, the operation doesn't need to be suppressed, as the transaction will be rolled back at commit time.
    /// </summary>
    public bool IsTransactionActive { get; }
}

internal sealed class DbTransactionInterceptor(DryRunnableInterceptorAggregator aggregator)
    : Microsoft.EntityFrameworkCore.Diagnostics.DbTransactionInterceptor,
    IDryRunTransactionMonitor
{
    // As DbContext isn't supposed to run parally, only 1 transaction can run at 1 time,
    // so only storing the out most transaction id is the right approach for rolling back for dry run commission
    private readonly DryRunnableInterceptorAggregator _aggregator = aggregator;
    private Guid? _transactionId = null;
    private bool? _dryRun = null;

    public bool? DryRunValue => _dryRun;

    public bool DryRun
    {
        private get
        {
            Common.ValidateIsDryRunSet(_dryRun);
            return _dryRun!.Value;
        }

        set
        {
            Common.ValidateIsDryRunSetOnce(_dryRun);
            _dryRun = value;
        }
    }

    public bool IsTransactionActive => _transactionId != null;

    public override DbTransaction TransactionStarted(DbConnection connection, TransactionEndEventData eventData, DbTransaction result)
    {
        if (DryRun && _transactionId == null)
        {
            _transactionId = eventData.TransactionId;
            _aggregator.RaiseOnDryRunTransactionStarted(eventData);
        }

        return base.TransactionStarted(connection, eventData, result);
    }

    public override ValueTask<DbTransaction> TransactionStartedAsync(DbConnection connection, TransactionEndEventData eventData, DbTransaction result, CancellationToken cancellationToken = default)
    {
        if (DryRun && _transactionId == null)
        {
            _transactionId = eventData.TransactionId;
            _aggregator.RaiseOnDryRunTransactionStarted(eventData);
        }

        return base.TransactionStartedAsync(connection, eventData, result, cancellationToken);
    }

    public override InterceptionResult TransactionCommitting(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result)
    {
        var dryRun = DryRun && eventData.TransactionId == _transactionId;
        if (result.IsSuppressed)
        {
            if (dryRun)
            {
                _transactionId = null;
                if (transaction.Connection != null)
                {
                    // can't leave the transaction open if nothing has been done to it by earlier suppress
                    transaction.Rollback();
                }

                _aggregator.RaiseOnDryRunTransactionCommissionSuppressed(eventData);
            }

            return result;
        }

        if (dryRun)
        {
            // roll back out most transaction
            _aggregator.RaiseOnDryRunTransactionCommitting(eventData);
            _transactionId = null;
            transaction.Rollback();
            _aggregator.RaiseOnDryRunTransactionCommitted(eventData);
            return InterceptionResult.Suppress();
        }

        return base.TransactionCommitting(transaction, eventData, result);
    }

    public override async ValueTask<InterceptionResult> TransactionCommittingAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
    {
        var dryRun = DryRun && eventData.TransactionId == _transactionId;
        if (result.IsSuppressed)
        {
            if (dryRun)
            {
                _transactionId = null;
                if (transaction.Connection != null)
                {
                    // can't leave the transaction open if nothing has been done to it by earlier suppress
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                }
                
                _aggregator.RaiseOnDryRunTransactionCommissionSuppressed(eventData);
            }

            return result;
        }

        if (dryRun)
        {
            // roll back out most transaction
            _aggregator.RaiseOnDryRunTransactionCommitting(eventData);
            _transactionId = null;
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            _aggregator.RaiseOnDryRunTransactionCommitted(eventData);
            return InterceptionResult.Suppress();
        }

        return await base.TransactionCommittingAsync(transaction, eventData, result, cancellationToken);
    }
}