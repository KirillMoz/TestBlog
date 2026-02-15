using TestBlog.Data.Repositories;
using TestBlog.Models;
using TestBlog.Utils;

namespace TestBlog.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<UserRole> _userRoleRepository;

        public UserService(
            IRepository<User> userRepository,
            IRepository<Role> roleRepository,
            IRepository<UserRole> userRoleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("username cannot be empty", nameof(username));

            var users = await _userRepository.FindAsync(u => u.Username == username);
            var user = users?.FirstOrDefault();

            return user ?? throw new KeyNotFoundException($"User with username {username} not found");
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            var users = await _userRepository.FindAsync(u => u.Email == email);
            var user = users?.FirstOrDefault();

            return user ?? throw new KeyNotFoundException($"User with email {email} not found");
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<bool> CreateUserAsync(User user, string password)
        {
            if (user == null)
                return false;

            // Расширенная валидация
            if (string.IsNullOrWhiteSpace(user.Username))
            {
                // Можно логировать ошибку
                return false;
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            // Проверка формата email (опционально)
            if (!IsValidEmail(user.Email))
            {
                return false;
            }

            try
            {
                // Теперь здесь нет предупреждений
                var existingUserByUsername = await GetUserByUsernameAsync(user.Username);
                if (existingUserByUsername != null)
                    return false;

                var existingUserByEmail = await GetUserByEmailAsync(user.Email);
                if (existingUserByEmail != null)
                    return false;

                // Остальной код...
                user.PasswordHash = PasswordHelper.HashPassword(password);
                user.RegistrationDate = DateTime.Now;
                user.IsActive = true;

                await _userRepository.AddAsync(user);
                await _userRepository.SaveAsync();
                await AddUserToRoleAsync(user.Id, "User");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
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

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null || !user.IsActive)
                return false;

            return user.PasswordHash != null &&
                PasswordHelper.VerifyPassword(password, user.PasswordHash);
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

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(oldPassword))
                    return false;

                if (string.IsNullOrWhiteSpace(newPassword))
                    return false;

                var user = await GetUserByIdAsync(userId); 
                if (user == null)
                    return false;

                if (string.IsNullOrEmpty(user.PasswordHash))
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

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}