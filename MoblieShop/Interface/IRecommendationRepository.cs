using MoblieShop.Models;

namespace MoblieShop.Interface
{
    public interface IRecommendationRepository
    {
        Task<List<RecommendationOrderData>> GetOrderDataAsync(string userId);
        Task<List<RecommendationViewData>> GetViewDataAsync(string userId);
        Task<Product?> GetProductByIdAsync(int productId);
        List<uint> GetAllProductIds();
        Product? GetProductById(int productId);
    }

    public class RecommendationOrderData
    {
        public int ProductId { get; set; }
        public int PurchaseCount { get; set; }
    }

    public class RecommendationViewData
    {
        public int ProductId { get; set; }
        public int ViewCount { get; set; }
    }
}