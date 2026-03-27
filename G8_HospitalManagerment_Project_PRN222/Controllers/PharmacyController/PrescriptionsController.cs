using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using G8_HospitalManagerment_Project_PRN222.Models;
using G8_HospitalManagerment_Project_PRN222.ViewModels;

namespace G8_HospitalManagerment_Project_PRN222.Controllers.PharmacyController
{
    public class PrescriptionsController : Controller
    {
        private readonly DbHospitalManagementContext _context;

        public PrescriptionsController(DbHospitalManagementContext context)
        {
            _context = context;
        }

        // =====================================================================
        // FEATURE 3 — Kê đơn thuốc điện tử (Add E-Prescription)
        // GET: Prescriptions/Create?medicalRecordId=5
        // =====================================================================

        [HttpGet]
        public async Task<IActionResult> Create(int medicalRecordId)
        {
            var record = await _context.MedicalRecords
                .Include(r => r.Doctor).ThenInclude(d => d.Employee).ThenInclude(e => e.User)
                .Include(r => r.Patient).ThenInclude(p => p.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RecordId == medicalRecordId && r.IsDeleted != true);

            if (record == null)
            {
                TempData["Error"] = $"Không tìm thấy hồ sơ bệnh án #{medicalRecordId}.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new PrescriptionCreateViewModel
            {
                MedicalRecordId = medicalRecordId,
                RecordDate      = record.RecordDate,
                PatientName     = record.Patient?.User != null
                                      ? $"{record.Patient.User.FirstName} {record.Patient.User.LastName}"
                                      : $"Patient #{record.PatientId}",
                DoctorName      = record.Doctor?.Employee?.User != null
                                      ? $"{record.Doctor.Employee.User.FirstName} {record.Doctor.Employee.User.LastName}"
                                      : $"Doctor #{record.DoctorId}"
            };

            await PopulateDrugSelectList();
            return View(vm);
        }

        // POST: Prescriptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PrescriptionCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDrugSelectList();
                return View(model);
            }

            // Verify MedicalRecord exists and pull PatientId/DoctorId
            var record = await _context.MedicalRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RecordId == model.MedicalRecordId && r.IsDeleted != true);

            if (record == null)
            {
                ModelState.AddModelError("", "Hồ sơ bệnh án không tồn tại.");
                await PopulateDrugSelectList();
                return View(model);
            }

            // Mandatory transaction for the entire function
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // STEP 1 · Create Prescription header
                var prescription = new Prescription
                {
                    MedicalRecordId  = model.MedicalRecordId,
                    PatientId        = record.PatientId,
                    DoctorId         = record.DoctorId,
                    PrescriptionDate = DateTime.Now,
                    DoctorAdvice     = model.DoctorAdvice,
                    CreatedAt        = DateTime.Now,
                    UpdatedAt        = DateTime.Now,
                    IsDeleted        = false
                };

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();   // auto-generates PrescriptionId
                int prescriptionId = prescription.PrescriptionId;

                // STEP 2 · Process each drug item
                foreach (var item in model.Items)
                {
                    var drug = await _context.Drugs
                        .FirstOrDefaultAsync(d => d.DrugId == item.DrugId && d.IsDeleted != true);

                    if (drug == null)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", $"Không tìm thấy thuốc với ID = {item.DrugId}.");
                        await PopulateDrugSelectList();
                        return View(model);
                    }

                    // Stock check — rollback and stay on the page to show error
                    if (item.Quantity > (drug.StockQuantity ?? 0))
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("",
                            $"Thuốc {drug.DrugName} không đủ hàng trong kho " +
                            $"(còn {drug.StockQuantity ?? 0}, yêu cầu {item.Quantity}).");
                        await PopulateDrugSelectList();
                        return View(model);
                    }

                    // Insert PrescriptionItem
                    _context.PrescriptionItems.Add(new PrescriptionItem
                    {
                        PrescriptionId    = prescriptionId,
                        DrugId            = item.DrugId,
                        Quantity          = item.Quantity,
                        Dosage            = item.Dosage,
                        DurationDays      = item.DurationDays,
                        UsageInstructions = item.UsageInstructions,
                        CreatedAt         = DateTime.Now,
                        UpdatedAt         = DateTime.Now,
                        IsDeleted         = false
                    });

                    // Decrement stock
                    drug.StockQuantity -= item.Quantity;
                    drug.UpdatedAt      = DateTime.Now;
                    _context.Drugs.Update(drug);
                }

                // STEP 3 · Commit
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Fire placeholder notification (outside transaction)
                await GenerateQRAndNotifyPatient(prescriptionId);

                TempData["Success"] = "Đơn thuốc đã được kê thành công. Tồn kho đã được cập nhật.";
                return RedirectToAction("Details", "MedicalRecords",
                    new { id = model.MedicalRecordId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Lỗi hệ thống: {ex.Message}");
                await PopulateDrugSelectList();
                return View(model);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task PopulateDrugSelectList()
        {
            var drugs = await _context.Drugs
                .Where(d => d.IsDeleted != true && d.StockQuantity > 0)
                .OrderBy(d => d.DrugName)
                .Select(d => new { d.DrugId, Display = $"{d.DrugName} (còn {d.StockQuantity} {d.Unit})" })
                .ToListAsync();

            ViewBag.DrugList = new SelectList(drugs, "DrugId", "Display");
        }

        /// <summary>
        /// Placeholder for QR generation and Email/SMS notification.
        /// Replace the TODO comments with real service calls.
        /// </summary>
        private async Task GenerateQRAndNotifyPatient(int prescriptionId)
        {
            // TODO: var qrBytes = QrCodeService.Generate($"/prescriptions/{prescriptionId}");
            // TODO: await _emailService.SendAsync(patient.Email, "Đơn thuốc của bạn", body);
            // TODO: await _smsService.SendAsync(patient.Phone, $"Đơn thuốc #{prescriptionId} đã sẵn sàng.");
            await Task.CompletedTask;
        }

        // =====================================================================
        // Original Scaffolded CRUD Actions (unchanged)
        // =====================================================================

        // GET: Prescriptions
        public async Task<IActionResult> Index()
        {
            var prescriptions = _context.Prescriptions
                .Where(p => p.IsDeleted != true)
                .Include(p => p.Patient)
                    .ThenInclude(p => p.User)
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.Employee)
                        .ThenInclude(e => e.User)
                .Include(p => p.MedicalRecord)
                .Include(p => p.PrescriptionItems)
                .OrderByDescending(p => p.PrescriptionDate);
            return View(await prescriptions.ToListAsync());
        }

        // GET: Prescriptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                    .ThenInclude(p => p.User)
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.Employee)
                        .ThenInclude(e => e.User)
                .Include(p => p.MedicalRecord)
                .Include(p => p.PrescriptionItems.Where(pi => pi.IsDeleted != true))
                    .ThenInclude(pi => pi.Drug)
                .FirstOrDefaultAsync(m => m.PrescriptionId == id);

            if (prescription == null)
                return NotFound();

            return View(prescription);
        }

        // GET: Prescriptions/Edit/5
        // BUSINESS RULE: Prescriptions are immutable after creation.
        public IActionResult Edit(int? id)
        {
            TempData["ErrorMessage"] = "Medical regulations prohibit editing a finalized prescription. Please delete and create a new one if necessary.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Prescriptions/Edit/5
        // BUSINESS RULE: Prescriptions are immutable after creation.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Prescription prescription)
        {
            TempData["ErrorMessage"] = "Medical regulations prohibit editing a finalized prescription. Please delete and create a new one if necessary.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Prescriptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var prescription = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.MedicalRecord)
                .Include(p => p.Patient)
                .FirstOrDefaultAsync(m => m.PrescriptionId == id);

            if (prescription == null)
                return NotFound();

            return View(prescription);
        }

        // POST: Prescriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription != null)
            {
                // Soft delete — preserve the record for audit/billing history
                prescription.IsDeleted = true;
                prescription.DeletedAt = DateTime.Now;
                _context.Update(prescription);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Prescription deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
