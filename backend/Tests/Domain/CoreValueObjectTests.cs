using Domain;

namespace Tests.Domain;

/// <summary>
/// Covers the boundary and ordering rules shared by Domain value objects.
/// </summary>
public sealed class CoreValueObjectTests
{
    /// <summary>
    /// Applies inclusive and exclusive endpoints consistently when enumerating and testing ranges.
    /// </summary>
    [Fact]
    public void DateRangeHonorsEndpointTypesAndRejectsEmptyRanges()
    {
        DateOnly start = new(2026, 7, 1);
        DateOnly end = new(2026, 7, 3);
        var range = new DateRange(start, end, EndpointType.Exclusive, EndpointType.Inclusive);

        Assert.Equal([new DateOnly(2026, 7, 2), end], range.GetInclusiveDates());
        Assert.False(range.IsWithinStartDate(start));
        Assert.True(range.IsWithinEndDate(end));
        Assert.True(range.IsInRange(new DateOnly(2026, 7, 2)));
        Assert.False(range.IsInRange(start));
        _ = Assert.Throws<InvalidOperationException>(() => new DateRange(start, start, EndpointType.Exclusive));
    }

    /// <summary>
    /// Compares IDs by value, including null and incompatible-object cases.
    /// </summary>
    [Fact]
    public void EntityIdSupportsOrderingAndValidatesValues()
    {
        var first = new EntityId(new Guid("00000000-0000-0000-0000-000000000001"));
        var second = new EntityId(new Guid("00000000-0000-0000-0000-000000000002"));
        EntityId? none = null;

        Assert.True(first < second);
        Assert.True(first <= second);
        Assert.True(second > first);
        Assert.True(second >= first);
        Assert.True(none! < first);
        Assert.True(none! <= first);
        Assert.True(second > none!);
        Assert.True(second >= none!);
        Assert.Equal(1, first.CompareTo(null));
        _ = Assert.Throws<ArgumentException>(() => first.CompareTo("not an ID"));
        _ = Assert.Throws<InvalidOperationException>(() => new EntityId(Guid.Empty));
    }
}