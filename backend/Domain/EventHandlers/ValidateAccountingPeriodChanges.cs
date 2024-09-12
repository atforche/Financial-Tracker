using Domain.Entities;
using Domain.Events;
using Domain.Repositories;

namespace Domain.EventHandlers;

/// <summary>
/// Handler of <see cref="AccountingPeriodChangedEvent"/> that validates changes to an Accounting Period.
/// </summary>
public class ValidateAccountingPeriodChanges : IDomainEventHandler<AccountingPeriodChangedEvent>
{
    private readonly IAccountingPeriodRepository _repository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="repository">Repository of Accounting Periods</param>
    public ValidateAccountingPeriodChanges(IAccountingPeriodRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc/>
    public Task Handle(AccountingPeriodChangedEvent notification, CancellationToken cancellationToken)
    {
        ValidateNewAccountingPeriod(notification);
        ValidateAccountingPeriodDelete(notification);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates an Accounting Period that was newly added
    /// </summary>
    /// <param name="notification">Accounting period changed event</param>
    private void ValidateNewAccountingPeriod(AccountingPeriodChangedEvent notification)
    {
        if (notification.Action != AccountingPeriodChangedAction.Added)
        {
            return;
        }
        List<AccountingPeriod> accountingPeriods = _repository.FindAll().ToList();
        if (accountingPeriods.Count > 0)
        {
            // Validate that there are no duplicate accounting periods
            AccountingPeriod? duplicatePeriod = accountingPeriods
                .SingleOrDefault(period => period.Year == notification.Year && period.Month == notification.Month);
            if (duplicatePeriod != null)
            {
                throw new InvalidOperationException();
            }

            // Validate that accounting periods can only be added after existing accounting periods
            DateTime previousMonth = new DateTime(notification.Year, notification.Month, 1).AddMonths(-1);
            AccountingPeriod? previousAccountingPeriod = accountingPeriods
                .SingleOrDefault(period => period.Year == previousMonth.Year && period.Month == previousMonth.Month);
            if (previousAccountingPeriod == null)
            {
                throw new InvalidOperationException();
            }

            // Validate that there cannot be multiple open accounting periods
            AccountingPeriod? alreadyOpenedPeriod = _repository.FindOpenPeriod();
            if (alreadyOpenedPeriod != null)
            {
                throw new InvalidOperationException();
            }
        }
    }

    /// <summary>
    /// Validates an Accounting Period that was deleted
    /// </summary>
    /// <param name="notification">Accounting period changed event</param>
    private void ValidateAccountingPeriodDelete(AccountingPeriodChangedEvent notification)
    {
        if (notification.Action != AccountingPeriodChangedAction.Deleted)
        {
            return;
        }
        List<AccountingPeriod> accountingPeriods = _repository.FindAll().ToList();
        if (accountingPeriods.Count > 0)
        {
            // Validate that that only the last accounting period can be deleted
            DateTime nextMonth = new DateTime(notification.Year, notification.Month, 1).AddMonths(1);
            AccountingPeriod? nextAccountingPeriod = accountingPeriods
                .SingleOrDefault(period => period.Year == nextMonth.Year && period.Month == nextMonth.Month);
            if (nextAccountingPeriod != null)
            {
                throw new InvalidOperationException();
            }
        }
    }
}