using Domain.Users;

namespace Data.Users;

/// <summary>
/// EF Core repository for user-management audit events.
/// </summary>
public sealed class UserAdministrationAuditEventRepository(DatabaseContext databaseContext) : IUserAdministrationAuditEventRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<UserAdministrationAuditEvent> GetAll() => databaseContext.UserAdministrationAuditEvents
        .OrderByDescending(auditEvent => auditEvent.OccurredAt)
        .ThenBy(auditEvent => auditEvent.Id)
        .ToList();

    /// <inheritdoc/>
    public void Add(UserAdministrationAuditEvent auditEvent) => databaseContext.Add(auditEvent);
}