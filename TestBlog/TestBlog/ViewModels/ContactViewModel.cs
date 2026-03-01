using System.ComponentModel.DataAnnotations;

namespace TestBlog.ViewModels
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(100, ErrorMessage = "Имя не должно превышать 100 символов")]
        [Display(Name = "Ваше имя")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Тема обязательна")]
        [StringLength(200, ErrorMessage = "Тема не должна превышать 200 символов")]
        [Display(Name = "Тема")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Сообщение обязательно")]
        [StringLength(5000, ErrorMessage = "Сообщение не должно превышать 5000 символов")]
        [Display(Name = "Сообщение")]
        public string Message { get; set; } = string.Empty;
    }
}
