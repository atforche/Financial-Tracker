using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Users;

/// <summary>
/// EF Core configuration for user invitations.
/// </summary>
internal sealed class UserInvitationConfiguration : IEntityTypeConfiguration<UserInvitation>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserInvitation> builder)
    {
        builder.HasKey(invitation => invitation.Id);
        builder.Property(invitation => invitation.Id)
            .HasConversion(invitationId => invitationId.Value, value => new UserInvitationId(value));

        builder.Property(invitation => invitation.Email).HasMaxLength(UserEmail.MaximumLength).IsRequired();
        builder.Property(invitation => invitation.NormalizedEmail).HasMaxLength(UserEmail.MaximumLength).IsRequired();
        builder.Property(invitation => invitation.Role).HasConversion<string>().IsRequired();
        builder.Property(invitation => invitation.Status).HasConversion<string>().IsRequired();
        builder.Property(invitation => invitation.InvitedByUserId)
            .HasConversion(userId => userId == null ? (Guid?)null : userId.Value, value => value == null ? null : new UserId(value.Value));
        builder.Property(invitation => invitation.AcceptedByUserId)
            .HasConversion(userId => userId == null ? (Guid?)null : userId.Value, value => value == null ? null : new UserId(value.Value));
        builder.Property(invitation => invitation.RevokedByUserId)
            .HasConversion(userId => userId == null ? (Guid?)null : userId.Value, value => value == null ? null : new UserId(value.Value));

        builder.HasIndex(invitation => new { invitation.NormalizedEmail, invitation.Status })
            .IsUnique()
            .HasFilter("\"Status\" = 'Pending'");
        builder.HasIndex(invitation => invitation.InvitedByUserId);
        builder.HasIndex(invitation => invitation.AcceptedByUserId);
        builder.HasIndex(invitation => invitation.RevokedByUserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(invitation => invitation.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(invitation => invitation.AcceptedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(invitation => invitation.RevokedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
