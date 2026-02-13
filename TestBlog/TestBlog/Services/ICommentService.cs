using TestBlog.Models;

namespace TestBlog.Services
{
    public interface ICommentService
    {
        Task<Comment> GetCommentByIdAsync(int id);
        Task<IEnumerable<Comment>> GetCommentsByArticleAsync(int articleId);
        Task<IEnumerable<Comment>> GetCommentsByUserAsync(int userId);
        Task<bool> CreateCommentAsync(Comment comment);
        Task<bool> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(int id);
        Task<bool> ApproveCommentAsync(int id);
    }
}
