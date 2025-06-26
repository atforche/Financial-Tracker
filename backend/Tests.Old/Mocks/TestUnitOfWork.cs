using Data;

namespace Tests.Old.Mocks;

/// <summary>
/// Class representing an atomic unit of work to be committed to the database for testing
/// </summary>
public class TestUnitOfWork(DatabaseContext? context = null)
{
    /// <inheritdoc/>
    public void SaveChanges() => context?.SaveChanges();
}