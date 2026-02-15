using System.ComponentModel.DataAnnotations;

namespace TestBlog.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public virtual ICollection<UserRole>? UserRoles { get; set; }
    }
}
