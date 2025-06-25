using Microsoft.AspNetCore.Identity;
using QLPhongKham.Models;
using QLPhongKham.Repositories;

namespace QLPhongKham.Services
{
    public class DoctorLinkingService : IDoctorLinkingService
    {
        private readonly IDoctorRepository _doctorRepo;
        private readonly UserManager<IdentityUser> _userManager;

        public DoctorLinkingService(IDoctorRepository doctorRepo, UserManager<IdentityUser> userManager)
        {
            _doctorRepo = doctorRepo;
            _userManager = userManager;
        }        public async Task<Doctor?> GetDoctorByUserIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user?.Email == null) return null;

            return _doctorRepo.GetByEmail(user.Email);
        }

        public async Task<bool> LinkUserToDoctorAsync(string userId, int doctorId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var doctor = _doctorRepo.GetById(doctorId);

            if (user == null || doctor == null) return false;

            doctor.Email = user.Email;
            _doctorRepo.Update(doctor);

            return true;
        }
    }
}

