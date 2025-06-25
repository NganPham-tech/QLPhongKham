using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;
using QLPhongKham.ViewModels;
using QLPhongKham.Services;
using System.Text.Json;
using System.Security.Claims;

namespace QLPhongKham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,BacSi,NhanVien")]    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IInvoiceService _invoiceService;
        private readonly IUserService _userService;

        public AppointmentController(
            ApplicationDbContext context, 
            INotificationService notificationService,
            IInvoiceService invoiceService,
            IUserService userService)
        {
            _context = context;
            _notificationService = notificationService;
            _invoiceService = invoiceService;
            _userService = userService;
        }

        // GET: Admin/Appointment
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            var calendarAppointments = appointments.Select(a => new AppointmentCalendarViewModel
            {
                AppointmentId = a.AppointmentId,
                Title = $"{a.Patient.FullName} - {a.Service.Name}",
                Start = a.AppointmentDate,
                End = a.AppointmentDate.AddMinutes(a.Service.Duration ?? 30),
                Status = a.Status,
                PatientName = a.Patient.FullName,
                DoctorName = a.Doctor.FullName,
                ServiceName = a.Service.Name,
                Color = GetStatusColor(a.Status)
            }).ToList();

            var doctors = await _context.Doctors.Where(d => d.IsActive).ToListAsync();

            var viewModel = new CalendarIndexViewModel
            {
                Appointments = calendarAppointments,
                Doctors = doctors
            };

            return View(viewModel);
        }       
        public async Task<IActionResult> GetCalendarEvents(DateTime start, DateTime end, int? doctorId = null)
        {
            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end);

            if (doctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == doctorId.Value);
            }

            var appointments = await query.ToListAsync();

            var events = appointments.Select(a => new
            {
                id = a.AppointmentId,
                title = $"{a.Patient.FullName} - {a.Service.Name}",
                start = a.AppointmentDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = a.AppointmentDate.AddMinutes(a.Service.Duration ?? 30).ToString("yyyy-MM-ddTHH:mm:ss"),
                color = GetStatusColor(a.Status),
                extendedProps = new
                {
                    patientName = a.Patient.FullName,
                    doctorName = a.Doctor.FullName,
                    serviceName = a.Service.Name,
                    status = a.Status,
                    notes = a.Notes
                }
            });

            return Json(events);
        }

        // GET: Admin/Appointment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }        // GET: Admin/Appointment/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new AppointmentCreateViewModel
            {
                Patients = await _context.Patients.OrderBy(p => p.FirstName).ToListAsync(),
                Doctors = await _context.Doctors.Where(d => d.IsActive).OrderBy(d => d.FirstName).ToListAsync(),
                Services = await _context.Services.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Admin/Appointment/Create (with debug logging)
        public async Task<IActionResult> CreateWithDebug()
        {
            var patients = await _context.Patients.OrderBy(p => p.FirstName).ToListAsync();
            var allDoctors = await _context.Doctors.ToListAsync();
            var activeDoctors = await _context.Doctors.Where(d => d.IsActive).OrderBy(d => d.FirstName).ToListAsync();
            var services = await _context.Services.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();

            var viewModel = new AppointmentCreateViewModel
            {
                Patients = patients,
                Doctors = activeDoctors,
                Services = services
            };

            // Log debug info
            Console.WriteLine($"Total patients: {patients.Count}");
            Console.WriteLine($"Total doctors: {allDoctors.Count}");
            Console.WriteLine($"Active doctors: {activeDoctors.Count}");
            Console.WriteLine($"Total services: {services.Count}");
            
            foreach (var doctor in allDoctors.Take(5))
            {
                Console.WriteLine($"Doctor: {doctor.FullName}, IsActive: {doctor.IsActive}");
            }

            return View("Create", viewModel);
        }

        // POST: Admin/Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {                    // Kiểm tra xung đột lịch hẹn - tính toán thời gian trước khi query
                    var appointmentStart = model.AppointmentDate.AddMinutes(-30);
                    var appointmentEnd = model.AppointmentDate.AddMinutes(30);
                    
                    var conflictingAppointment = await _context.Appointments
                        .Where(a => a.DoctorId == model.DoctorId && 
                               a.AppointmentDate >= appointmentStart &&
                               a.AppointmentDate <= appointmentEnd &&
                               a.Status != "Cancelled")
                        .FirstOrDefaultAsync();

                    if (conflictingAppointment != null)
                    {
                        ModelState.AddModelError("AppointmentDate", "Bác sĩ đã có lịch hẹn trong khoảng thời gian này.");
                        await LoadDropdownData(model);
                        return View(model);
                    }

                    var appointment = new Appointment
                    {
                        PatientId = model.PatientId,
                        DoctorId = model.DoctorId,
                        ServiceId = model.ServiceId,
                        AppointmentDate = model.AppointmentDate,
                        Status = model.Status,
                        Notes = model.Notes ?? "",
                        CreatedDate = DateTime.Now
                    };

                    _context.Add(appointment);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Tạo lịch hẹn thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            await LoadDropdownData(model);
            return View(model);
        }

        // GET: Admin/Appointment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var viewModel = new AppointmentEditViewModel
            {
                AppointmentId = appointment.AppointmentId,
                PatientId = appointment.PatientId,
                DoctorId = appointment.DoctorId,
                ServiceId = appointment.ServiceId,
                AppointmentDate = appointment.AppointmentDate,
                Status = appointment.Status,
                Notes = appointment.Notes,
                Patients = await _context.Patients.OrderBy(p => p.FirstName).ToListAsync(),
                Doctors = await _context.Doctors.Where(d => d.IsActive).OrderBy(d => d.FirstName).ToListAsync(),
                Services = await _context.Services.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync()
            };

            return View(viewModel);
        }

        // POST: Admin/Appointment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentEditViewModel model)
        {
            if (id != model.AppointmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var appointment = await _context.Appointments.FindAsync(id);
                    if (appointment == null)
                    {
                        return NotFound();
                    }                    // Kiểm tra xung đột lịch hẹn (trừ lịch hẹn hiện tại)
                    var appointmentStart = model.AppointmentDate.AddMinutes(-30);
                    var appointmentEnd = model.AppointmentDate.AddMinutes(30);
                    
                    var conflictingAppointment = await _context.Appointments
                        .Where(a => a.DoctorId == model.DoctorId && 
                               a.AppointmentId != model.AppointmentId &&
                               a.AppointmentDate >= appointmentStart &&
                               a.AppointmentDate <= appointmentEnd &&
                               a.Status != "Cancelled")
                        .FirstOrDefaultAsync();

                    if (conflictingAppointment != null)
                    {
                        ModelState.AddModelError("AppointmentDate", "Bác sĩ đã có lịch hẹn trong khoảng thời gian này.");
                        await LoadDropdownData(model);
                        return View(model);
                    }

                    appointment.PatientId = model.PatientId;
                    appointment.DoctorId = model.DoctorId;
                    appointment.ServiceId = model.ServiceId;
                    appointment.AppointmentDate = model.AppointmentDate;
                    appointment.Status = model.Status;
                    appointment.Notes = model.Notes ?? "";

                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật lịch hẹn thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(model.AppointmentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                    await LoadDropdownData(model);
                    return View(model);
                }
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownData(model);
            return View(model);
        }        // POST: Admin/Appointment/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                // Log để debug
                Console.WriteLine($"UpdateStatus called with id: {id}, status: {status}");
                
                // Kiểm tra nếu id = 0 hoặc âm
                if (id <= 0)
                {
                    Console.WriteLine($"Invalid ID: {id}");
                    return Json(new { success = false, message = $"ID không hợp lệ: {id}" });
                }

                // Kiểm tra status có hợp lệ không
                var validStatuses = new[] { "Pending", "In Progress", "Completed", "Cancelled", "Scheduled", "NoShow" };
                if (string.IsNullOrEmpty(status) || !validStatuses.Contains(status))
                {
                    Console.WriteLine($"Invalid status: {status}");
                    return Json(new { success = false, message = $"Trạng thái không hợp lệ: {status}" });
                }

                // Tìm appointment với include để đảm bảo có đầy đủ thông tin
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Include(a => a.Service)
                    .FirstOrDefaultAsync(a => a.AppointmentId == id);

                if (appointment == null)
                {
                    Console.WriteLine($"Appointment not found for ID: {id}");
                    
                    // Kiểm tra xem có appointment nào trong database không
                    var totalAppointments = await _context.Appointments.CountAsync();
                    var allIds = await _context.Appointments.Select(a => a.AppointmentId).ToListAsync();
                    
                    Console.WriteLine($"Total appointments in DB: {totalAppointments}");
                    Console.WriteLine($"Available IDs: {string.Join(", ", allIds)}");
                    
                    return Json(new { 
                        success = false, 
                        message = $"Không tìm thấy lịch hẹn với ID: {id}. Có {totalAppointments} lịch hẹn trong database.",
                        debug = new { 
                            searchedId = id, 
                            totalAppointments = totalAppointments,
                            availableIds = allIds 
                        }
                    });
                }                Console.WriteLine($"Found appointment: {appointment.AppointmentId} - {appointment.Patient?.FullName} - Current status: {appointment.Status}");                // Cập nhật trạng thái
                var oldStatus = appointment.Status;
                appointment.Status = status;
                
                _context.Update(appointment);
                await _context.SaveChangesAsync();

                // Tự động tạo hóa đơn khi appointment hoàn thành
                if (status == "Completed" && oldStatus != "Completed")
                {
                    try
                    {
                        // Kiểm tra xem đã có hóa đơn cho appointment này chưa
                        var existingInvoice = await _context.Invoices
                            .FirstOrDefaultAsync(i => i.AppointmentId == appointment.AppointmentId);

                        if (existingInvoice == null)
                        {
                            var staffId = await GetCurrentStaffIdAsync();
                            if (staffId.HasValue)
                            {
                                var invoice = await _invoiceService.CreateInvoiceFromAppointmentAsync(
                                    appointment.AppointmentId, 
                                    staffId.Value);
                                
                                Console.WriteLine($"Auto-created invoice {invoice.InvoiceNumber} for completed appointment {appointment.AppointmentId}");
                            }
                            else
                            {
                                Console.WriteLine("Could not determine current staff ID for invoice creation");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Invoice already exists for appointment {appointment.AppointmentId}");
                        }
                    }
                    catch (Exception invoiceEx)
                    {
                        Console.WriteLine($"Failed to create invoice for appointment {appointment.AppointmentId}: {invoiceEx.Message}");
                        // Không làm fail toàn bộ request nếu tạo hóa đơn lỗi
                    }
                }

                // Gửi thông báo cho bệnh nhân
                try
                {
                    await _notificationService.NotifyAppointmentStatusChangeAsync(appointment.AppointmentId, status);
                    Console.WriteLine($"Notification sent for appointment {appointment.AppointmentId}");
                }
                catch (Exception notifEx)
                {
                    Console.WriteLine($"Failed to send notification: {notifEx.Message}");
                    // Không làm fail toàn bộ request nếu gửi thông báo lỗi
                }

                Console.WriteLine($"Successfully updated appointment {id} from {oldStatus} to {status}");

                var responseMessage = $"Cập nhật trạng thái thành công từ '{oldStatus}' sang '{status}'";
                if (status == "Completed" && oldStatus != "Completed")
                {
                    responseMessage += ". Hóa đơn đã được tạo tự động.";
                }

                return Json(new { 
                    success = true, 
                    message = responseMessage
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateStatus: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: Admin/Appointment/UpdateDateTime (for drag & drop)
        [HttpPost]
        public async Task<IActionResult> UpdateDateTime(int id, DateTime newDateTime)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lịch hẹn" });
                }                // Kiểm tra xung đột
                var appointmentStart = newDateTime.AddMinutes(-30);
                var appointmentEnd = newDateTime.AddMinutes(30);
                
                var conflictingAppointment = await _context.Appointments
                    .Where(a => a.DoctorId == appointment.DoctorId && 
                           a.AppointmentId != appointment.AppointmentId &&
                           a.AppointmentDate >= appointmentStart &&
                           a.AppointmentDate <= appointmentEnd &&
                           a.Status != "Cancelled")
                    .FirstOrDefaultAsync();

                if (conflictingAppointment != null)
                {
                    return Json(new { success = false, message = "Bác sĩ đã có lịch hẹn trong khoảng thời gian này" });
                }

                appointment.AppointmentDate = newDateTime;
                _context.Update(appointment);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật thời gian thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Admin/Appointment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Admin/Appointment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment != null)
                {
                    _context.Appointments.Remove(appointment);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Xóa lịch hẹn thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Không thể xóa lịch hẹn: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Appointment/GetServiceInfo
        [HttpGet]
        public async Task<IActionResult> GetServiceInfo(int serviceId)
        {
            try
            {
                var service = await _context.Services.FindAsync(serviceId);
                if (service == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy dịch vụ" });
                }

                return Json(new
                {
                    success = true,
                    price = service.Price,
                    duration = service.Duration ?? 30,
                    description = service.Description
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/Appointment/CheckConflict        [HttpPost]
        public async Task<IActionResult> CheckConflict(int doctorId, DateTime appointmentDate, int? excludeAppointmentId = null)
        {
            try
            {
                var appointmentStart = appointmentDate.AddMinutes(-30);
                var appointmentEnd = appointmentDate.AddMinutes(30);
                
                var query = _context.Appointments
                    .Where(a => a.DoctorId == doctorId &&
                               a.AppointmentDate >= appointmentStart &&
                               a.AppointmentDate <= appointmentEnd &&
                               a.Status != "Cancelled");

                if (excludeAppointmentId.HasValue)
                {
                    query = query.Where(a => a.AppointmentId != excludeAppointmentId.Value);
                }

                var conflictingAppointment = await query
                    .Include(a => a.Patient)
                    .FirstOrDefaultAsync();

                if (conflictingAppointment != null)
                {
                    return Json(new
                    {
                        hasConflict = true,
                        message = $"Bác sĩ đã có lịch hẹn với {conflictingAppointment.Patient.FullName} lúc {conflictingAppointment.AppointmentDate:HH:mm}"
                    });
                }

                return Json(new { hasConflict = false });
            }
            catch (Exception ex)
            {
                return Json(new { hasConflict = true, message = ex.Message });
            }
        }

        // Temporary action to update existing doctors to IsActive = true
        [HttpGet]
        public async Task<IActionResult> UpdateDoctorsActiveStatus()
        {
            try
            {
                var doctors = await _context.Doctors.ToListAsync();
                foreach (var doctor in doctors)
                {
                    doctor.IsActive = true;
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Updated {doctors.Count} doctors to active status" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API để kiểm tra dữ liệu dropdown
        [HttpGet]
        public async Task<IActionResult> GetDropdownData()
        {
            try
            {
                var patients = await _context.Patients.Select(p => new { p.PatientId, p.FullName }).ToListAsync();
                var allDoctors = await _context.Doctors.Select(d => new { d.DoctorId, d.FullName, d.IsActive }).ToListAsync();
                var activeDoctors = await _context.Doctors.Where(d => d.IsActive).Select(d => new { d.DoctorId, d.FullName, d.IsActive }).ToListAsync();
                var services = await _context.Services.Where(s => s.IsActive).Select(s => new { s.ServiceId, s.Name, s.IsActive }).ToListAsync();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        patients = new { count = patients.Count, items = patients },
                        allDoctors = new { count = allDoctors.Count, items = allDoctors },
                        activeDoctors = new { count = activeDoctors.Count, items = activeDoctors },
                        services = new { count = services.Count, items = services }
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // Action để cập nhật tất cả bác sĩ thành IsActive = true
        [HttpGet]
        public async Task<IActionResult> FixDoctorStatus()
        {
            try
            {
                var doctors = await _context.Doctors.ToListAsync();
                foreach (var doctor in doctors)
                {
                    doctor.IsActive = true;
                }
                
                await _context.SaveChangesAsync();
                
                var activeCount = await _context.Doctors.CountAsync(d => d.IsActive);
                
                return Json(new { 
                    success = true, 
                    message = $"Đã cập nhật {doctors.Count} bác sĩ thành IsActive = true. Hiện có {activeCount} bác sĩ active." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Lỗi: {ex.Message}" 
                });
            }
        }

        // Temporary action to seed sample data for testing
        [HttpGet]
        public async Task<IActionResult> SeedData()
        {
            try
            {
                // Check if we already have data
                if (await _context.Patients.AnyAsync() && await _context.Doctors.AnyAsync() && await _context.Services.AnyAsync())
                {
                    return Json(new { success = true, message = "Dữ liệu đã tồn tại" });
                }

                // Create sample patients
                if (!await _context.Patients.AnyAsync())
                {
                    var patients = new[]
                    {
                        new Models.Patient { FirstName = "Nguyễn", LastName = "Văn A", DateOfBirth = new DateTime(1990, 1, 15), Gender = "Nam", Phone = "0123456789", Email = "nguyenvana@email.com", Address = "Hà Nội" },
                        new Models.Patient { FirstName = "Trần", LastName = "Thị B", DateOfBirth = new DateTime(1985, 5, 20), Gender = "Nữ", Phone = "0987654321", Email = "tranthib@email.com", Address = "Hồ Chí Minh" },
                        new Models.Patient { FirstName = "Lê", LastName = "Văn C", DateOfBirth = new DateTime(1978, 12, 10), Gender = "Nam", Phone = "0369258147", Email = "levanc@email.com", Address = "Đà Nẵng" }
                    };
                    _context.Patients.AddRange(patients);
                }

                // Create sample doctors
                if (!await _context.Doctors.AnyAsync())
                {
                    var doctors = new[]
                    {
                        new Models.Doctor { FirstName = "Nguyễn", LastName = "Văn D", DateOfBirth = new DateTime(1975, 3, 15), Gender = "Nam", Specialty = "Nội khoa", Phone = "0123456780", Email = "bsnguyenvand@email.com", DateHired = new DateTime(2020, 1, 1), IsActive = true },
                        new Models.Doctor { FirstName = "Trần", LastName = "Thị E", DateOfBirth = new DateTime(1980, 7, 25), Gender = "Nữ", Specialty = "Nhi khoa", Phone = "0987654320", Email = "bstranthie@email.com", DateHired = new DateTime(2021, 6, 1), IsActive = true },
                        new Models.Doctor { FirstName = "Lê", LastName = "Văn F", DateOfBirth = new DateTime(1970, 11, 5), Gender = "Nam", Specialty = "Ngoại khoa", Phone = "0369258140", Email = "bslevanf@email.com", DateHired = new DateTime(2019, 3, 1), IsActive = true }
                    };
                    _context.Doctors.AddRange(doctors);
                }

                // Create sample services
                if (!await _context.Services.AnyAsync())
                {
                    var services = new[]
                    {
                        new Models.Service { Name = "Khám tổng quát", Description = "Khám sức khỏe tổng quát", Price = 200000, Duration = 30, IsActive = true },
                        new Models.Service { Name = "Khám chuyên khoa", Description = "Khám chuyên khoa theo yêu cầu", Price = 300000, Duration = 45, IsActive = true },
                        new Models.Service { Name = "Xét nghiệm máu", Description = "Xét nghiệm máu cơ bản", Price = 150000, Duration = 15, IsActive = true },
                        new Models.Service { Name = "Siêu âm", Description = "Siêu âm tổng quát", Price = 250000, Duration = 30, IsActive = true }
                    };
                    _context.Services.AddRange(services);
                }

                await _context.SaveChangesAsync();

                // Create sample appointments
                var patient = await _context.Patients.FirstAsync();
                var doctor = await _context.Doctors.FirstAsync();
                var service = await _context.Services.FirstAsync();

                var today = DateTime.Today;
                var appointments = new[]
                {
                    new Models.Appointment 
                    { 
                        PatientId = patient.PatientId, 
                        DoctorId = doctor.DoctorId, 
                        ServiceId = service.ServiceId,
                        AppointmentDate = today.AddHours(9),
                        Status = "Pending",
                        Notes = "Khám định kỳ",
                        CreatedDate = DateTime.Now
                    },
                    new Models.Appointment 
                    { 
                        PatientId = patient.PatientId, 
                        DoctorId = doctor.DoctorId, 
                        ServiceId = service.ServiceId,
                        AppointmentDate = today.AddDays(1).AddHours(10),
                        Status = "In Progress",
                        Notes = "Tái khám",
                        CreatedDate = DateTime.Now
                    },
                    new Models.Appointment 
                    { 
                        PatientId = patient.PatientId, 
                        DoctorId = doctor.DoctorId, 
                        ServiceId = service.ServiceId,
                        AppointmentDate = today.AddDays(-1).AddHours(14),
                        Status = "Completed",
                        Notes = "Đã hoàn thành",
                        CreatedDate = DateTime.Now
                    }
                };

                _context.Appointments.AddRange(appointments);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Tạo dữ liệu mẫu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.AppointmentId == id);
        }

        private async Task LoadDropdownData(AppointmentCreateViewModel model)
        {
            model.Patients = await _context.Patients.OrderBy(p => p.FirstName).ToListAsync();
            model.Doctors = await _context.Doctors.Where(d => d.IsActive).OrderBy(d => d.FirstName).ToListAsync();
            model.Services = await _context.Services.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
        }

        private async Task LoadDropdownData(AppointmentEditViewModel model)
        {
            model.Patients = await _context.Patients.OrderBy(p => p.FirstName).ToListAsync();
            model.Doctors = await _context.Doctors.Where(d => d.IsActive).OrderBy(d => d.FirstName).ToListAsync();
            model.Services = await _context.Services.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
        }

        private string GetStatusColor(string status)
        {
            return status switch
            {
                "Pending" => "#ffc107", // Yellow
                "In Progress" => "#17a2b8", // Blue
                "Completed" => "#28a745", // Green
                "Cancelled" => "#dc3545", // Red
                "No Show" => "#6c757d", // Gray
                _ => "#007bff" // Default blue
            };
        }

        // Debug action để kiểm tra dữ liệu bác sĩ
        public async Task<IActionResult> CheckDoctors()
        {
            var allDoctors = await _context.Doctors.ToListAsync();
            var activeDoctors = await _context.Doctors.Where(d => d.IsActive).ToListAsync();
            
            var result = new
            {
                TotalDoctors = allDoctors.Count,
                ActiveDoctors = activeDoctors.Count,
                AllDoctorsList = allDoctors.Select(d => new { d.DoctorId, d.FirstName, d.LastName, d.IsActive }).ToList(),
                ActiveDoctorsList = activeDoctors.Select(d => new { d.DoctorId, d.FirstName, d.LastName, d.IsActive }).ToList()
            };
            
            return Json(result);
        }

        // Debug action to check appointments
        [HttpGet]
        public async Task<IActionResult> DebugAppointments()
        {
            try
            {
                var appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Include(a => a.Service)
                    .ToListAsync();

                var result = appointments.Select(a => new {
                    Id = a.AppointmentId,
                    PatientName = a.Patient?.FullName ?? "NULL",
                    DoctorName = a.Doctor?.FullName ?? "NULL",
                    ServiceName = a.Service?.Name ?? "NULL",
                    Date = a.AppointmentDate,
                    Status = a.Status,
                    Notes = a.Notes
                }).ToList();

                return Json(new {
                    success = true,
                    count = appointments.Count,
                    appointments = result
                });
            }
            catch (Exception ex)
            {
                return Json(new {
                    success = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // Action to create test appointment
        [HttpGet]
        public async Task<IActionResult> CreateTestAppointment()
        {
            try
            {
                // Kiểm tra xem có patients, doctors, services không
                var patient = await _context.Patients.FirstOrDefaultAsync();
                var doctor = await _context.Doctors.FirstOrDefaultAsync();
                var service = await _context.Services.FirstOrDefaultAsync();

                if (patient == null || doctor == null || service == null)
                {
                    return Json(new {
                        success = false,
                        message = "Cần có ít nhất 1 patient, 1 doctor và 1 service để tạo test appointment",
                        data = new {
                            patients = await _context.Patients.CountAsync(),
                            doctors = await _context.Doctors.CountAsync(),
                            services = await _context.Services.CountAsync()
                        }
                    });
                }

                var testAppointment = new Appointment
                {
                    PatientId = patient.PatientId,
                    DoctorId = doctor.DoctorId,
                    ServiceId = service.ServiceId,
                    AppointmentDate = DateTime.Now.AddDays(1).Date.AddHours(10), // Tomorrow at 10 AM
                    Status = "Pending",
                    Notes = "Test appointment created by admin",
                    CreatedDate = DateTime.Now
                };

                _context.Appointments.Add(testAppointment);
                await _context.SaveChangesAsync();

                return Json(new {
                    success = true,
                    message = "Đã tạo test appointment thành công",
                    appointmentId = testAppointment.AppointmentId,
                    details = new {
                        patient = patient.FullName,
                        doctor = doctor.FullName,
                        service = service.Name,
                        date = testAppointment.AppointmentDate,
                        status = testAppointment.Status
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new {
                    success = false,
                    error = ex.Message
                });
            }
        }        // Debug view
        [HttpGet]
        public IActionResult Debug()
        {
            return View();
        }

        // Helper method to get current staff ID
        private async Task<int?> GetCurrentStaffIdAsync()
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null) return null;

                // Tìm staff record theo email của user hiện tại
                var staff = await _context.Staffs
                    .FirstOrDefaultAsync(s => s.Email == currentUser.Email);

                return staff?.StaffId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting current staff ID: {ex.Message}");
                return null;
            }
        }

        // GET: Admin/Appointment/GetAppointmentsList - For list view
        public async Task<IActionResult> GetAppointmentsList(int? doctorId = null)
        {
            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .AsQueryable();

            if (doctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == doctorId.Value);
            }

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .Take(100) // Limit to 100 recent appointments
                .ToListAsync();

            var result = appointments.Select(a => new
            {
                appointmentId = a.AppointmentId,
                appointmentDate = a.AppointmentDate,
                patientName = a.Patient.FullName,
                doctorName = a.Doctor.FullName,
                serviceName = a.Service.Name,
                status = a.Status,
                notes = a.Notes
            });

            return Json(result);
        }
    }
}
