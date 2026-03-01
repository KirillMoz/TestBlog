using System.ComponentModel.DataAnnotations;

namespace TestBlog.Models
{
    public class Article
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заголовок обязателен")]
        [StringLength(200, ErrorMessage = "Заголовок не должен превышать 200 символов")]
        [Display(Name = "Заголовок")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Содержание обязательно")]
        [Display(Name = "Содержание")]
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int ViewCount { get; set; }
        public bool IsPublished { get; set; }

        public int AuthorId { get; set; }
        public virtual User? Author { get; set; }

        public virtual ICollection<ArticleTag>? ArticleTags { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
    }
}