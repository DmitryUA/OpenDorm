namespace OpenDorm.Application.Abstractions;

public record PagedQuery(int Page = 1, int PageSize = 20);