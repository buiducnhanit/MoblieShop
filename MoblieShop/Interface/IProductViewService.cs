namespace MoblieShop.Interface
{
    public interface IProductViewService
    {
        Task RecordProductViewAsync(string userId, int productId);
    }
}