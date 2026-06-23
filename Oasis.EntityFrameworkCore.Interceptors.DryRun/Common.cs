namespace Oasis.EntityFrameworkCore.Interceptors.DryRun;

internal static class Common
{
    public const string DryRunMustBeSet = "DryRun must be set before executing any database operations.";
    public const string DryRunCanOnlyBeSetOnce = "DryRun can only be set once per database context instance.";

    public static void ValidateIsDryRunSetOnce(bool? isDryRun)
    {
        if (isDryRun is not null)
        {
            throw new InvalidOperationException(DryRunCanOnlyBeSetOnce);
        }
    }

    public static void ValidateIsDryRunSet(bool? isDryRun)
    {
        if (isDryRun is null)
        {
            throw new InvalidOperationException(DryRunMustBeSet);
        }
    }
}