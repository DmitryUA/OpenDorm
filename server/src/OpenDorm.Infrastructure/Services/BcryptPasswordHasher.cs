using OpenDorm.Application.Abstractions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Infrastructure.Services;

public class BcryptPasswordHasher : IPasswordHasher
{
    public PasswordHash Hash(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
            throw new ArgumentException("Password cannot be empty.", nameof(plainPassword));

        var hashed = BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);
        return new PasswordHash(hashed);
    }

    public bool Verify(string plainPassword, PasswordHash hash)
    {
        return BCrypt.Net.BCrypt.Verify(plainPassword, hash.Value);
    }
}