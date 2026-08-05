using Microsoft.EntityFrameworkCore;

namespace Data;

/// <summary>
/// Class representing an atomic unit of work to be committed to the database
/// </summary>
public class UnitOfWork(DatabaseContext context)
{
    private readonly DatabaseContext _context = context;

    /// <inheritdoc/>
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    /// <summary>
    /// Begins a database transaction for lifecycle operations that must be serialized.
    /// </summary>
    /// <param name="isolationLevel">Isolation level requested from the provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The active database transaction.</returns>
    public Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(
        System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.Serializable,
        CancellationToken cancellationToken = default) =>
        _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
}