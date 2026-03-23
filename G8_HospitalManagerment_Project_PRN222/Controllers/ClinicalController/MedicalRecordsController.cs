using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using G8_HospitalManagerment_Project_PRN222.Models;
using G8_HospitalManagerment_Project_PRN222.Models.ViewModels;

namespace G8_HospitalManagerment_Project_PRN222.Controllers.ClinicalController
{
    public class MedicalRecordsController : Controller
    {
        private readonly DbHospitalManagementContext _context;

        public MedicalRecordsController(DbHospitalManagementContext context)
        {
            _context = context;
        }

        // =====================================================================
        // FEATURE 1 — Xem chi tiết hồ sơ bệnh án
        // GET: MedicalRecords/Details/5
        // =====================================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var record = await _context.MedicalRecords
                .Include(r => r.Doctor)
                    .ThenInclude(d => d.Employee)
                        .ThenInclude(e => e.User)
                .Include(r => r.Patient)
                    .ThenInclude(p => p.User)
                .Include(r => r.Prescriptions)
                    .ThenInclude(p => p.PrescriptionItems)
                        .ThenInclude(pi => pi.Drug)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RecordId == id && r.IsDeleted != true);

            if (record == null)
                return NotFound();

            var vm = new MedicalRecordDetailsViewModel
            {
                RecordId             = record.RecordId,
                RecordDate           = record.RecordDate,
                Diagnosis            = record.Diagnosis,
                Symptoms             = record.Symptoms,
                Treatment            = record.Treatment,
                DoctorId             = record.DoctorId,
                DoctorName           = record.Doctor?.Employee?.User != null
                                         ? $"{record.Doctor.Employee.User.FirstName} {record.Doctor.Employee.User.LastName}"
                                         : "N/A",
                DoctorSpecialization = record.Doctor?.Specialization,
                PatientId            = record.PatientId,
                PatientName          = record.Patient?.User != null
                                         ? $"{record.Patient.User.FirstName} {record.Patient.User.LastName}"
                                         : "N/A",
                Prescriptions = record.Prescriptions
                    .Where(p => p.IsDeleted != true)
                    .Select(p => new PrescriptionDetailBlock
                    {
                        PrescriptionId   = p.PrescriptionId,
                        PrescriptionDate = p.PrescriptionDate,
                        DoctorAdvice     = p.DoctorAdvice,
                        Drugs = p.PrescriptionItems
                            .Where(pi => pi.IsDeleted != true)
                            .Select(pi => new DrugDetailRow
                            {
                                DrugName          = pi.Drug?.DrugName ?? "N/A",
                                Dosage            = pi.Dosage,
                                Quantity          = pi.Quantity,
                                Unit              = pi.Drug?.Unit,
                                DurationDays      = pi.DurationDays,
                                UsageInstructions = pi.UsageInstructions
                            }).ToList()
                    }).ToList()
            };

            return View(vm);
        }

        // =====================================================================
        // FEATURE 2 — Thêm hồ sơ bệnh án
        // GET: MedicalRecords/Create?appointmentId=1&patientId=2&doctorId=3
        // =====================================================================

        [HttpGet]
        public async Task<IActionResult> Create(int appointmentId, int patientId, int doctorId)
        {
            // Load appointment date + patient name + doctor name for display
            var appointment = await _context.Appointments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            var patient = await _context.Patients
                .Include(p => p.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            var doctor = await _context.Doctors
                .Include(d => d.Employee).ThenInclude(e => e.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            var vm = new MedicalRecordCreateViewModel
            {
                AppointmentId  = appointmentId,
                PatientId      = patientId,
                DoctorId       = doctorId,
                AppointmentDate = appointment?.AppointmentDate,
                PatientName    = patient?.User != null
                                    ? $"{patient.User.FirstName} {patient.User.LastName}"
                                    : $"Patient #{patientId}",
                DoctorName     = doctor?.Employee?.User != null
                                    ? $"{doctor.Employee.User.FirstName} {doctor.Employee.User.LastName}"
                                    : $"Doctor #{doctorId}"
            };

            return View(vm);
        }

        // POST: MedicalRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicalRecordCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Insert new MedicalRecord
                var newRecord = new MedicalRecord
                {
                    AppointmentId = model.AppointmentId,
                    PatientId     = model.PatientId,
                    DoctorId      = model.DoctorId,
                    Diagnosis     = model.Diagnosis,
                    Symptoms      = model.Symptoms,
                    Treatment     = model.Treatment,
                    RecordDate    = DateTime.Now,
                    CreatedAt     = DateTime.Now,
                    UpdatedAt     = DateTime.Now,
                    IsDeleted     = false
                };

                _context.MedicalRecords.Add(newRecord);
                await _context.SaveChangesAsync();  // RecordId populated here

                // 2. Mark Appointment as Completed
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.AppointmentId == model.AppointmentId);

                if (appointment != null)
                {
                    appointment.Status    = "Completed";
                    appointment.UpdatedAt = DateTime.Now;
                    _context.Appointments.Update(appointment);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                TempData["Success"] = "Hồ sơ bệnh án đã được tạo thành công.";

                // Redirect to prescription creation with the new RecordId
                return RedirectToAction("Create", "Prescriptions",
                    new { medicalRecordId = newRecord.RecordId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                return View(model);
            }
        }

        // =====================================================================
        // Original Scaffolded CRUD Actions (unchanged except Details replaced above)
        // =====================================================================

        // GET: MedicalRecords
        public async Task<IActionResult> Index()
        {
            var records = _context.MedicalRecords
                .Include(m => m.Appointment)
                .Include(m => m.Doctor)
                .Include(m => m.Patient);
            return View(await records.ToListAsync());
        }

        // GET: MedicalRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord == null)
                return NotFound();

            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "AppointmentId", "AppointmentId", medicalRecord.AppointmentId);
            ViewData["DoctorId"]      = new SelectList(_context.Doctors, "DoctorId", "DoctorId", medicalRecord.DoctorId);
            ViewData["PatientId"]     = new SelectList(_context.Patients, "PatientId", "PatientId", medicalRecord.PatientId);
            return View(medicalRecord);
        }

        // POST: MedicalRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RecordId,AppointmentId,PatientId,DoctorId,Diagnosis,Symptoms,Treatment,RecordDate,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] MedicalRecord medicalRecord)
        {
            if (id != medicalRecord.RecordId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medicalRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.MedicalRecords.Any(e => e.RecordId == medicalRecord.RecordId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "AppointmentId", "AppointmentId", medicalRecord.AppointmentId);
            ViewData["DoctorId"]      = new SelectList(_context.Doctors, "DoctorId", "DoctorId", medicalRecord.DoctorId);
            ViewData["PatientId"]     = new SelectList(_context.Patients, "PatientId", "PatientId", medicalRecord.PatientId);
            return View(medicalRecord);
        }

        // GET: MedicalRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Appointment)
                .Include(m => m.Doctor)
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.RecordId == id);

            if (medicalRecord == null)
                return NotFound();

            return View(medicalRecord);
        }

        // POST: MedicalRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord != null)
                _context.MedicalRecords.Remove(medicalRecord);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
