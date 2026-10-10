using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Abstractions;

public interface IPasswordHasher
{
    PasswordHash Hash(string plainPassword);
    bool Verify(string plainPassword, PasswordHash hash);
}