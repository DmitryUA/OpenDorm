using OpenDorm.Domain.Common;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Domain.Aggregates.User;

public class User : AggregateRoot
{
    public Login Login { get; private set; } = null!;
    public PasswordHash PasswordHash { get; private set; } = null!;
    public LastName LastName { get; private set; } = null!;
    public FirstName FirstName { get; private set; } = null!;
    public Patronymic? Patronymic { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    public string FullName => Patronymic == null
        ? $"{LastName} {FirstName}"
        : $"{LastName} {FirstName} {Patronymic}";

    private readonly List<UserRole> _userRoles = [];
    internal IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    // ReSharper disable once UnusedMember.Local
    private User() { } // For EF Core only

    public User(
        Guid id,
        Login login,
        PasswordHash passwordHash,
        LastName lastName,
        FirstName firstName,
        Patronymic? patronymic) : base(id)
    {
        Login = login ?? throw new InvalidUserException("Login cannot be null.");
        PasswordHash = passwordHash ?? throw new InvalidUserException("Password hash cannot be null.");
        LastName = lastName ?? throw new InvalidUserException("Last name cannot be null.");
        FirstName = firstName ?? throw new InvalidUserException("First name cannot be null.");
        Patronymic = patronymic;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdatePersonalData(
        LastName lastName,
        FirstName firstName,
        Patronymic? patronymic)
    {
        LastName = lastName ?? throw new InvalidUserException("Last name cannot be null.");
        FirstName = firstName ?? throw new InvalidUserException("First name cannot be null.");
        Patronymic = patronymic;
    }
    public void AssignRole(Guid roleId)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId))
            throw new DomainException($"User '{FullName}' already has this role. User id: '{Id}'.");

        _userRoles.Add(new UserRole(Guid.NewGuid(), Id, roleId));
    }

    public void RevokeRole(Guid roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId)
            ?? throw new DomainException($"User '{FullName}' does not have this role. User id: '{Id}'.");

        _userRoles.Remove(userRole);
    }

    public void ChangePassword(PasswordHash newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }
}