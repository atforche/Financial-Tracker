namespace Tests.Validators;

/// <summary>
/// Base class of all validators that validate an Entity
/// </summary>
internal abstract class EntityValidatorBase<TEntity, TState>
{
    /// <summary>
    /// Validates the Entity against the provided expected state
    /// </summary>
    /// <param name="entityToValidate">Entity to validate</param>
    /// <param name="expectedState">Expected state for the Entity</param>
    public abstract void Validate(TEntity entityToValidate, TState expectedState);

    /// <summary>
    /// Validates the list of Entities against the list of provided expected state
    /// </summary>
    /// <param name="entitiesToValidate">Entities to validate</param>
    /// <param name="expectedStates">Expected states for the entities</param>
    public void Validate(IEnumerable<TEntity> entitiesToValidate, IEnumerable<TState> expectedStates)
    {
        entitiesToValidate = entitiesToValidate.ToList();
        expectedStates = expectedStates.ToList();
        Assert.Equal(expectedStates.Count(), entitiesToValidate.Count());
        foreach ((TEntity entity, TState expectedState) in entitiesToValidate.Zip(expectedStates))
        {
            Validate(entity, expectedState);
        }
    }
}