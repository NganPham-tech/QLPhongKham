using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLPhongKham.Models;
using QLPhongKham.Repositories;

namespace QLPhongKham.Areas.NhanVien
{
    [Area("NhanVien")]
    [Authorize(Roles = "NhanVien")]
    public class PatientController : Controller
    {
        private readonly IPatientRepository _patientRepo;

        public PatientController(IPatientRepository patientRepo)
        {
            _patientRepo = patientRepo;
        }

        public IActionResult Index(string searchTerm)
        {
            var patients = string.IsNullOrEmpty(searchTerm)
                ? _patientRepo.GetAll().ToList()
                : _patientRepo.SearchPatients(searchTerm).ToList();

            ViewBag.SearchTerm = searchTerm;
            return View(patients);
        }

        public IActionResult Details(int id)
        {
            var patient = _patientRepo.GetById(id);
            if (patient == null) return NotFound();

            return View(patient);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Patient());
        }

        [HttpPost]
        public IActionResult Create(Patient model)
        {
            if (ModelState.IsValid)
            {
                _patientRepo.Add(model);
                TempData["Success"] = "Thêm bệnh nhân thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var patient = _patientRepo.GetById(id);
            if (patient == null) return NotFound();

            return View(patient);
        }

        [HttpPost]
        public IActionResult Edit(Patient model)
        {
            if (ModelState.IsValid)
            {
                _patientRepo.Update(model);
                TempData["Success"] = "Cập nhật thông tin bệnh nhân thành công!";
                return RedirectToAction(nameof(Details), new { id = model.PatientId });
            }

            return View(model);
        }
    }
}
