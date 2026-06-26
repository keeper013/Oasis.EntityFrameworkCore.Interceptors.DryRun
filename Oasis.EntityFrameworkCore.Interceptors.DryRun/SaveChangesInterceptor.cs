namespace Oasis.EntityFrameworkCore.Interceptors.DryRun;

using Microsoft.EntityFrameworkCore.Diagnostics;

internal sealed class SaveChangesInterceptor(DryRunInterceptor interceptor, IDryRunTransactionMonitor monitor)
    : Microsoft.EntityFrameworkCore.Diagnostics.SaveChangesInterceptor
{
    private readonly DryRunInterceptor _interceptor = interceptor;
    private readonly IDryRunTransactionMonitor _monitor = monitor;
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

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var dryRun = DryRun;
        if (result.HasResult)
        {
            if (dryRun && !_monitor.IsTransactionActive)
            {
                _interceptor.RaiseOnDryRunSaveChangesSuppressed(eventData);
            }

            return result;
        }

        if (dryRun && !_monitor.IsTransactionActive)
        {
            _interceptor.RaiseOnDryRunSavingChanges(eventData);

            // pretend in memory that the transaction is done, but in fact the data is not persisted.
            eventData.Context?.ChangeTracker.AcceptAllChanges();
            _interceptor.RaiseOnDryRunSavedChanges(eventData);
            return InterceptionResult<int>.SuppressWithResult(0);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var dryRun = DryRun;
        if (result.HasResult)
        {
            if (dryRun && !_monitor.IsTransactionActive)
            {
                _interceptor.RaiseOnDryRunSaveChangesSuppressed(eventData);
            }

            return new ValueTask<InterceptionResult<int>>(result);
        }

        if (dryRun && !_monitor.IsTransactionActive)
        {
            _interceptor.RaiseOnDryRunSavingChanges(eventData);

            // pretend in memory that the transaction is done, but in fact the data is not persisted.
            eventData.Context!.ChangeTracker.AcceptAllChanges();
            _interceptor.RaiseOnDryRunSavedChanges(eventData);
            return new ValueTask<InterceptionResult<int>>(InterceptionResult<int>.SuppressWithResult(0));
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}