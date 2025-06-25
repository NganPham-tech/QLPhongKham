using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations.Schema;
namespace QLPhongKham.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }        [Required(ErrorMessage = "Họ là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ không được quá 100 ký tự")]
        [Display(Name = "Họ")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên không được quá 100 ký tự")]
        [Display(Name = "Tên")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime DateOfBirth { get; set; }        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string Gender { get; set; } = null!;

        [StringLength(200)]
        [Display(Name = "Chuyên khoa")]
        public string? Specialty { get; set; }   // Ví dụ: Nội, Ngoại, Nhi, Sản, v.v.

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Ngày tuyển dụng là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày tuyển dụng")]
        public DateTime DateHired { get; set; }

        [StringLength(200)]
        [Display(Name = "Ảnh đại diện")]
        public string? PhotoUrl { get; set; } = "/images/default-doctor.jpg";

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [StringLength(200)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [StringLength(100)]
        [Display(Name = "Bằng cấp")]
        public string? Qualification { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool IsActive { get; set; } = true;

        // Computed property for age
        [Display(Name = "Tuổi")]
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Year;
                if (DateOfBirth.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        // Computed property for full name
        [Display(Name = "Họ và tên")]
        public string FullName => $"BS. {FirstName} {LastName}";        // Computed property for years of experience
        [Display(Name = "Số năm kinh nghiệm")]
        public int YearsOfExperience
        {
            get
            {
                var today = DateTime.Today;
                var years = today.Year - DateHired.Year;
                if (DateHired.Date > today.AddYears(-years)) years--;
                return years;
            }
        }

        // Computed property for experience in months (more accurate for new employees)
        [Display(Name = "Kinh nghiệm")]
        public string ExperienceDescription
        {
            get
            {
                var today = DateTime.Today;
                var totalMonths = (today.Year - DateHired.Year) * 12 + today.Month - DateHired.Month;
                
                if (today.Day < DateHired.Day)
                    totalMonths--;

                if (totalMonths < 0) totalMonths = 0;

                if (totalMonths >= 12)
                {
                    var years = totalMonths / 12;
                    var months = totalMonths % 12;
                    if (months == 0)
                        return $"{years} năm";
                    else
                        return $"{years} năm {months} tháng";
                }
                else
                {
                    return $"{totalMonths} tháng";
                }
            }
        }

        // Navigation properties
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    }
}
