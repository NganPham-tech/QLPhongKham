using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;
using QLPhongKham.ViewModels;
using QLPhongKham.Repositories;

namespace QLPhongKham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Doctor")]
    public class MedicalRecordController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMedicalRecordRepository _medicalRecordRepository;

        public MedicalRecordController(
            ApplicationDbContext context,
            IMedicalRecordRepository medicalRecordRepository)
        {
            _context = context;
            _medicalRecordRepository = medicalRecordRepository;
        }

        // GET: Admin/MedicalRecord
        public async Task<IActionResult> Index(MedicalRecordSearchViewModel model)
        {
            var query = _context.MedicalRecords
                .Include(m => m.Patient)
                .Include(m => m.Doctor)
                .Include(m => m.Appointment)
                    .ThenInclude(a => a.Service)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(model.PatientName))
            {
                query = query.Where(m => m.Patient.FirstName.Contains(model.PatientName) || 
                                        m.Patient.LastName.Contains(model.PatientName));
            }

            if (!string.IsNullOrEmpty(model.PatientPhone))
            {
                query = query.Where(m => m.Patient.Phone.Contains(model.PatientPhone));
            }

            if (model.FromDate.HasValue)
            {
                query = query.Where(m => m.CreatedDate >= model.FromDate.Value);
            }

            if (model.ToDate.HasValue)
            {
                query = query.Where(m => m.CreatedDate <= model.ToDate.Value.AddDays(1));
            }

            if (model.DoctorId.HasValue)
            {
                query = query.Where(m => m.DoctorId == model.DoctorId.Value);
            }

            if (!string.IsNullOrEmpty(model.Diagnosis))
            {
                query = query.Where(m => m.Diagnosis.Contains(model.Diagnosis));
            }

            // Calculate pagination
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / model.PageSize);

            var records = await query
                .OrderByDescending(m => m.CreatedDate)
                .Skip((model.CurrentPage - 1) * model.PageSize)
                .Take(model.PageSize)
                .Select(m => new MedicalRecordDetailViewModel
                {
                    MedicalRecordId = m.MedicalRecordId,
                    CreatedDate = m.CreatedDate,
                    DoctorName = m.Doctor != null ? m.Doctor.FullName : "Chưa có",
                    Diagnosis = m.Diagnosis,
                    Treatment = m.Treatment,
                    Notes = m.Notes,
                    Prescription = m.Prescription,
                    Advice = m.Advice,
                    ServiceName = m.Appointment.Service.Name
                })
                .ToListAsync();

            model.Results = records;
            model.TotalRecords = totalRecords;
            model.TotalPages = totalPages;
            model.AvailableDoctors = await _context.Doctors.Where(d => d.IsActive).ToListAsync();

            return View(model);
        }

        // GET: Admin/MedicalRecord/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Patient)
                .Include(m => m.Doctor)
                .Include(m => m.Appointment)
                    .ThenInclude(a => a.Service)
                .FirstOrDefaultAsync(m => m.MedicalRecordId == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            var model = new MedicalRecordDetailViewModel
            {
                MedicalRecordId = medicalRecord.MedicalRecordId,
                CreatedDate = medicalRecord.CreatedDate,
                DoctorName = medicalRecord.Doctor?.FullName ?? "Chưa có",
                Diagnosis = medicalRecord.Diagnosis,
                Treatment = medicalRecord.Treatment,
                Notes = medicalRecord.Notes,
                Prescription = medicalRecord.Prescription,
                Advice = medicalRecord.Advice,
                ServiceName = medicalRecord.Appointment.Service.Name
            };

            ViewData["PatientInfo"] = medicalRecord.Patient;
            return View(model);
        }

        // GET: Admin/MedicalRecord/Create
        public async Task<IActionResult> Create(int? appointmentId)
        {
            var model = new CreateMedicalRecordViewModel();

            if (appointmentId.HasValue)
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Service)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId.Value);

                if (appointment != null)
                {
                    model.AppointmentId = appointment.AppointmentId;
                    model.PatientName = appointment.Patient.FullName;
                    model.PatientPhone = appointment.Patient.Phone;
                    model.PatientDateOfBirth = appointment.Patient.DateOfBirth;
                    model.PatientGender = appointment.Patient.Gender;
                    
                    model.MedicalRecord.AppointmentId = appointment.AppointmentId;
                    model.MedicalRecord.PatientId = appointment.PatientId;
                }
            }

            // Get available appointments that don't have medical records yet
            model.AvailableAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Service)
                .Where(a => a.Status == "Completed" && 
                           !_context.MedicalRecords.Any(m => m.AppointmentId == a.AppointmentId))
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            model.AvailableDoctors = await _context.Doctors
                .Where(d => d.IsActive)
                .ToListAsync();

            return View(model);
        }

        // POST: Admin/MedicalRecord/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMedicalRecordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if medical record already exists for this appointment
                var existingRecord = await _context.MedicalRecords
                    .FirstOrDefaultAsync(m => m.AppointmentId == model.MedicalRecord.AppointmentId);

                if (existingRecord != null)
                {
                    ModelState.AddModelError("", "Lịch hẹn này đã có hồ sơ bệnh án.");
                }
                else
                {
                    // Get the appointment and patient objects
                    var appointment = await _context.Appointments
                        .Include(a => a.Patient)
                        .FirstOrDefaultAsync(a => a.AppointmentId == model.MedicalRecord.AppointmentId);

                    var patient = await _context.Patients.FindAsync(model.MedicalRecord.PatientId);

                    if (appointment == null || patient == null)
                    {
                        ModelState.AddModelError("", "Không tìm thấy thông tin lịch hẹn hoặc bệnh nhân.");
                    }
                    else
                    {
                        var medicalRecord = new MedicalRecord
                        {
                            AppointmentId = model.MedicalRecord.AppointmentId,
                            PatientId = model.MedicalRecord.PatientId,
                            Diagnosis = model.MedicalRecord.Diagnosis,
                            Treatment = model.MedicalRecord.Treatment,
                            Notes = model.MedicalRecord.Notes,
                            DoctorId = model.MedicalRecord.DoctorId,
                            Prescription = model.MedicalRecord.Prescription,
                            Advice = model.MedicalRecord.Advice,
                            CreatedDate = DateTime.Now,
                            Appointment = appointment,
                            Patient = patient
                        };

                        _context.MedicalRecords.Add(medicalRecord);
                        await _context.SaveChangesAsync();

                        TempData["Success"] = "Tạo hồ sơ bệnh án thành công!";
                        return RedirectToAction(nameof(Details), new { id = medicalRecord.MedicalRecordId });
                    }
                }
            }

            // Reload data if validation fails
            model.AvailableAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Service)
                .Where(a => a.Status == "Completed" && 
                           !_context.MedicalRecords.Any(m => m.AppointmentId == a.AppointmentId))
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            model.AvailableDoctors = await _context.Doctors
                .Where(d => d.IsActive)
                .ToListAsync();

            return View(model);
        }

        // GET: Admin/MedicalRecord/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Patient)
                .Include(m => m.Appointment)
                    .ThenInclude(a => a.Service)
                .FirstOrDefaultAsync(m => m.MedicalRecordId == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            var model = new CreateMedicalRecordViewModel
            {
                AppointmentId = medicalRecord.AppointmentId,
                PatientName = medicalRecord.Patient.FullName,
                PatientPhone = medicalRecord.Patient.Phone,
                PatientDateOfBirth = medicalRecord.Patient.DateOfBirth,
                PatientGender = medicalRecord.Patient.Gender,
                MedicalRecord = new MedicalRecordViewModel
                {
                    MedicalRecordId = medicalRecord.MedicalRecordId,
                    AppointmentId = medicalRecord.AppointmentId,
                    PatientId = medicalRecord.PatientId,
                    Diagnosis = medicalRecord.Diagnosis,
                    Treatment = medicalRecord.Treatment,
                    Notes = medicalRecord.Notes,
                    DoctorId = medicalRecord.DoctorId,
                    Prescription = medicalRecord.Prescription,
                    Advice = medicalRecord.Advice,
                    CreatedDate = medicalRecord.CreatedDate
                }
            };

            model.AvailableDoctors = await _context.Doctors
                .Where(d => d.IsActive)
                .ToListAsync();

            return View(model);
        }

        // POST: Admin/MedicalRecord/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateMedicalRecordViewModel model)
        {
            if (id != model.MedicalRecord.MedicalRecordId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var medicalRecord = await _context.MedicalRecords.FindAsync(id);
                    if (medicalRecord == null)
                    {
                        return NotFound();
                    }

                    medicalRecord.Diagnosis = model.MedicalRecord.Diagnosis;
                    medicalRecord.Treatment = model.MedicalRecord.Treatment;
                    medicalRecord.Notes = model.MedicalRecord.Notes;
                    medicalRecord.DoctorId = model.MedicalRecord.DoctorId;
                    medicalRecord.Prescription = model.MedicalRecord.Prescription;
                    medicalRecord.Advice = model.MedicalRecord.Advice;

                    _context.Update(medicalRecord);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Cập nhật hồ sơ bệnh án thành công!";
                    return RedirectToAction(nameof(Details), new { id = medicalRecord.MedicalRecordId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await MedicalRecordExists(model.MedicalRecord.MedicalRecordId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Reload data if validation fails
            model.AvailableDoctors = await _context.Doctors
                .Where(d => d.IsActive)
                .ToListAsync();

            return View(model);
        }

        // GET: Admin/MedicalRecord/PatientHistory/5
        public async Task<IActionResult> PatientHistory(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound();
            }

            var medicalRecords = await _context.MedicalRecords
                .Include(m => m.Doctor)
                .Include(m => m.Appointment)
                    .ThenInclude(a => a.Service)
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.CreatedDate)
                .Select(m => new MedicalRecordDetailViewModel
                {
                    MedicalRecordId = m.MedicalRecordId,
                    CreatedDate = m.CreatedDate,
                    DoctorName = m.Doctor != null ? m.Doctor.FullName : "Chưa có",
                    Diagnosis = m.Diagnosis,
                    Treatment = m.Treatment,
                    Notes = m.Notes,
                    Prescription = m.Prescription,
                    Advice = m.Advice,
                    ServiceName = m.Appointment.Service.Name
                })
                .ToListAsync();

            var model = new PatientMedicalHistoryViewModel
            {
                Patient = patient,
                MedicalRecords = medicalRecords,
                TotalRecords = medicalRecords.Count,
                LastVisit = medicalRecords.FirstOrDefault()?.CreatedDate,
                LastDiagnosis = medicalRecords.FirstOrDefault()?.Diagnosis
            };

            return View(model);
        }

        // GET: Admin/MedicalRecord/Print/5
        public async Task<IActionResult> Print(int id)
        {
            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Patient)
                .Include(m => m.Doctor)
                .Include(m => m.Appointment)
                    .ThenInclude(a => a.Service)
                .FirstOrDefaultAsync(m => m.MedicalRecordId == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            var model = new MedicalRecordDetailViewModel
            {
                MedicalRecordId = medicalRecord.MedicalRecordId,
                CreatedDate = medicalRecord.CreatedDate,
                DoctorName = medicalRecord.Doctor?.FullName ?? "Chưa có",
                Diagnosis = medicalRecord.Diagnosis,
                Treatment = medicalRecord.Treatment,
                Notes = medicalRecord.Notes,
                Prescription = medicalRecord.Prescription,
                Advice = medicalRecord.Advice,
                ServiceName = medicalRecord.Appointment.Service.Name
            };

            ViewData["PatientInfo"] = medicalRecord.Patient;
            return View(model);
        }

        // AJAX: Get appointment details
        [HttpGet]
        public async Task<IActionResult> GetAppointmentDetails(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            return Json(new
            {
                patientId = appointment.PatientId,
                patientName = appointment.Patient.FullName,
                patientPhone = appointment.Patient.Phone,
                patientDateOfBirth = appointment.Patient.DateOfBirth.ToString("dd/MM/yyyy"),
                patientGender = appointment.Patient.Gender,
                serviceName = appointment.Service.Name,
                appointmentDate = appointment.AppointmentDate.ToString("dd/MM/yyyy HH:mm")
            });
        }

        private async Task<bool> MedicalRecordExists(int id)
        {
            return await _context.MedicalRecords.AnyAsync(e => e.MedicalRecordId == id);
        }
    }
}
