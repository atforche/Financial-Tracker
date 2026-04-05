using System.Diagnostics.CodeAnalysis;
using Data.Budgets;
using Domain.Budgets;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Budgets to Budget Models
/// </summary>
/// <param name="budgetRepository"></param>
public sealed class BudgetMapper(BudgetRepository budgetRepository)
{
    /// <summary>
    /// Attempts to map the provided ID to a Budget
    /// </summary>
    public bool TryToDomain(Guid budgetId, [NotNullWhen(true)] out Budget? budget) => budgetRepository.TryGetById(budgetId, out budget);
}