namespace Tests.Validators;

/// <summary>
/// Base class of all validators that validate a collection of Entities
/// </summary>
internal abstract class EntityValidatorBase<TEntity, TState, TComparer>(IEnumerable<TEntity> entitiesToValidate)
    where TComparer : IComparer<TEntity>, IComparer<TState>, new()
{
    /// <summary>
    /// Entities to be validated by this Entity Validator
    /// </summary>
    protected List<TEntity> EntitiesToValidate { get; } = entitiesToValidate.ToList();

    /// <summary>
    /// Validates the Entity against the provided expected state
    /// </summary>
    /// <param name="expectedState">Expected state for the Entity</param>
    public void Validate(TState expectedState) => Validate([expectedState]);

    /// <summary>
    /// Validates the Entities against the provided expected states
    /// </summary>
    /// <param name="expectedStates">Expected states for the Entities</param>
    public void Validate(List<TState> expectedStates)
    {
        Assert.Equal(expectedStates.Count, EntitiesToValidate.Count);
        IEnumerable<TState> orderedStates = expectedStates.OrderBy(state => state, new TComparer());
        IEnumerable<TEntity> orderedEntities = EntitiesToValidate.OrderBy(entity => entity, new TComparer());
        foreach ((TState expectedState, TEntity entity) in orderedStates.Zip(orderedEntities))
        {
            ValidatePrivate(expectedState, entity);
        }
    }

    /// <summary>
    /// Validates that the provided expected state matches the provided actual state
    /// </summary>
    /// <param name="expectedState">Expected state of the entity</param>
    /// <param name="entity">Entity to validate</param>
    protected abstract void ValidatePrivate(TState expectedState, TEntity entity);
}