namespace OpenDorm.Application.Abstractions;

public record PagedQuery(
    int Page = PagedQuery.DefaultPage,
    int PageSize = PagedQuery.DefaultPageSize)
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
}