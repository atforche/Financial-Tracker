using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Users;

/// <summary>
/// EF Core configuration for application users.
/// </summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).HasConversion(userId => userId.Value, value => new UserId(value));

        builder.Property(user => user.GoogleSubject)
            .HasMaxLength(255)
            .UseCollation("BINARY")
            .IsRequired();
        builder.HasIndex(user => user.GoogleSubject).IsUnique();

        builder.Property(user => user.Email).HasMaxLength(UserEmail.MaximumLength).IsRequired();
        builder.Property(user => user.NormalizedEmail).HasMaxLength(UserEmail.MaximumLength).IsRequired();
        builder.HasIndex(user => user.NormalizedEmail);
        builder.Property(user => user.DisplayName).HasMaxLength(255);
        builder.Property(user => user.Role).HasConversion<string>().IsRequired();
        builder.Property(user => user.Status).HasConversion<string>().IsRequired();
        builder.Property(user => user.CreatedAt).IsRequired();
        builder.Property(user => user.ActivatedAt).IsRequired();
        builder.Property(user => user.UpdatedAt).IsRequired();
    }
}
