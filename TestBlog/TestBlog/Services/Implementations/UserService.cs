using TestBlog.Data.Repositories;
using TestBlog.Models;
using TestBlog.Utils;
using TestBlog.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace TestBlog.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IRepository<User> userRepository,
            IRepository<Role> roleRepository,
            IRepository<UserRole> userRoleRepository,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _logger = logger;
        }

        public async Task<User?> GetUserByUsernameAsync(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var trimmedUsername = username.Trim();
            var users = await _userRepository.FindAsync(u =>
                u.Username != null && u.Username.ToLower() == trimmedUsername.ToLower());

            return users.FirstOrDefault();
        }

        public async Task<User?> GetUserByEmailAsync(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var trimmedEmail = email.Trim().ToLower();
            var users = await _userRepository.FindAsync(u =>
                u.Email != null && u.Email.ToLower() == trimmedEmail);

            return users.FirstOrDefault();
        }

        public async Task<bool> CreateUserAsync(User user, string password)
        {
            try
            {
                if (user == null)
                    return false;

                user.Username = user.Username?.Trim() ?? string.Empty;
                user.Email = user.Email?.Trim().ToLower() ?? string.Empty;

                if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(password))
                {
                    return false;
                }

                var existingUserByUsername = await GetUserByUsernameAsync(user.Username);
                if (existingUserByUsername != null)
                {
                    return false;
                }

                var existingUserByEmail = await GetUserByEmailAsync(user.Email);
                if (existingUserByEmail != null)
                {
                    return false;
                }

                user.PasswordHash = PasswordHelper.HashPassword(password);
                user.RegistrationDate = DateTime.Now;
                user.IsActive = true;

                await _userRepository.AddAsync(user);
                await _userRepository.SaveAsync();

                await AddUserToRoleAsync(user.Id, "User");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании пользователя");
                return false;
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                if (user == null)
                    return false;

                _userRepository.Update(user);
                await _userRepository.SaveAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var user = await GetUserByIdAsync(id);
                if (user != null && user.Username != "admin")
                {
                    _userRepository.Delete(user);
                    await _userRepository.SaveAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AuthenticateAsync(string? username, string? password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            var user = await GetUserByUsernameAsync(username);
            if (user == null || !user.IsActive || string.IsNullOrEmpty(user.PasswordHash))
                return false;

            return PasswordHelper.VerifyPassword(password, user.PasswordHash);
        }

        public async Task<IEnumerable<Role>> GetUserRolesAsync(int userId)
        {
            var userRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == userId);
            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

            var roles = new List<Role>();
            foreach (var roleId in roleIds)
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role != null)
                    roles.Add(role);
            }

            return roles;
        }

        public async Task<bool> AddUserToRoleAsync(int userId, string roleName)
        {
            try
            {
                if (string.IsNullOrEmpty(roleName))
                    return false;

                var user = await GetUserByIdAsync(userId);
                var roles = await _roleRepository.FindAsync(r => r.Name == roleName);
                var role = roles.FirstOrDefault();

                if (user == null || role == null)
                    return false;

                var existingUserRole = await _userRoleRepository
                    .FindAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

                if (existingUserRole.Any())
                    return false;

                await _userRoleRepository.AddAsync(new UserRole
                {
                    UserId = userId,
                    RoleId = role.Id
                });

                await _userRoleRepository.SaveAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveUserFromRoleAsync(int userId, string roleName)
        {
            try
            {
                if (string.IsNullOrEmpty(roleName))
                    return false;

                var roles = await _roleRepository.FindAsync(r => r.Name == roleName);
                var role = roles.FirstOrDefault();

                if (role == null)
                    return false;

                var userRoles = await _userRoleRepository
                    .FindAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

                var userRole = userRoles.FirstOrDefault();
                if (userRole == null)
                    return false;

                _userRoleRepository.Delete(userRole);
                await _userRoleRepository.SaveAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsUserInRoleAsync(int userId, string roleName)
        {
            var roles = await GetUserRolesAsync(userId);
            return roles.Any(r => r.Name == roleName);
        }

        public async Task<bool> ChangePasswordAsync(int userId, string? oldPassword, string? newPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
                    return false;

                var user = await GetUserByIdAsync(userId);
                if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                    return false;

                if (!PasswordHelper.VerifyPassword(oldPassword, user.PasswordHash))
                    return false;

                user.PasswordHash = PasswordHelper.HashPassword(newPassword);
                _userRepository.Update(user);
                await _userRepository.SaveAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}