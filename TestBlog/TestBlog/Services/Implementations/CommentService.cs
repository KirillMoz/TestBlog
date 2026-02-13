using TestBlog.Data.Repositories;
using TestBlog.Models;

namespace TestBlog.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly IRepository<Comment> _commentRepository;

        public CommentService(IRepository<Comment> commentRepository)
        {
            _commentRepository = commentRepository;
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
                return true;
            }
            catch
            {
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
                return true;
            }
            catch
            {
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
                    return true;
                }
                return false;
            }
            catch
            {
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
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}