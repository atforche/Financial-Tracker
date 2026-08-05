namespace Domain.Users;

/// <summary>
/// Persistence operations for user-management audit events.
/// </summary>
public interface IUserAdministrationAuditEventRepository
{
    /// <summary>
    /// Gets all audit events ordered by occurrence.
    /// </summary>
    IReadOnlyCollection<UserAdministrationAuditEvent> GetAll();

    /// <summary>
    /// Adds an audit event to the current unit of work.
    /// </summary>
    void Add(UserAdministrationAuditEvent auditEvent);
}