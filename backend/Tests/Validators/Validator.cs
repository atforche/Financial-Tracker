namespace Tests.Validators;

/// <summary>
/// Base class of all validators
/// </summary>
internal abstract class Validator<TObject, TState>
{
    /// <summary>
    /// Validates the object against the provided expected state
    /// </summary>
    /// <param name="objectToValidate">Object to validate</param>
    /// <param name="expectedState">Expected state for the object</param>
    public abstract void Validate(TObject objectToValidate, TState expectedState);

    /// <summary>
    /// Validates the list of objects against the list of expected states
    /// </summary>
    /// <param name="objectsToValidate">Objects to validate</param>
    /// <param name="expectedStates">Expected states for the objects</param>
    public void Validate(IEnumerable<TObject> objectsToValidate, IEnumerable<TState> expectedStates)
    {
        objectsToValidate = objectsToValidate.ToList();
        expectedStates = expectedStates.ToList();
        Assert.Equal(expectedStates.Count(), objectsToValidate.Count());
        foreach ((TObject entity, TState expectedState) in objectsToValidate.Zip(expectedStates))
        {
            Validate(entity, expectedState);
        }
    }
}