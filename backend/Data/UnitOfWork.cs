namespace Data;

/// <summary>
/// Interface representing an atomic unit of work to be committed to the database
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all the changes associated with this unit of work to the database
    /// </summary>
    Task SaveChangesAsync();
}

/// <summary>
/// Class representing an atomic unit of work to be committed to the database
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DatabaseContext _context;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="context">Context to use to connect to the database</param>
    public UnitOfWork(DatabaseContext context) => _context = context;

    /// <inheritdoc/>
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}