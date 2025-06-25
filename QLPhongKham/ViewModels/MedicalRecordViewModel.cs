using System.ComponentModel.DataAnnotations;
using QLPhongKham.Models;

namespace QLPhongKham.ViewModels
{
    public class MedicalRecordViewModel
    {
        public int MedicalRecordId { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn lịch hẹn")]
        [Display(Name = "Lịch hẹn")]
        public int AppointmentId { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn bệnh nhân")]
        [Display(Name = "Bệnh nhân")]
        public int PatientId { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập triệu chứng")]
        [StringLength(1000, ErrorMessage = "Triệu chứng không được quá 1000 ký tự")]
        [Display(Name = "Triệu chứng")]
        public string Symptoms { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng nhập chẩn đoán")]
        [StringLength(500, ErrorMessage = "Chẩn đoán không được quá 500 ký tự")]
        [Display(Name = "Chẩn đoán")]
        public string Diagnosis { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng nhập phương pháp điều trị")]
        [StringLength(1000, ErrorMessage = "Điều trị không được quá 1000 ký tự")]
        [Display(Name = "Phương pháp điều trị")]
        public string Treatment { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Ghi chú không được quá 1000 ký tự")]
        [Display(Name = "Ghi chú")]
        public string Notes { get; set; } = string.Empty;
        
        [Display(Name = "Bác sĩ khám")]
        public int? DoctorId { get; set; }
        
        [StringLength(500, ErrorMessage = "Đơn thuốc không được quá 500 ký tự")]
        [Display(Name = "Đơn thuốc")]
        public string? Prescription { get; set; }
        
        [StringLength(500, ErrorMessage = "Lời khuyên không được quá 500 ký tự")]
        [Display(Name = "Lời khuyên của bác sĩ")]
        public string? Advice { get; set; }
        
        [Display(Name = "Ngày khám")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        [Display(Name = "Huyết áp")]
        public string? BloodPressure { get; set; }
        
        [Display(Name = "Nhịp tim")]
        public int? HeartRate { get; set; }
        
        [Display(Name = "Nhiệt độ (°C)")]
        public decimal? Temperature { get; set; }
        
        [Display(Name = "Cân nặng (kg)")]
        public decimal? Weight { get; set; }
        
        [Display(Name = "Chiều cao (cm)")]
        public decimal? Height { get; set; }
        
        [Display(Name = "Ngày hẹn tái khám")]
        public DateTime? NextAppointmentDate { get; set; }
        
        // Navigation properties for display
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string AppointmentInfo { get; set; } = string.Empty;
    }

    public class PatientMedicalHistoryViewModel
    {
        public Patient Patient { get; set; } = new Patient();
        public List<MedicalRecordDetailViewModel> MedicalRecords { get; set; } = new List<MedicalRecordDetailViewModel>();
        public int TotalRecords { get; set; }
        public DateTime? LastVisit { get; set; }
        public string? LastDiagnosis { get; set; }
    }

    public class MedicalRecordDetailViewModel
    {
        public int MedicalRecordId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Symptoms { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string? Prescription { get; set; }
        public string? Advice { get; set; }
        public string? BloodPressure { get; set; }
        public int? HeartRate { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public DateTime? NextAppointmentDate { get; set; }
        public string ServiceName { get; set; } = string.Empty;
    }

    public class CreateMedicalRecordViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn lịch hẹn")]
        public int AppointmentId { get; set; }
        
        public List<Appointment> AvailableAppointments { get; set; } = new List<Appointment>();
        public List<Doctor> AvailableDoctors { get; set; } = new List<Doctor>();
        
        public MedicalRecordViewModel MedicalRecord { get; set; } = new MedicalRecordViewModel();
        
        // For patient info display
        public string PatientName { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
        public DateTime? PatientDateOfBirth { get; set; }
        public string PatientGender { get; set; } = string.Empty;
    }

    public class MedicalRecordSearchViewModel
    {
        [Display(Name = "Tên bệnh nhân")]
        public string? PatientName { get; set; }
        
        [Display(Name = "Số điện thoại")]
        public string? PatientPhone { get; set; }
        
        [Display(Name = "Từ ngày")]
        public DateTime? FromDate { get; set; }
        
        [Display(Name = "Đến ngày")]
        public DateTime? ToDate { get; set; }
        
        [Display(Name = "Bác sĩ")]
        public int? DoctorId { get; set; }
        
        [Display(Name = "Chẩn đoán")]
        public string? Diagnosis { get; set; }
        
        public List<MedicalRecordDetailViewModel> Results { get; set; } = new List<MedicalRecordDetailViewModel>();
        public List<Doctor> AvailableDoctors { get; set; } = new List<Doctor>();
        
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public int PageSize { get; set; } = 10;
    }
}
