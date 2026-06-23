namespace Oasis.EntityFrameworkCore.Interceptors.DryRun.Tests;

using Oasis.EntityFrameworkCore.Interceptors.DryRun;

[Flags]
public enum DryRunEventTrace
{
    None = 0,
    SaveChangesSuppressed = 1,
    SavingChanges = 2,
    SavedChanges = 4,
    CommandSuppressed = 8,
    CommandExecution = 16,
    TransactionCommissionSuppressed = 32,
    TransactionCommitting = 64,
    TransactionCommitted = 128,
    TransactionStarted = 256,
}

internal static class DryRunEventTraceExtensions
{
    private static readonly IReadOnlyList<DryRunEventTrace> _validDryRunEvents = [.. Enum.GetValues<DryRunEventTrace>().Except([DryRunEventTrace.None])];

    public static bool Match(this DryRunEventTrace actual, bool dryRun, params DryRunEventTrace[] expected)
    {
        var expectedSet = new HashSet<DryRunEventTrace>(expected);
        foreach (var trace in _validDryRunEvents)
        {
            if (dryRun && expectedSet.Contains(trace))
            {
                if ((actual & trace) != trace)
                {
                    return false;
                }
            }
            else
            {
                if ((actual & trace) != 0)
                {
                    return false;
                }
            }
        }

        return true;
    }
}

public abstract class DryRunEventTestBase : TestBase
{
    protected DryRunEventTrace TraceFlags { get; private set; }

    protected override void ConfigureDbContext(DbContext<DryRunnableInterceptorAggregator, IDryRunnable> context)
    {
        var handle = context.GetHandle<IDryRunnable>();

        handle.OnDryRunSaveChangesSuppressed += (sender, eventData) =>
        {
            TraceFlags |= DryRunEventTrace.SaveChangesSuppressed;
        };

        handle.OnDryRunSavingChanges += (sender, eventData) =>
        {
            TraceFlags |= DryRunEventTrace.SavingChanges;
        };

        handle.OnDryRunSavedChanges += (sender, eventData) =>
        {
            TraceFlags |= DryRunEventTrace.SavedChanges;
        };

        handle.OnDryRunCommandSuppressed += (sender, eventData) =>
        {
            TraceFlags |= DryRunEventTrace.CommandSuppressed;
        };

        handle.OnDryRunCommandExecution += (sender, eventData) =>
        {
            TraceFlags |= DryRunEventTrace.CommandExecution;
        };

        handle.OnDryRunTransactionCommissionSuppressed += (sender, eventData) =>
        {
            TraceFlags |= DryRunEventTrace.TransactionCommissionSuppressed;
        };

        handle.OnDryRunTransactionCommitting += (sender, eventData) =>
        {
            TraceFlags |= DryRunEventTrace.TransactionCommitting;
        };

        handle.OnDryRunTransactionCommitted += (sender, eventData) =>
        {
            TraceFlags |= DryRunEventTrace.TransactionCommitted;
        };

        handle.OnDryRunTransactionStarted += (sender, eventData) =>
        {
            TraceFlags |= DryRunEventTrace.TransactionStarted;
        };

        base.ConfigureDbContext(context);
    }
}
