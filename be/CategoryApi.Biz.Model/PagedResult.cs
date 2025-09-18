namespace CategoryApi.Biz.Model;
public class PagedResult<T>
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int Total { get; set; }
    public List<T> Items { get; set; } = [];
    public PagedResult() { }
    public PagedResult(int page, int size, int total, List<T> items)
    { Page = page; Size = size; Total = total; Items = items; }
}
