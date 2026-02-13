using Microsoft.EntityFrameworkCore;
using TestBlog.Models;
using TestBlog.Utils;

namespace TestBlog.Data
{
    public static class DbInit
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            if (await context.Users.AnyAsync())
                return;

            // Create admin user
            var admin = new User
            {
                Username = "admin",
                Email = "admin@blog.com",
                PasswordHash = PasswordHelper.HashPassword("admin"),
                RegistrationDate = DateTime.Now,
                IsActive = true
            };

            await context.Users.AddAsync(admin);
            await context.SaveChangesAsync();

            // Assign Admin role
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null)
            {
                await context.UserRoles.AddAsync(new UserRole
                {
                    UserId = admin.Id,
                    RoleId = adminRole.Id
                });
                await context.SaveChangesAsync();
            }

            // Create test user
            var testUser = new User
            {
                Username = "user",
                Email = "user@example.com",
                PasswordHash = PasswordHelper.HashPassword("user123"),
                RegistrationDate = DateTime.Now,
                IsActive = true
            };

            await context.Users.AddAsync(testUser);
            await context.SaveChangesAsync();

            // Assign User role
            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole != null)
            {
                await context.UserRoles.AddAsync(new UserRole
                {
                    UserId = testUser.Id,
                    RoleId = userRole.Id
                });
                await context.SaveChangesAsync();
            }

            // Create moderator
            var moderator = new User
            {
                Username = "moderator",
                Email = "moderator@blog.com",
                PasswordHash = PasswordHelper.HashPassword("moderator123"),
                RegistrationDate = DateTime.Now,
                IsActive = true
            };

            await context.Users.AddAsync(moderator);
            await context.SaveChangesAsync();

            // Assign Moderator role
            var moderatorRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Moderator");
            if (moderatorRole != null)
            {
                await context.UserRoles.AddAsync(new UserRole
                {
                    UserId = moderator.Id,
                    RoleId = moderatorRole.Id
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
