using TestBlog.Data.Repositories;
using TestBlog.Models;

namespace TestBlog.Services.Implementations
{
    public class TagService : ITagService
    {
        private readonly IRepository<Tag> _tagRepository;
        private readonly IRepository<ArticleTag> _articleTagRepository;

        public TagService(
            IRepository<Tag> tagRepository,
            IRepository<ArticleTag> articleTagRepository)
        {
            _tagRepository = tagRepository;
            _articleTagRepository = articleTagRepository;
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
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateTagAsync(Tag tag)
        {
            try
            {
                _tagRepository.Update(tag);
                await _tagRepository.SaveAsync();
                return true;
            }
            catch
            {
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
                    return true;
                }
                return false;
            }
            catch
            {
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