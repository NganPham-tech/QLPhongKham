using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
namespace QLPhongKham.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày giờ hẹn")]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled, NoShow

        [Required]
        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string Notes { get; set; } = "";

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;        // Navigation properties
        public Patient Patient { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
        public Service Service { get; set; } = null!;
        public MedicalRecord? MedicalRecord { get; set; }
        public Invoice? Invoice { get; set; }
    }
}
