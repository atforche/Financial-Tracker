namespace Models;

/// <summary>
/// Model used to represent a collection of items, along with the total count of items in the collection.
/// </summary>
public class CollectionModel<T>
{
    /// <summary>
    /// The collection of items.
    /// </summary>
    public required IReadOnlyCollection<T> Items { get; init; }

    /// <summary>
    /// The total count of items in the collection, which may be greater than the number of items in the Items property if pagination is being used.
    /// </summary>
    public required int TotalCount { get; init; }
}