using TestBlog.Models;

namespace TestBlog.Services
{
    public interface IArticleService
    {
        Task<Article> GetArticleByIdAsync(int id);
        Task<IEnumerable<Article>> GetAllArticlesAsync();
        Task<IEnumerable<Article>> GetPublishedArticlesAsync();
        Task<IEnumerable<Article>> GetArticlesByAuthorAsync(int authorId);
        Task<IEnumerable<Article>> GetArticlesByTagAsync(int tagId);
        Task<bool> CreateArticleAsync(Article article, List<int> tagIds);
        Task<bool> UpdateArticleAsync(Article article, List<int> tagIds);
        Task<bool> DeleteArticleAsync(int id);
        Task<bool> PublishArticleAsync(int id);
        Task<bool> UnpublishArticleAsync(int id);
        Task IncrementViewCountAsync(int id);
    }
}
