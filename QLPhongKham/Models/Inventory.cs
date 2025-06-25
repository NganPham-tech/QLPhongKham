using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace QLPhongKham.Models
{
    public class Inventory
    {
        [Key]
        public int InventoryId { get; set; }

        [Required]
        public int MedicineId { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải >= 0")]
        [Display(Name = "Số lượng tồn kho")]
        public int QuantityInStock { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày hết hạn")]
        public DateTime? ExpirationDate { get; set; }

        [Required]
        public int ManagedByStaffId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày nhập")]
        public DateTime ImportDate { get; set; } = DateTime.Now;

        [StringLength(200)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }        // Navigation properties
        public required Medicine Medicine { get; set; }
        public required Staff ManagedByStaff { get; set; }
    }
}
