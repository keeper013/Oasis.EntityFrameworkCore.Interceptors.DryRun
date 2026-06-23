namespace Oasis.EntityFrameworkCore.Interceptors.DryRun;

using Microsoft.EntityFrameworkCore.Diagnostics;

public interface IDryRunnable
{
    public event EventHandler<DbContextEventData>? OnDryRunSaveChangesSuppressed;

    public event EventHandler<DbContextEventData>? OnDryRunSavingChanges;

    public event EventHandler<DbContextEventData>? OnDryRunSavedChanges;

    public event EventHandler<CommandEventData>? OnDryRunCommandSuppressed;

    public event EventHandler<CommandEventData>? OnDryRunCommandExecution;

    public event EventHandler<TransactionEventData>? OnDryRunTransactionCommissionSuppressed;

    public event EventHandler<TransactionEventData>? OnDryRunTransactionCommitting;

    public event EventHandler<TransactionEventData>? OnDryRunTransactionCommitted;

    public event EventHandler<TransactionEndEventData>? OnDryRunTransactionStarted;

    /// <summary>
    /// Sets a value indicating whether the configuration is for dry run.
    /// This property must be set before executing any database operations, and can only be set once per database context instance.
    /// </summary>
    public bool DryRun { set; }
}
