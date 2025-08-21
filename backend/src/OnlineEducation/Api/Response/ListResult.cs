namespace OnlineEducation.Api.Response;

public class ListResult<T> : BaseResponse
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    
}