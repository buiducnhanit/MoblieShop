using MoblieShop.Models;

namespace MoblieShop.Interface
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetOrdersByUserIdAsync(string userId);
    }
}