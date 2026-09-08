using MoblieShop.Models;

namespace MoblieShop.Interface
{
    public interface IProductRecommendationService
    {
        List<Product> GetRecommendedProducts(int productId);
    }
}
