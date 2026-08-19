namespace Domain;

/// <summary>
/// A page of query results and the total number of matching items.
/// </summary>
public sealed record QueryPage<T>(IReadOnlyCollection<T> Items, int TotalCount);
