namespace Data.EntityModels;

/// <summary>
/// Interface representing a data model used to store an entity in the database
/// </summary>
public interface IEntityDataModel<T>
{
    /// <summary>
    /// Replaces the current model with the data for the provided model.
    /// Allows replacement while maintaining EF Core change tracking information
    /// </summary>
    /// <param name="newModel">New model to replace the current model with</param>
    void Replace(T newModel);
}