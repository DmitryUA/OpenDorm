namespace OpenDorm.Application.Abstractions.Search;

public interface IOccupantSearchIndex
{
    byte[]? ComputeHash(string? value);
}
