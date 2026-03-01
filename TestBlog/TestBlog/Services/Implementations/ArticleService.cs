using Microsoft.Extensions.Logging;
using TestBlog.Data.Repositories;
using TestBlog.Models;

namespace TestBlog.Services.Implementations
{
    public class ArticleService : IArticleService
    {
        private readonly IRepository<Article> _articleRepository;
        private readonly IRepository<ArticleTag> _articleTagRepository;
        private readonly IRepository<Tag> _tagRepository;
        private readonly ILogger<ArticleService> _logger;

        public ArticleService(
            IRepository<Article> articleRepository,
            IRepository<ArticleTag> articleTagRepository,
            IRepository<Tag> tagRepository,
            ILogger<ArticleService> logger)
        {
            _articleRepository = articleRepository;
            _articleTagRepository = articleTagRepository;
            _tagRepository = tagRepository;
            _logger = logger;
        }

        public async Task<Article> GetArticleByIdAsync(int id)
        {
            return await _articleRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Article>> GetAllArticlesAsync()
        {
            return await _articleRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Article>> GetPublishedArticlesAsync()
        {
            return await _articleRepository.FindAsync(a => a.IsPublished);
        }

        public async Task<IEnumerable<Article>> GetArticlesByAuthorAsync(int authorId)
        {
            return await _articleRepository.FindAsync(a => a.AuthorId == authorId);
        }

        public async Task<IEnumerable<Article>> GetArticlesByTagAsync(int tagId)
        {
            var articleTags = await _articleTagRepository.FindAsync(at => at.TagId == tagId);
            var articleIds = articleTags.Select(at => at.ArticleId).ToList();

            var articles = new List<Article>();
            foreach (var articleId in articleIds)
            {
                var article = await _articleRepository.GetByIdAsync(articleId);
                if (article != null)
                    articles.Add(article);
            }

            return articles;
        }

        public async Task<bool> CreateArticleAsync(Article article, List<int> tagIds)
        {
            try
            {
                article.CreatedDate = DateTime.Now;
                article.ViewCount = 0;
                article.IsPublished = false;

                await _articleRepository.AddAsync(article);
                await _articleRepository.SaveAsync();

                foreach (var tagId in tagIds)
                {
                    await _articleTagRepository.AddAsync(new ArticleTag
                    {
                        ArticleId = article.Id,
                        TagId = tagId
                    });
                }

                await _articleTagRepository.SaveAsync();
                _logger.LogInformation("Создана статья ID {ArticleId} '{Title}'", article.Id, article.Title);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании статьи '{Title}'", article.Title);
                return false;
            }
        }

        public async Task<bool> UpdateArticleAsync(Article article, List<int> tagIds)
        {
            try
            {
                article.UpdatedDate = DateTime.Now;
                _articleRepository.Update(article);
                await _articleRepository.SaveAsync();

                var existingTags = await _articleTagRepository.FindAsync(at => at.ArticleId == article.Id);
                foreach (var tag in existingTags)
                {
                    _articleTagRepository.Delete(tag);
                }

                foreach (var tagId in tagIds)
                {
                    await _articleTagRepository.AddAsync(new ArticleTag
                    {
                        ArticleId = article.Id,
                        TagId = tagId
                    });
                }

                await _articleTagRepository.SaveAsync();
                _logger.LogInformation("Обновлена статья ID {ArticleId}", article.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении статьи ID {ArticleId}", article.Id);
                return false;
            }
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            try
            {
                var article = await GetArticleByIdAsync(id);
                if (article != null)
                {
                    _articleRepository.Delete(article);
                    await _articleRepository.SaveAsync();
                    _logger.LogInformation("Удалена статья ID {ArticleId}", id);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении статьи ID {ArticleId}", id);
                return false;
            }
        }

        public async Task<bool> PublishArticleAsync(int id)
        {
            try
            {
                var article = await GetArticleByIdAsync(id);
                if (article == null)
                    return false;

                article.IsPublished = true;
                article.UpdatedDate = DateTime.Now;
                _articleRepository.Update(article);
                await _articleRepository.SaveAsync();
                _logger.LogInformation("Опубликована статья ID {ArticleId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при публикации статьи ID {ArticleId}", id);
                return false;
            }
        }

        public async Task<bool> UnpublishArticleAsync(int id)
        {
            try
            {
                var article = await GetArticleByIdAsync(id);
                if (article == null)
                    return false;

                article.IsPublished = false;
                article.UpdatedDate = DateTime.Now;
                _articleRepository.Update(article);
                await _articleRepository.SaveAsync();
                _logger.LogInformation("Снята с публикации статья ID {ArticleId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при снятии с публикации статьи ID {ArticleId}", id);
                return false;
            }
        }

        public async Task IncrementViewCountAsync(int id)
        {
            var article = await GetArticleByIdAsync(id);
            if (article != null)
            {
                article.ViewCount++;
                _articleRepository.Update(article);
                await _articleRepository.SaveAsync();
            }
        }
    }
}