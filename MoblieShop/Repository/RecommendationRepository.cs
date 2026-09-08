using Microsoft.EntityFrameworkCore;
using MoblieShop.Data;
using MoblieShop.Models;

namespace MoblieShop.Repository
{
    public class RecommendationRepository : IRecommendationRepository
    {
        private readonly ApplicationDbContext _context;

        public RecommendationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RecommendationOrderData>> GetOrderDataAsync(string userId)
        {
            return await _context.OrderDetails
                .Where(od => od.Order.UserId == userId)
                .GroupBy(od => od.ProductId)
                .Select(g => new RecommendationOrderData
                {
                    ProductId = g.Key,
                    PurchaseCount = g.Sum(od => od.Quantity)
                })
                .ToListAsync();
        }

        public async Task<List<RecommendationViewData>> GetViewDataAsync(string userId)
        {
            return await _context.ProductViews
                .Where(pv => pv.UserId == userId)
                .GroupBy(pv => pv.ProductId)
                .Select(g => new RecommendationViewData
                {
                    ProductId = g.Key,
                    ViewCount = g.Sum(pv => pv.ViewCount)
                })
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Products.FindAsync(productId);
        }

        public List<uint> GetAllProductIds()
        {
            return _context.Products.Select(p => (uint)p.ProductId).ToList();
        }

        public Product? GetProductById(int productId)
        {
            return _context.Products.FirstOrDefault(p => p.ProductId == productId);
        }
    }
}
