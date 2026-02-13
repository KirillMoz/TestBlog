using TestBlog.Models;

namespace TestBlog.Services
{
    public interface ITagService
    {
        Task<Tag> GetTagByIdAsync(int id);
        Task<Tag> GetTagByNameAsync(string name);
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task<bool> CreateTagAsync(Tag tag);
        Task<bool> UpdateTagAsync(Tag tag);
        Task<bool> DeleteTagAsync(int id);
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int count);
    }
}
