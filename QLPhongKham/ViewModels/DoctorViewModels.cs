using System.ComponentModel.DataAnnotations;
using QLPhongKham.Models;

namespace QLPhongKham.ViewModels
{
    public class DoctorCreateViewModel
    {
        [Required(ErrorMessage = "Họ là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ không được quá 100 ký tự")]
        [Display(Name = "Họ và tên đệm")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên không được quá 100 ký tự")]
        [Display(Name = "Tên")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [Display(Name = "Giới tính")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [StringLength(200)]
        [Display(Name = "Chuyên khoa")]
        public string? Specialty { get; set; }

        [StringLength(200)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [StringLength(100)]
        [Display(Name = "Trình độ chuyên môn")]
        public string? Qualification { get; set; }        [StringLength(500)]
        [Display(Name = "Mô tả / Kinh nghiệm")]
        public string? Description { get; set; }

        [Display(Name = "Ngày vào làm")]
        [DataType(DataType.Date)]
        public DateTime DateHired { get; set; } = DateTime.Today;

        [Display(Name = "Ảnh đại diện")]
        public IFormFile? PhotoFile { get; set; }

        public string? PhotoUrl { get; set; } = "/images/default-doctor.jpg";
    }

    public class DoctorEditViewModel
    {
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Họ là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ không được quá 100 ký tự")]
        [Display(Name = "Họ và tên đệm")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên không được quá 100 ký tự")]
        [Display(Name = "Tên")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [Display(Name = "Giới tính")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [StringLength(200)]
        [Display(Name = "Chuyên khoa")]
        public string? Specialty { get; set; }

        [StringLength(200)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [StringLength(100)]
        [Display(Name = "Trình độ chuyên môn")]
        public string? Qualification { get; set; }        [StringLength(500)]
        [Display(Name = "Mô tả / Kinh nghiệm")]
        public string? Description { get; set; }

        [Display(Name = "Ngày vào làm")]
        [DataType(DataType.Date)]
        public DateTime DateHired { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public IFormFile? PhotoFile { get; set; }

        public string? PhotoUrl { get; set; }
    }
}
