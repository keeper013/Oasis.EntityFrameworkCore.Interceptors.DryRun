namespace Oasis.EntityFrameworkCore.Interceptors.DryRun.Tests;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Oasis.EntityFrameworkCore.Interceptors.DryRun;

public abstract class TestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    private DbContext<DryRunnableInterceptorAggregator, IDryRunnable>? _dbContext;
    private bool _isDisposed;

    protected TestBase()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
    }

    protected DbContext<DryRunnableInterceptorAggregator, IDryRunnable> DbContext
    {
        get
        {
            if (_dbContext == null)
            {
                _dbContext = InitializeDatabaseContext();
                _dbContext.Database.EnsureCreated();
                ConfigureDbContext(_dbContext);
            }
            
            return _dbContext;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void ConfigureDbContext(DbContext<DryRunnableInterceptorAggregator, IDryRunnable> context)
    {
    }

    protected virtual DbContextOptionsBuilder<DbContext> ConfigureDbContextOptionsBuilder(DbContextOptionsBuilder<DbContext> optionsBuilder)
    {
        return optionsBuilder;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            // free managed resources
            _connection.Close();
        }

        _isDisposed = true;
    }

    private DbContext<DryRunnableInterceptorAggregator, IDryRunnable> InitializeDatabaseContext()
    {
        var initializerOptions = new DbContextOptionsBuilder<InitializerDbContext>()
            .UseSqlite(_connection)
            .Options;
        InitializerDbContext initializerDbContext = new(initializerOptions);
        initializerDbContext.Database.EnsureCreated();
        var optionsBuilder = new DbContextOptionsBuilder<DbContext>()
            .UseSqlite(_connection);
        optionsBuilder = ConfigureDbContextOptionsBuilder(optionsBuilder);
        return new DbContext(new DryRunnableInterceptorAggregator(), optionsBuilder.Options);
    }
}
