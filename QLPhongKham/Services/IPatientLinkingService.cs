using Microsoft.AspNetCore.Identity;
using QLPhongKham.Models;

namespace QLPhongKham.Services
{    public interface IPatientLinkingService
    {
        Task<Patient?> GetPatientByUserIdAsync(string userId);
        Task<Patient> CreatePatientForUserAsync(IdentityUser user);
        Task<bool> LinkUserToPatientAsync(string userId, int patientId);
    }
}
