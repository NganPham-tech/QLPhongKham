using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
namespace QLPhongKham.Models
{
    public class Staff
    {
        [Key]
        public int StaffId { get; set; }        [Required(ErrorMessage = "Họ là bắt buộc")]
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
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

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

        [StringLength(50)]
        [Display(Name = "Chức vụ")]
        public string? Position { get; set; }

        [StringLength(50)]
        [Display(Name = "Phòng ban")]
        public string? Department { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Lương")]
        public decimal? Salary { get; set; }

        [StringLength(200)]
        [Display(Name = "Ảnh đại diện")]
        public string? PhotoUrl { get; set; } = "/images/default-staff.jpg";

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
        public string FullName => $"{FirstName} {LastName}";

        // Computed property for years of service
        [Display(Name = "Số năm làm việc")]
        public int YearsOfService
        {
            get
            {
                var today = DateTime.Today;
                var years = today.Year - DateHired.Year;
                if (DateHired.Date > today.AddYears(-years)) years--;
                return years;
            }
        }        // Navigation properties
        public ICollection<Invoice> InvoicesHandled { get; set; } = new List<Invoice>();
        public ICollection<Inventory> InventoriesManaged { get; set; } = new List<Inventory>();
        public ICollection<Payment> PaymentsCollected { get; set; } = new List<Payment>();
    }
}
