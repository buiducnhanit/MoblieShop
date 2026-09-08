using MoblieShop.Models;
using MoblieShop.ViewModels;

namespace MoblieShop.Interface
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task CreateProductAsync(ProductCreateViewModel viewModel);
        Task UpdateProductAsync(ProductUpdateViewModel viewModel);
        Task DeleteProductAsync(int id);
    }
}
