using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QLPhongKham.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Số giao dịch")]
        public string PaymentNumber { get; set; } = null!;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày thanh toán")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Số tiền thanh toán")]
        public decimal AmountPaid { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Card, Transfer, Momo, ZaloPay, ViettelPay

        [Required]
        [StringLength(20)]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Completed"; // Pending, Completed, Failed, Refunded

        [StringLength(100)]
        [Display(Name = "Mã giao dịch ngân hàng")]
        public string? BankTransactionId { get; set; }

        [StringLength(50)]
        [Display(Name = "Ngân hàng")]
        public string? BankName { get; set; }

        [StringLength(20)]
        [Display(Name = "Số thẻ (4 số cuối)")]
        public string? CardLastFourDigits { get; set; }

        [StringLength(100)]
        [Display(Name = "Mã ủy quyền")]
        public string? AuthorizationCode { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Phí giao dịch")]
        public decimal TransactionFee { get; set; } = 0;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [Display(Name = "Người thu")]
        public int? CollectedByStaffId { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public Invoice Invoice { get; set; } = null!;
        public Staff? CollectedByStaff { get; set; }
    }
}
