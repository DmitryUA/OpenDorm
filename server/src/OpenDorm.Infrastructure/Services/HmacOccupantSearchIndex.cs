using System.Security.Cryptography;
using System.Text;
using OpenDorm.Application.Abstractions.Search;

namespace OpenDorm.Infrastructure.Services;

public sealed class HmacOccupantSearchIndex : IOccupantSearchIndex
{
    private readonly byte[] _key;

    public HmacOccupantSearchIndex(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length < 32)
            throw new ArgumentException("The search-index HMAC key must be at least 32 bytes.", nameof(key));

        _key = key.ToArray();
    }

    public byte[]? ComputeHash(string? value)
    {
        var normalized = Normalize(value);
        return normalized is null
            ? null
            : HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(normalized));
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = string.Join(
            ' ',
            value.Normalize(NormalizationForm.FormKC)
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant()
            .Replace('Ё', 'Е');

        return normalized.Length == 0 ? null : normalized;
    }
}
