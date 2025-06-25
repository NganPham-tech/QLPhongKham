using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace QLPhongKham.Models
{
    public class MedicalRecord
    {
        [Key]
        public int MedicalRecordId { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]        [StringLength(500)]
        [Display(Name = "Chẩn đoán")]
        public required string Diagnosis { get; set; }

        [Required]
        [StringLength(1000)]
        [Display(Name = "Điều trị")]
        public required string Treatment { get; set; }

        [Required]
        [StringLength(1000)]
        [Display(Name = "Ghi chú")]
        public required string Notes { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int? DoctorId { get; set; }

        [StringLength(200)]
        [Display(Name = "Đơn thuốc")]
        public string? Prescription { get; set; }

        [StringLength(200)]
        [Display(Name = "Lời khuyên")]
        public string? Advice { get; set; }        // Navigation properties
        public required Appointment Appointment { get; set; }
        public required Patient Patient { get; set; }
        public Doctor? Doctor { get; set; }
    }
}
