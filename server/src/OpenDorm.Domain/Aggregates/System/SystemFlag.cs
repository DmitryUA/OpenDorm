using OpenDorm.Domain.Common;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.Aggregates.System;

public class SystemFlag : Entity
{
    public const int MaxNameLength = 128;
    public const int MaxValueLength = 512;
    public string Name { get; private set; } = null!;
    public string Value { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private SystemFlag() { } // For EF Core only

    public SystemFlag(Guid id, string name, string value) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidSystemFlagException("Flag name cannot be empty.");

        if (name.Length > MaxNameLength)
            throw new InvalidSystemFlagException($"Flag name cannot exceed {MaxNameLength} characters.");

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidSystemFlagException("Flag value cannot be empty.");

        if (value.Length > MaxValueLength)
            throw new InvalidSystemFlagException($"Flag value cannot exceed {MaxValueLength} characters.");

        Name = name;
        Value = value;
        CreatedAt = DateTime.UtcNow;
    }
}