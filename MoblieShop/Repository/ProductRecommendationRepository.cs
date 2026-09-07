using MoblieShop.Data;
using MoblieShop.Models;

namespace MoblieShop.Repository
{
    public class ProductRecommendationRepository : IProductRecommendationRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRecommendationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Product> GetAllProducts()
        {
            return _context.Products.ToList();
        }
    }
}