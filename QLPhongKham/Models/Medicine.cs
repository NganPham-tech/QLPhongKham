using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace QLPhongKham.Models
{
    public class Medicine
    {
        [Key]
        public int MedicineId { get; set; }        [Required(ErrorMessage = "Tên thuốc là bắt buộc")]
        [StringLength(150, ErrorMessage = "Tên thuốc không được quá 150 ký tự")]
        [Display(Name = "Tên thuốc")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [StringLength(500, ErrorMessage = "Mô tả không được quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Giá đơn vị là bắt buộc")]
        [DataType(DataType.Currency)]
        [Display(Name = "Giá đơn vị")]
        public decimal UnitPrice { get; set; }

        [StringLength(50)]
        [Display(Name = "Đơn vị tính")]
        public string? Unit { get; set; } = "Viên";

        [StringLength(100)]
        [Display(Name = "Nhà sản xuất")]
        public string? Manufacturer { get; set; }

        [Display(Name = "Còn kinh doanh")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
    }
}
