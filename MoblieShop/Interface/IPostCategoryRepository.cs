using MoblieShop.Models;

namespace MoblieShop.Interface
{
    public interface IPostCategoryRepository
    {
        Task<IEnumerable<PostCategory>> GetAllCategoriesAsync();
        Task<PostCategory> GetCategoryByIdAsync(int id);
        Task AddAsync(PostCategory category);
        Task UpdateAsync(PostCategory category);
        Task DeleteAsync(int id);
    }
}
