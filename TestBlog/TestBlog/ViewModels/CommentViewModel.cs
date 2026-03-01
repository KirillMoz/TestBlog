using System.ComponentModel.DataAnnotations;

namespace TestBlog.ViewModels
{
    public class CommentViewModel
    {
        public int ArticleId { get; set; }

        [Required(ErrorMessage = "Текст комментария обязателен")]
        [StringLength(2000, ErrorMessage = "Комментарий не должен превышать 2000 символов")]
        [Display(Name = "Комментарий")]
        public string Content { get; set; } = string.Empty;
    }
}
