using home_work_66.BLL.Models;
using Microsoft.AspNetCore.Identity;

namespace home_work_66.Services;

public class AdminInitializer
{
    public static async Task SeedAdminAndRoleDataAsync(RoleManager<IdentityRole<int>> _roleManager, UserManager<User> _userManager)
    {
        string adminEmail = "admin@admin.admin";
        string adminPassword = "1qaz@WSX";
        var roles = new[] { "admin", "user", "company" };
        foreach (var role in roles)
        {
            if (await _roleManager.FindByNameAsync(role) is null)
                await _roleManager.CreateAsync(new IdentityRole<int>(role));
        }

        if (await _userManager.FindByEmailAsync(adminEmail) == null)
        {
            User admin = new User
            {
                Email = adminEmail,
                UserName = "admin",
                AvatarUrl = "/default.jpg",
            };
            IdentityResult result = await _userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, "admin");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Ошибка при создании админа: {error.Description}");
                }
            }
        }
    }
}