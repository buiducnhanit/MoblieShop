using MoblieShop.Models;

namespace MoblieShop.Interface
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(int postId);
        Task AddCommentAsync(Comment comment);
    }
}
