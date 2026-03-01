using Microsoft.Extensions.Logging;
using TestBlog.Data.Repositories;
using TestBlog.Models;

namespace TestBlog.Services.Implementations
{
    public class TagService : ITagService
    {
        private readonly IRepository<Tag> _tagRepository;
        private readonly IRepository<ArticleTag> _articleTagRepository;
        private readonly ILogger<TagService> _logger;

        public TagService(
            IRepository<Tag> tagRepository,
            IRepository<ArticleTag> articleTagRepository,
            ILogger<TagService> logger)
        {
            _tagRepository = tagRepository;
            _articleTagRepository = articleTagRepository;
            _logger = logger;
        }

        public async Task<Tag> GetTagByIdAsync(int id)
        {
            return await _tagRepository.GetByIdAsync(id);
        }

        public async Task<Tag> GetTagByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tag name cannot be empty", nameof(name));

            var tags = await _tagRepository.FindAsync(t => t.Name == name);
            var tag = tags?.FirstOrDefault();

            if (tag == null)
            {
                throw new KeyNotFoundException($"Tag '{name}' not found");
            }

            return tag;
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _tagRepository.GetAllAsync();
        }

        public async Task<bool> CreateTagAsync(Tag tag)
        {
            try
            {
                if (await GetTagByNameAsync(tag.Name) != null)
                    return false;

                await _tagRepository.AddAsync(tag);
                await _tagRepository.SaveAsync();
                _logger.LogInformation("Создан тег ID {TagId} '{TagName}'", tag.Id, tag.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании тега '{TagName}'", tag.Name);
                return false;
            }
        }

        public async Task<bool> UpdateTagAsync(Tag tag)
        {
            try
            {
                _tagRepository.Update(tag);
                await _tagRepository.SaveAsync();
                _logger.LogInformation("Обновлен тег ID {TagId} '{TagName}'", tag.Id, tag.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении тега ID {TagId}", tag.Id);
                return false;
            }
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            try
            {
                var tag = await GetTagByIdAsync(id);
                if (tag != null)
                {
                    var articleTags = await _articleTagRepository.FindAsync(at => at.TagId == id);
                    _articleTagRepository.DeleteRange(articleTags);

                    _tagRepository.Delete(tag);
                    await _tagRepository.SaveAsync();
                    _logger.LogInformation("Удален тег ID {TagId} '{TagName}'", id, tag.Name);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении тега ID {TagId}", id);
                return false;
            }
        }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int count)
        {
            var allTags = await _tagRepository.GetAllAsync();
            return allTags.Take(count);
        }
    }
}