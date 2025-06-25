using Microsoft.AspNetCore.Identity;

namespace QLPhongKham.ViewModels
{    public class UserRoleViewModel
    {
        public required IdentityUser User { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class AssignRoleViewModel
    {
        public required IdentityUser User { get; set; }
        public List<string> AvailableRoles { get; set; } = new List<string>();
        public List<string> UserRoles { get; set; } = new List<string>();
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }

    
}
