using System.ComponentModel.DataAnnotations;

namespace TestBlog.ViewModels
{
    public class TagViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название тега обязательно")]
        [StringLength(50, ErrorMessage = "Название тега не должно превышать 50 символов")]
        [Display(Name = "Название")]
        public required string Name { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Количество статей")]
        public int ArticlesCount { get; set; }
    }
}