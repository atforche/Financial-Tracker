namespace Data;

/// <summary>
/// Class representing an atomic unit of work to be committed to the database
/// </summary>
public class UnitOfWork(DatabaseContext context)
{
    private readonly DatabaseContext _context = context;

    /// <inheritdoc/>
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}