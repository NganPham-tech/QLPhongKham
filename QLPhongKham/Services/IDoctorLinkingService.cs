using QLPhongKham.Models;

namespace QLPhongKham.Services
{    public interface IDoctorLinkingService
    {
        Task<Doctor?> GetDoctorByUserIdAsync(string userId);
        Task<bool> LinkUserToDoctorAsync(string userId, int doctorId);
    }
}
