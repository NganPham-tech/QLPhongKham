using System.ComponentModel.DataAnnotations;

namespace QLPhongKham.ViewModels
{
    public class CreateStaffAccountViewModel
    {
        [Required(ErrorMessage = "Họ là bắt buộc")]
        [Display(Name = "Họ và tên đệm")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [Display(Name = "Tên")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        [Display(Name = "Vai trò")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [Display(Name = "Giới tính")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Ngày vào làm")]
        [DataType(DataType.Date)]
        public DateTime DateHired { get; set; } = DateTime.Today;

        // Fields cho Doctor
        [Display(Name = "Chuyên khoa")]
        public string? Specialty { get; set; }

        [Display(Name = "Trình độ chuyên môn")]
        public string? Qualification { get; set; }

        [Display(Name = "Mô tả / Kinh nghiệm")]
        public string? Description { get; set; }

        // Fields cho Staff
        [Display(Name = "Chức vụ")]
        public string? Position { get; set; }

        [Display(Name = "Phòng ban")]
        public string? Department { get; set; }

        [Display(Name = "Lương")]
        public decimal? Salary { get; set; }
    }
}
