using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Tests.Infrastructure;

/// <summary>
/// Counts database commands initialized by one isolated test application instance.
/// </summary>
internal sealed class ReadQueryCounter : DbCommandInterceptor
{
    private int _count;

    /// <summary>
    /// Gets the command count since the last reset.
    /// </summary>
    public int Count => Volatile.Read(ref _count);

    /// <summary>
    /// Resets the count after fixture setup and before a measured request group.
    /// </summary>
    public void Reset() => _ = Interlocked.Exchange(ref _count, 0);

    /// <inheritdoc/>
    public override DbCommand CommandInitialized(CommandEndEventData eventData, DbCommand result)
    {
        _ = Interlocked.Increment(ref _count);
        return result;
    }
}
