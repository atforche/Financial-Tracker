using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Domain.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Tests.Infrastructure;

/// <summary>
/// Hosts the API with its JWT bearer handler and a locally controlled signing key.
/// </summary>
internal sealed class JwtBearerAuthenticationApplicationFactory : FinancialTrackerApplicationFactory
{
    internal const string ProvisionedSubject = "provisioned-test-subject";

    private const string Audience = "financial-tracker-tests";
    private const string Issuer = "https://issuer.test";
    private readonly RSA _signingKey = RSA.Create(2048);

    /// <summary>
    /// Creates a signed ID token with the supplied identity values.
    /// </summary>
    public string CreateToken(
        string subject,
        string issuer = Issuer,
        string audience = Audience,
        DateTime? expires = null,
        string? email = null,
        bool? emailVerified = null,
        string? displayName = null)
    {
        List<Claim> claims = [new Claim("sub", subject)];
        if (email != null)
        {
            claims.Add(new Claim("email", email));
        }
        if (emailVerified.HasValue)
        {
            claims.Add(new Claim("email_verified", emailVerified.Value ? "true" : "false"));
        }
        if (displayName != null)
        {
            claims.Add(new Claim("name", displayName));
        }

        JwtSecurityToken token = new(
            issuer,
            audience,
            claims,
            expires: expires ?? DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(new RsaSecurityKey(_signingKey), SecurityAlgorithms.RsaSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc/>
    public override async Task InitializeDatabaseAsync()
    {
        await base.InitializeDatabaseAsync();
        await SeedUserAsync(ProvisionedSubject, "provisioned@example.test", UserRole.Admin);
    }

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        _ = builder.ConfigureServices(services =>
        {
            _ = services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });
            _ = services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = null;
                options.ConfigurationManager = null;
                options.RequireHttpsMetadata = false;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = Issuer,
                    ValidateAudience = true,
                    ValidAudience = Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(_signingKey),
                    ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                    NameClaimType = "sub"
                };
            });
        });
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _signingKey.Dispose();
        }

        base.Dispose(disposing);
    }
}