using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace QLPhongKham.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc")]
        [StringLength(150, ErrorMessage = "Tên dịch vụ không được quá 150 ký tự")]
        [Display(Name = "Tên dịch vụ")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [StringLength(500, ErrorMessage = "Mô tả không được quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Giá dịch vụ là bắt buộc")]
        [DataType(DataType.Currency)]
        [Display(Name = "Giá dịch vụ")]
        public decimal Price { get; set; }        [Display(Name = "Thời gian thực hiện (phút)")]
        public int? Duration { get; set; }

        [StringLength(200)]
        [Display(Name = "Chuyên khoa")]
        public string? Specialty { get; set; }   // Chuyên khoa yêu cầu cho dịch vụ này

        [Display(Name = "Còn hoạt động")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    
}
}
