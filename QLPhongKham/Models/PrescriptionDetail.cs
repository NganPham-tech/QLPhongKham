using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
namespace QLPhongKham.Models
{
    public class PrescriptionDetail
    {
        [Required]        public int PrescriptionId { get; set; }
        [ForeignKey(nameof(PrescriptionId))]
        public required virtual Prescription Prescription { get; set; }

        [Required]
        public int MedicineId { get; set; }
        [ForeignKey(nameof(MedicineId))]
        public required virtual Medicine Medicine { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required, StringLength(500)]
        public required string Instructions { get; set; }
    }
}
