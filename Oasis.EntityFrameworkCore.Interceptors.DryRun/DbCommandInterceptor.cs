namespace Oasis.EntityFrameworkCore.Interceptors.DryRun;

using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

internal sealed class DbCommandInterceptor(DryRunnableInterceptorAggregator aggregator, IDryRunTransactionMonitor monitor)
    : Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
{
    private readonly DryRunnableInterceptorAggregator _aggregator = aggregator;
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

    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        var dryRun = DryRun;
        if (result.HasResult)
        {
            if (dryRun && !_monitor.IsTransactionActive)
            {
                _aggregator.RaiseOnDryRunCommandSuppressed(eventData);
            }

            return result;
        }

        if (dryRun && !_monitor.IsTransactionActive)
        {
            _aggregator.RaiseOnDryRunCommandExecution(eventData);
            return InterceptionResult<int>.SuppressWithResult(0);
        }

        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var dryRun = DryRun;
        if (result.HasResult)
        {
            if (dryRun && !_monitor.IsTransactionActive)
            {
                _aggregator.RaiseOnDryRunCommandSuppressed(eventData);
            }

            return new ValueTask<InterceptionResult<int>>(result);
        }

        if (dryRun && !_monitor.IsTransactionActive)
        {
            _aggregator.RaiseOnDryRunCommandExecution(eventData);
            return new ValueTask<InterceptionResult<int>>(InterceptionResult<int>.SuppressWithResult(0));
        }

        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }
}