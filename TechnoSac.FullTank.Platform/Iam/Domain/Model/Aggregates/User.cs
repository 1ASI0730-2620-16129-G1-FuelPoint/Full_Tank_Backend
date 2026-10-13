using System.Text.Json.Serialization;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;

namespace TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;

/// <summary>
///     Aggregate root representing an application user (buyer/applicant or provider/distributor operator).
/// </summary>
/// <remarks>
///     Carries both the profile data (<see cref="Name" />, <see cref="Email" />, <see cref="Role" />,
///     <see cref="CompanyId" />) and the authentication credentials (<see cref="Username" />,
///     <see cref="PasswordHash" />). The password is never stored in plain text and the hash is excluded
///     from serialization via <see cref="JsonIgnoreAttribute" />.
/// </remarks>
public partial class User
{
    /// <summary>Protected parameterless constructor for EF Core.</summary>
    protected User()
    {
        Name = string.Empty;
        Email = string.Empty;
        Username = string.Empty;
        PasswordHash = string.Empty;
        Role = string.Empty;
    }

    /// <summary>Creates an authenticated user from a sign-up command and an already-hashed password.</summary>
    public User(SignUpCommand command, string passwordHash)
    {
        ArgumentNullException.ThrowIfNull(command);
        var role = string.IsNullOrWhiteSpace(command.Role) ? "BUYER" : command.Role.Trim().ToUpperInvariant();
        if (role != "BUYER" && role != "PROVIDER")
            throw new ArgumentException("Unauthorized role. Public registration only supports BUYER or PROVIDER.", nameof(command));

        Name = command.Name;
        Email = command.Email;
        Username = string.IsNullOrWhiteSpace(command.Username) ? command.Email : command.Username;
        PasswordHash = passwordHash;
        Role = role;
        CompanyId = command.CompanyId;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Username { get; private set; }

    /// <summary>BCrypt password hash. Excluded from JSON serialization — never returned in resources.</summary>
    [JsonIgnore]
    public string PasswordHash { get; private set; }

    public string Role { get; private set; }
    public int? CompanyId { get; private set; }

    /// <summary>Updates only the editable profile fields. Never touches the password hash.</summary>
    public User UpdateProfile(string name, string email)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name;
        if (!string.IsNullOrWhiteSpace(email)) Email = email;
        return this;
    }

    /// <summary>Replaces the stored password hash. The caller is responsible for hashing.</summary>
    public User ChangePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
        PasswordHash = passwordHash;
        return this;
    }
}
