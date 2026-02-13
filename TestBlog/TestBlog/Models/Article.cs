using System.ComponentModel.DataAnnotations;

namespace TestBlog.Models
{
    public class Article
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public int AuthorId { get; set; }
        public int ViewCount { get; set; }
        public bool IsPublished { get; set; }

        // Навигационные свойства
        public virtual User Author { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<ArticleTag> ArticleTags { get; set; }
    }
}
