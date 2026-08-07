using System.Net.Mail;

namespace Domain.Users;

/// <summary>
/// Normalizes and validates email addresses used by user management.
/// </summary>
public static class UserEmail
{
    /// <summary>
    /// Maximum supported length for an email address.
    /// </summary>
    public const int MaximumLength = 320;

    /// <summary>
    /// Attempts to normalize an email address according to the user-management contract.
    /// </summary>
    public static bool TryNormalize(
        string? email,
        out string? displayEmail,
        out string? normalizedEmail)
    {
        displayEmail = null;
        normalizedEmail = null;
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        string trimmedEmail = email.Trim();
        if (trimmedEmail.Length > MaximumLength)
        {
            return false;
        }

        try
        {
            MailAddress parsedEmail = new(trimmedEmail);
            if (!string.Equals(parsedEmail.Address, trimmedEmail, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        catch (FormatException)
        {
            return false;
        }

        displayEmail = trimmedEmail;
        normalizedEmail = trimmedEmail.ToLowerInvariant();
        return true;
    }
}