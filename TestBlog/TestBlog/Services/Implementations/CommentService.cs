using Microsoft.Extensions.Logging;
using TestBlog.Data.Repositories;
using TestBlog.Models;

namespace TestBlog.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly IRepository<Comment> _commentRepository;
        private readonly ILogger<CommentService> _logger;

        public CommentService(IRepository<Comment> commentRepository, ILogger<CommentService> logger)
        {
            _commentRepository = commentRepository;
            _logger = logger;
        }

        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            return await _commentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Comment>> GetCommentsByArticleAsync(int articleId)
        {
            return await _commentRepository.FindAsync(c => c.ArticleId == articleId && c.IsApproved);
        }

        public async Task<IEnumerable<Comment>> GetCommentsByUserAsync(int userId)
        {
            return await _commentRepository.FindAsync(c => c.UserId == userId);
        }

        public async Task<bool> CreateCommentAsync(Comment comment)
        {
            try
            {
                comment.CreatedDate = DateTime.Now;
                comment.IsApproved = false;

                await _commentRepository.AddAsync(comment);
                await _commentRepository.SaveAsync();
                _logger.LogInformation("Создан комментарий ID {CommentId} к статье ID {ArticleId}", comment.Id, comment.ArticleId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании комментария к статье ID {ArticleId}", comment.ArticleId);
                return false;
            }
        }

        public async Task<bool> UpdateCommentAsync(Comment comment)
        {
            try
            {
                comment.UpdatedDate = DateTime.Now;
                _commentRepository.Update(comment);
                await _commentRepository.SaveAsync();
                _logger.LogInformation("Обновлен комментарий ID {CommentId}", comment.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении комментария ID {CommentId}", comment.Id);
                return false;
            }
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            try
            {
                var comment = await GetCommentByIdAsync(id);
                if (comment != null)
                {
                    _commentRepository.Delete(comment);
                    await _commentRepository.SaveAsync();
                    _logger.LogInformation("Удален комментарий ID {CommentId}", id);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении комментария ID {CommentId}", id);
                return false;
            }
        }

        public async Task<bool> ApproveCommentAsync(int id)
        {
            try
            {
                var comment = await GetCommentByIdAsync(id);
                if (comment == null)
                    return false;

                comment.IsApproved = true;
                _commentRepository.Update(comment);
                await _commentRepository.SaveAsync();
                _logger.LogInformation("Одобрен комментарий ID {CommentId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при одобрении комментария ID {CommentId}", id);
                return false;
            }
        }
    }
}