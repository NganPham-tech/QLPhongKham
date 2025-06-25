using Microsoft.AspNetCore.Identity;
using QLPhongKham.Models;
using QLPhongKham.Repositories;

namespace QLPhongKham.Services
{
    public class PatientLinkingService : IPatientLinkingService
    {
        private readonly IPatientRepository _patientRepo;
        private readonly UserManager<IdentityUser> _userManager;

        public PatientLinkingService(IPatientRepository patientRepo, UserManager<IdentityUser> userManager)
        {
            _patientRepo = patientRepo;
            _userManager = userManager;
        }        public async Task<Patient?> GetPatientByUserIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user?.Email == null) return null;

            // Tìm patient theo email
            return _patientRepo.GetByEmail(user.Email);
        }        public Task<Patient> CreatePatientForUserAsync(IdentityUser user)
        {
            // Tách FirstName và LastName từ email hoặc UserName
            var names = ExtractNames(user.Email ?? "unknown@example.com");

            var patient = new Patient
            {
                FirstName = names.firstName,
                LastName = names.lastName,
                Email = user.Email ?? "",
                DateOfBirth = DateTime.Now.AddYears(-25), // Default age
                Gender = "Chưa xác định",
                Address = "",
                Phone = ""
            };

            _patientRepo.Add(patient);
            return Task.FromResult(patient);
        }

        public async Task<bool> LinkUserToPatientAsync(string userId, int patientId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var patient = _patientRepo.GetById(patientId);

            if (user == null || patient == null) return false;            // Cập nhật email của patient để match với user
            patient.Email = user.Email ?? "";
            _patientRepo.Update(patient);

            return true;
        }

        private (string firstName, string lastName) ExtractNames(string email)
        {
            var localPart = email.Split('@')[0];
            var parts = localPart.Split('.');

            if (parts.Length >= 2)
            {
                return (parts[0], string.Join(" ", parts.Skip(1)));
            }

            return (localPart, "");
        }
    }
}
