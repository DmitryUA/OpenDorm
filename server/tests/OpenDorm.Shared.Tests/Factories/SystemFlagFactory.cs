using OpenDorm.Domain.Aggregates.System;

namespace OpenDorm.Shared.Tests.Factories;

public static class SystemFlagFactory
{
    public const string DefaultName = "feature:test-flag";
    public const string DefaultValue = "enabled";

    /// <summary>
    /// Создаёт валидный системный флаг с указанными параметрами.
    /// Параметры, не переданные явно, заполняются дефолтными валидными значениями.
    /// </summary>
    public static SystemFlag Create(string? name = null, string? value = null)
    {
        return new SystemFlag(
            Guid.NewGuid(),
            name ?? DefaultName,
            value ?? DefaultValue);
    }
}