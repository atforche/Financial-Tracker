using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Users;

/// <summary>
/// EF Core configuration for user-management audit events.
/// </summary>
internal sealed class UserAdministrationAuditEventConfiguration : IEntityTypeConfiguration<UserAdministrationAuditEvent>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserAdministrationAuditEvent> builder)
    {
        builder.HasKey(auditEvent => auditEvent.Id);
        builder.Property(auditEvent => auditEvent.Id)
            .HasConversion(eventId => eventId.Value, value => new UserAdministrationAuditEventId(value));
        builder.Property(auditEvent => auditEvent.ActorUserId)
            .HasConversion(userId => userId == null ? (Guid?)null : userId.Value, value => value == null ? null : new UserId(value.Value));
        builder.Property(auditEvent => auditEvent.TargetUserId)
            .HasConversion(userId => userId == null ? (Guid?)null : userId.Value, value => value == null ? null : new UserId(value.Value));
        builder.Property(auditEvent => auditEvent.TargetInvitationId)
            .HasConversion(invitationId => invitationId == null ? (Guid?)null : invitationId.Value, value => value == null ? null : new UserInvitationId(value.Value));
        builder.Property(auditEvent => auditEvent.Action).HasConversion<string>().IsRequired();
        builder.Property(auditEvent => auditEvent.PreviousRole).HasConversion<string>();
        builder.Property(auditEvent => auditEvent.NewRole).HasConversion<string>();
        builder.Property(auditEvent => auditEvent.IsSystemActor).IsRequired();
        builder.Property(auditEvent => auditEvent.OccurredAt).IsRequired();

        builder.HasIndex(auditEvent => auditEvent.ActorUserId);
        builder.HasIndex(auditEvent => auditEvent.TargetUserId);
        builder.HasIndex(auditEvent => auditEvent.TargetInvitationId);
        builder.HasIndex(auditEvent => auditEvent.OccurredAt);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(auditEvent => auditEvent.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(auditEvent => auditEvent.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<UserInvitation>()
            .WithMany()
            .HasForeignKey(auditEvent => auditEvent.TargetInvitationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}