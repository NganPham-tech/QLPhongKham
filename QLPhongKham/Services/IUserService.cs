using Microsoft.AspNetCore.Identity;

namespace QLPhongKham.Services
{
    public interface IUserService
    {
        Task<List<IdentityUser>> GetAllUsersAsync();
        Task<List<IdentityUser>> GetUsersByRoleAsync(string roleName);
        Task<List<string>> GetUserRolesAsync(IdentityUser user);
        Task<IdentityResult> AssignRoleAsync(IdentityUser user, string roleName);
        Task<IdentityResult> RemoveRoleAsync(IdentityUser user, string roleName);        Task<IdentityUser?> GetUserByEmailAsync(string email);
        Task<IdentityUser?> GetCurrentUserAsync();
        Task<bool> IsInRoleAsync(IdentityUser user, string roleName);
        Task<IdentityUser?> GetUserByIdAsync(string userId);
    }
}
