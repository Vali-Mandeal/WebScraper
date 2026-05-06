namespace WebScrapper.ScraperApi.Models;

public record PagedResult<T>
{
    public List<T> Data { get; init; } = [];
    public long Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;
}
