using System.ComponentModel.DataAnnotations;

namespace TestBlog.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ICollection<ArticleTag> ArticleTags { get; set; }
    }
}
