using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using G8_HospitalManagerment_Project_PRN222.Models;

namespace G8_HospitalManagerment_Project_PRN222.Controllers.PharmacyController
{
    public class PrescriptionItemsController : Controller
    {
        private readonly DbHospitalManagementContext _context;

        public PrescriptionItemsController(DbHospitalManagementContext context)
        {
            _context = context;
        }

        // GET: PrescriptionItems
        public async Task<IActionResult> Index()
        {
            var dbHospitalManagementContext = _context.PrescriptionItems.Include(p => p.Drug).Include(p => p.Prescription);
            return View(await dbHospitalManagementContext.ToListAsync());
        }

        // GET: PrescriptionItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescriptionItem = await _context.PrescriptionItems
                .Include(p => p.Drug)
                .Include(p => p.Prescription)
                .FirstOrDefaultAsync(m => m.PrescriptionItemId == id);
            if (prescriptionItem == null)
            {
                return NotFound();
            }

            return View(prescriptionItem);
        }

        // GET: PrescriptionItems/Create
        public IActionResult Create()
        {
            ViewData["DrugId"] = new SelectList(_context.Drugs, "DrugId", "DrugId");
            ViewData["PrescriptionId"] = new SelectList(_context.Prescriptions, "PrescriptionId", "PrescriptionId");
            return View();
        }

        // POST: PrescriptionItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PrescriptionItemId,PrescriptionId,DrugId,Quantity,Dosage,DurationDays,UsageInstructions,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] PrescriptionItem prescriptionItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(prescriptionItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DrugId"] = new SelectList(_context.Drugs, "DrugId", "DrugId", prescriptionItem.DrugId);
            ViewData["PrescriptionId"] = new SelectList(_context.Prescriptions, "PrescriptionId", "PrescriptionId", prescriptionItem.PrescriptionId);
            return View(prescriptionItem);
        }

        // GET: PrescriptionItems/Edit/5
        // BUSINESS RULE: Individual prescription items are immutable after creation.
        public async Task<IActionResult> Edit(int? id)
        {
            // Fetch the item only to know which prescription to redirect back to
            var item = id.HasValue ? await _context.PrescriptionItems.FindAsync(id.Value) : null;
            int? prescriptionId = item?.PrescriptionId;

            TempData["ErrorMessage"] = "Medical regulations prohibit editing a finalized prescription. Please delete and create a new one if necessary.";

            if (prescriptionId.HasValue)
                return RedirectToAction("Details", "Prescriptions", new { id = prescriptionId.Value });

            return RedirectToAction("Index", "Prescriptions");
        }

        // POST: PrescriptionItems/Edit/5
        // BUSINESS RULE: Individual prescription items are immutable after creation.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PrescriptionItem prescriptionItem)
        {
            var item = await _context.PrescriptionItems.FindAsync(id);
            int? prescriptionId = item?.PrescriptionId;

            TempData["ErrorMessage"] = "Medical regulations prohibit editing a finalized prescription. Please delete and create a new one if necessary.";

            if (prescriptionId.HasValue)
                return RedirectToAction("Details", "Prescriptions", new { id = prescriptionId.Value });

            return RedirectToAction("Index", "Prescriptions");
        }

        // GET: PrescriptionItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescriptionItem = await _context.PrescriptionItems
                .Include(p => p.Drug)
                .Include(p => p.Prescription)
                .FirstOrDefaultAsync(m => m.PrescriptionItemId == id);
            if (prescriptionItem == null)
            {
                return NotFound();
            }

            return View(prescriptionItem);
        }

        // POST: PrescriptionItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prescriptionItem = await _context.PrescriptionItems.FindAsync(id);
            int? prescriptionId = prescriptionItem?.PrescriptionId;

            if (prescriptionItem != null)
            {
                // Soft delete — keep for audit trail
                prescriptionItem.IsDeleted = true;
                prescriptionItem.DeletedAt = DateTime.Now;
                _context.Update(prescriptionItem);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Drug removed from prescription successfully.";

            if (prescriptionId.HasValue)
                return RedirectToAction("Details", "Prescriptions", new { id = prescriptionId.Value });

            return RedirectToAction("Index", "Prescriptions");
        }

        private bool PrescriptionItemExists(int id)
        {
            return _context.PrescriptionItems.Any(e => e.PrescriptionItemId == id);
        }
    }
}
