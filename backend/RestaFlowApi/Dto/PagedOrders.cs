namespace RestaFlowApi.Dto
{
    public class PagedOrderResult<T>
    {
        public int TotalCount { get; set; }
        public List<T> Data { get; set; }
    }
}
