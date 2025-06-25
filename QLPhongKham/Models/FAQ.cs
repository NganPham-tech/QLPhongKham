using System.ComponentModel.DataAnnotations;

namespace QLPhongKham.Models
{
    public class FAQ
    {
        [Key]
        public int FAQId { get; set; }

        [Required(ErrorMessage = "Câu hỏi là bắt buộc")]
        [StringLength(300, ErrorMessage = "Câu hỏi không được quá 300 ký tự")]
        [Display(Name = "Câu hỏi")]
        public string Question { get; set; } = null!;

        [Required(ErrorMessage = "Câu trả lời là bắt buộc")]
        [Display(Name = "Câu trả lời")]
        public string Answer { get; set; } = null!;

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Danh mục")]
        public string Category { get; set; } = null!; // Quy trình đặt lịch, Thanh toán, Hủy lịch, etc.

        [Display(Name = "Thứ tự hiển thị")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Còn hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Lượt xem")]
        public int ViewCount { get; set; } = 0;
    }
}
