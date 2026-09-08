using MoblieShop.Models;

namespace MoblieShop.Interface
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrdersByUserIdAsync(string userId);
    }
}
