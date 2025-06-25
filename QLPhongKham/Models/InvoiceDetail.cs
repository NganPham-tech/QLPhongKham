using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QLPhongKham.Models
{
    public class InvoiceDetail
    {
        [Key]
        public int InvoiceDetailId { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        public int? ServiceId { get; set; }

        public int? MedicineId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải > 0")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Đơn giá")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Thành tiền")]
        public decimal TotalPrice => Quantity * UnitPrice;

        [StringLength(200)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }        // Navigation properties
        public required Invoice Invoice { get; set; }
        public Service? Service { get; set; }
        public Medicine? Medicine { get; set; }
    }
}
