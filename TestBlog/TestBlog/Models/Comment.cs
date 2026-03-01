using System.ComponentModel.DataAnnotations;

namespace TestBlog.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Текст комментария обязателен")]
        [StringLength(2000, ErrorMessage = "Комментарий не должен превышать 2000 символов")]
        [Display(Name = "Комментарий")]
        public required string Content { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public int ArticleId { get; set; }
        public int UserId { get; set; }
        public int? ParentCommentId { get; set; }
        public bool IsApproved { get; set; }

        // Навигационные свойства
        public virtual Article? Article { get; set; }
        public virtual User? User { get; set; }
        public virtual Comment? ParentComment { get; set; }
        public virtual ICollection<Comment>? ChildComments { get; set; }
    }
}
