using TestBlog.Models;

namespace TestBlog.Services.Interfaces  
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string? username);
        Task<User?> GetUserByEmailAsync(string? email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> CreateUserAsync(User user, string password);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> AuthenticateAsync(string? username, string? password);
        Task<IEnumerable<Role>> GetUserRolesAsync(int userId);
        Task<bool> AddUserToRoleAsync(int userId, string roleName);
        Task<bool> RemoveUserFromRoleAsync(int userId, string roleName);
        Task<bool> IsUserInRoleAsync(int userId, string roleName);
        Task<bool> ChangePasswordAsync(int userId, string? oldPassword, string? newPassword);  
    }
}