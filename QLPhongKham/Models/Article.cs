using System.ComponentModel.DataAnnotations;

namespace QLPhongKham.Models
{
    public class Article
    {
        [Key]
        public int ArticleId { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được quá 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Mô tả ngắn là bắt buộc")]
        [StringLength(500, ErrorMessage = "Mô tả ngắn không được quá 500 ký tự")]
        [Display(Name = "Mô tả ngắn")]
        public string Summary { get; set; } = null!;

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = null!;

        [StringLength(200)]
        [Display(Name = "Ảnh đại diện")]
        public string? FeaturedImage { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Danh mục")]
        public string Category { get; set; } = null!; // Tin tức, Bài viết y tế, Thông báo

        [StringLength(500)]
        [Display(Name = "Tags")]
        public string? Tags { get; set; } // Các từ khóa, cách nhau bởi dấu phẩy

        [Required]
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }

        [Required(ErrorMessage = "Tác giả là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Tác giả")]
        public string Author { get; set; } = null!;

        [Display(Name = "Lượt xem")]
        public int ViewCount { get; set; } = 0;

        [Display(Name = "Còn hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Nổi bật")]
        public bool IsFeatured { get; set; } = false;
    }
}
