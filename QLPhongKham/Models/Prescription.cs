using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
namespace QLPhongKham.Models
{
    public class Prescription
    {
        [Key]
        public int Id { get; set; }        [Required]
        public int MedicalRecordId { get; set; }
        [ForeignKey(nameof(MedicalRecordId))]
        public required virtual MedicalRecord MedicalRecord { get; set; }

        [Required]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        public required virtual ICollection<PrescriptionDetail> Details { get; set; }
    }
}
