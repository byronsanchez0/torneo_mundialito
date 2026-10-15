namespace Mundialito.Application.Common.Pagination;

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Data,
    int PageNumber,
    int PageSize,
    long TotalRecords)
{
    public int TotalPages => TotalRecords == 0
        ? 0
        : (int)Math.Ceiling(TotalRecords / (double)PageSize);
}
