using System.ComponentModel.DataAnnotations;
using QLPhongKham.Models;

namespace QLPhongKham.ViewModels
{
    public class AppointmentCreateViewModel
    {
        [Required(ErrorMessage = "Bệnh nhân là bắt buộc")]
        [Display(Name = "Bệnh nhân")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Bác sĩ là bắt buộc")]
        [Display(Name = "Bác sĩ")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Dịch vụ là bắt buộc")]
        [Display(Name = "Dịch vụ")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Ngày giờ hẹn là bắt buộc")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày giờ hẹn")]
        public DateTime AppointmentDate { get; set; } = DateTime.Now.AddHours(1);

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        // For dropdowns
        public List<Patient> Patients { get; set; } = new List<Patient>();
        public List<Doctor> Doctors { get; set; } = new List<Doctor>();
        public List<Service> Services { get; set; } = new List<Service>();
    }

    public class AppointmentEditViewModel
    {
        public int AppointmentId { get; set; }

        [Required(ErrorMessage = "Bệnh nhân là bắt buộc")]
        [Display(Name = "Bệnh nhân")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Bác sĩ là bắt buộc")]
        [Display(Name = "Bác sĩ")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Dịch vụ là bắt buộc")]
        [Display(Name = "Dịch vụ")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Ngày giờ hẹn là bắt buộc")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày giờ hẹn")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        // For dropdowns
        public List<Patient> Patients { get; set; } = new List<Patient>();
        public List<Doctor> Doctors { get; set; } = new List<Doctor>();
        public List<Service> Services { get; set; } = new List<Service>();
    }

    public class AppointmentCalendarViewModel
    {
        public int AppointmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public bool AllDay { get; set; } = false;
    }

    public class CalendarIndexViewModel
    {
        public List<AppointmentCalendarViewModel> Appointments { get; set; } = new List<AppointmentCalendarViewModel>();
        public List<Doctor> Doctors { get; set; } = new List<Doctor>();
        public DateTime CurrentDate { get; set; } = DateTime.Today;
        public string View { get; set; } = "month"; // month, week, day
    }
}
