
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace QLPhongKham.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [Required]
        public int PatientId { get; set; }

        public int? AppointmentId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Số hóa đơn")]
        public string InvoiceNumber { get; set; } = "";

        [Required]
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày đáo hạn")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Tổng tiền trước thuế")]
        public decimal SubTotal { get; set; } = 0;

        [Display(Name = "Thuế VAT")]
        public decimal TaxAmount { get; set; } = 0;

        [Display(Name = "Giảm giá")]
        public decimal DiscountAmount { get; set; } = 0;

        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; } = 0;

        [Display(Name = "Số tiền đã thanh toán")]
        public decimal PaidAmount { get; set; } = 0;

        public int? StaffId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Draft";

        [Required]
        [StringLength(20)]
        [Display(Name = "Loại hóa đơn")]
        public string InvoiceType { get; set; } = "Service";

        [Display(Name = "Hóa đơn điện tử")]
        public bool IsElectronic { get; set; } = false;

        [StringLength(50)]
        [Display(Name = "Mã hóa đơn điện tử")]
        public string? ElectronicInvoiceCode { get; set; }

        [StringLength(200)]
        [Display(Name = "Thông tin công ty (cho hóa đơn VAT)")]
        public string? CompanyInfo { get; set; }

        [StringLength(20)]
        [Display(Name = "Mã số thuế")]
        public string? TaxCode { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public Patient? Patient { get; set; }
        public Appointment? Appointment { get; set; }
        public Staff? Staff { get; set; }
        public ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        // Computed properties
        [NotMapped]
        public decimal RemainingAmount => TotalAmount - PaidAmount;

        [NotMapped]
        public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.Now && RemainingAmount > 0;

        [NotMapped]
        public bool IsPaid => RemainingAmount <= 0;

        [NotMapped]
        public bool IsPartiallyPaid => PaidAmount > 0 && RemainingAmount > 0;
    }
}
