namespace ChatShaker.Api.Helpers.Models
{
    public class ItemWithMeta<TData>
    {
        public TData Data { get; set; }
        public dynamic Meta { get; set; }
    }
}
