using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using VHS.Data.Auth.Models.Auth;

namespace VHS.Data.Auth.Seeders
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            var roles = new[] { "global_admin", "admin", "farm_manager", "operator" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        public static async Task SeedAdminUserAsync(
            UserManager<User> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            string email,
            string password,
            string firstName = "Admin",
            string lastName = "User")
        {
            if (!await roleManager.RoleExistsAsync("global_admin"))
                await roleManager.CreateAsync(new IdentityRole<Guid>("global_admin"));

            var adminUser = await userManager.FindByEmailAsync(email);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true,
                    AddedDateTime = DateTime.UtcNow,
                    ModifiedDateTime = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create default admin user: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, "global_admin"))
                await userManager.AddToRoleAsync(adminUser, "global_admin");
        }
    }
}
