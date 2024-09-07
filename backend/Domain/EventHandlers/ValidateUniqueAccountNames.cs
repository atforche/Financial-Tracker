using Domain.Events;
using Domain.Repositories;

namespace Domain.EventHandlers;

/// <summary>
/// Handler of <see cref="AccountRenamedDomainEvent"/> that validates that Account names are unique
/// </summary>
public class ValidateUniqueAccountNames : IDomainEventHandler<AccountRenamedDomainEvent>
{
    private readonly IAccountRepository _repository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="repository">Repository of Accounts</param>
    public ValidateUniqueAccountNames(IAccountRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc/>
    public Task Handle(AccountRenamedDomainEvent notification, CancellationToken cancellationToken)
    {
        if (_repository.FindByNameOrNull(notification.NewName) != null)
        {
            throw new InvalidOperationException();
        }
        return Task.CompletedTask;
    }
}