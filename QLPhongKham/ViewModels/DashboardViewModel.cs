
namespace QLPhongKham.ViewModels
{
    public class DashboardViewModel
    {
        // Basic Statistics
        public int TotalUsers { get; set; }
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int InProgressAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int TotalStaff { get; set; }

        // Financial Statistics
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal PendingPayments { get; set; }
        public int UnpaidInvoicesCount { get; set; }
        public int OverdueInvoices { get; set; }

        // Inventory Alerts
        public int LowStockMedicines { get; set; }
        public int ExpiredMedicines { get; set; }
        public int ExpiringMedicines { get; set; }

        // Patient Statistics
        public int NewPatientsThisMonth { get; set; }
        public int NewPatientsToday { get; set; }

        // Appointment Statistics
        public int TomorrowAppointments { get; set; }
        public int WeeklyAppointments { get; set; }
        public int LateAppointments { get; set; }

        // Collections
        public List<TodayAppointmentViewModel> TodayAppointmentsList { get; set; } = new List<TodayAppointmentViewModel>();
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new List<RecentActivityViewModel>();
        public List<UnpaidInvoiceViewModel> UnpaidInvoicesList { get; set; } = new List<UnpaidInvoiceViewModel>();
        public List<AlertViewModel> Alerts { get; set; } = new List<AlertViewModel>();
        public List<MonthlyRevenueData> MonthlyRevenueChart { get; set; } = new List<MonthlyRevenueData>();
        public List<ServiceStatistics> TopServices { get; set; } = new List<ServiceStatistics>();
        public List<AppointmentStatusData> AppointmentStatusChart { get; set; } = new List<AppointmentStatusData>();
    }

    public class TodayAppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
    }

    public class UnpaidInvoiceViewModel
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class AlertViewModel
    {
        public string Type { get; set; } = string.Empty; // Medicine, Invoice, Appointment
        public string Level { get; set; } = string.Empty; // Info, Warning, Danger
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class MonthlyRevenueData
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int AppointmentCount { get; set; }
    }

    public class ServiceStatistics
    {
        public string ServiceName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Revenue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class AppointmentStatusData
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class RecentActivityViewModel
    {
        public string Action { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}