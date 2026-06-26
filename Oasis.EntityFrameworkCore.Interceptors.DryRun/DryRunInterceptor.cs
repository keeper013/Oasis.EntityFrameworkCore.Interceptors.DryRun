namespace Oasis.EntityFrameworkCore.Interceptors.DryRun;

using Microsoft.EntityFrameworkCore.Diagnostics;

public sealed class DryRunInterceptor : Oasis.EntityFrameworkCore.Interceptors.IInterceptor, IDryRunHandle
{
    private readonly DbTransactionInterceptor _dbTransactionInterceptor;
    private readonly DbCommandInterceptor _commandInterceptor;
    private readonly SaveChangesInterceptor _saveChangesInterceptor;
    private bool? _dryRun = null;

    public DryRunInterceptor()
    {
        _dbTransactionInterceptor = new DbTransactionInterceptor(this);
        _commandInterceptor = new DbCommandInterceptor(this, _dbTransactionInterceptor);
        _saveChangesInterceptor = new SaveChangesInterceptor(this, _dbTransactionInterceptor);
    }

    public event EventHandler<DbContextEventData>? OnDryRunSaveChangesSuppressed;

    public event EventHandler<DbContextEventData>? OnDryRunSavingChanges;

    public event EventHandler<DbContextEventData>? OnDryRunSavedChanges;

    public event EventHandler<CommandEventData>? OnDryRunCommandSuppressed;

    public event EventHandler<CommandEventData>? OnDryRunCommandExecution;

    public event EventHandler<TransactionEventData>? OnDryRunTransactionCommissionSuppressed;

    public event EventHandler<TransactionEventData>? OnDryRunTransactionCommitting;

    public event EventHandler<TransactionEventData>? OnDryRunTransactionCommitted;

    public event EventHandler<TransactionEndEventData>? OnDryRunTransactionStarted;

    public bool? DryRunValue => _dryRun;

    // This property must be set before executing any database operations, and can only be set once per database context instance.
    // Setting it to true will make all database operations no-op, and roll back any transaction at commit time, effectively making the entire database context a dry run.
    public bool DryRun
    {
        set
        {
            Common.ValidateIsDryRunSetOnce(_dryRun);

            _dryRun = value;
            _dbTransactionInterceptor.DryRun = value;
            _commandInterceptor.DryRun = value;
            _saveChangesInterceptor.DryRun = value;
        }
    }

    public IInterceptor[] Interceptors => [_dbTransactionInterceptor, _commandInterceptor, _saveChangesInterceptor];

    internal void RaiseOnDryRunSaveChangesSuppressed(DbContextEventData eventData)
    {
        OnDryRunSaveChangesSuppressed?.Invoke(_saveChangesInterceptor, eventData);
    }

    internal void RaiseOnDryRunSavingChanges(DbContextEventData eventData)
    {
        OnDryRunSavingChanges?.Invoke(_saveChangesInterceptor, eventData);
    }

    internal void RaiseOnDryRunSavedChanges(DbContextEventData eventData)
    {
        OnDryRunSavedChanges?.Invoke(_saveChangesInterceptor, eventData);
    }

    internal void RaiseOnDryRunCommandExecution(CommandEventData eventData)
    {
        OnDryRunCommandExecution?.Invoke(_commandInterceptor, eventData);
    }

    internal void RaiseOnDryRunCommandSuppressed(CommandEventData eventData)
    {
        OnDryRunCommandSuppressed?.Invoke(_commandInterceptor, eventData);
    }

    internal void RaiseOnDryRunTransactionCommissionSuppressed(TransactionEventData eventData)
    {
        OnDryRunTransactionCommissionSuppressed?.Invoke(_dbTransactionInterceptor, eventData);
    }

    internal void RaiseOnDryRunTransactionCommitting(TransactionEventData eventData)
    {
        OnDryRunTransactionCommitting?.Invoke(_dbTransactionInterceptor, eventData);
    }

    internal void RaiseOnDryRunTransactionCommitted(TransactionEventData eventData)
    {
        OnDryRunTransactionCommitted?.Invoke(_dbTransactionInterceptor, eventData);
    }

    internal void RaiseOnDryRunTransactionStarted(TransactionEndEventData eventData)
    {
        OnDryRunTransactionStarted?.Invoke(_dbTransactionInterceptor, eventData);
    }
}