using MoblieShop.Models;

namespace MoblieShop.Interface
{
    public interface IProductRecommendationRepository
    {
        List<Product> GetAllProducts();
    }
}
