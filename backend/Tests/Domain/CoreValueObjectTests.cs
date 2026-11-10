using Domain;

namespace Tests.Domain;

/// <summary>
/// Covers the boundary and ordering rules shared by Domain value objects.
/// </summary>
public sealed class CoreValueObjectTests
{
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
        _ = Assert.Throws<EmptyEntityIdException>(() => new EntityId(Guid.Empty));
    }
}
