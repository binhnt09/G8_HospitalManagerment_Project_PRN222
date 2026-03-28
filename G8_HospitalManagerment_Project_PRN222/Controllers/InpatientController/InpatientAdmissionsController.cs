using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using G8_HospitalManagerment_Project_PRN222.Models;

namespace G8_HospitalManagerment_Project_PRN222.Controllers.InpatientController
{
    public class InpatientAdmissionsController : Controller
    {
        private readonly DbHospitalManagementContext _context;

        public InpatientAdmissionsController(DbHospitalManagementContext context)
        {
            _context = context;
        }

        // GET: InpatientAdmissions
        public async Task<IActionResult> Index()
        {
            var dbHospitalManagementContext = _context.InpatientAdmissions.Include(i => i.Bed).Include(i => i.Doctor).Include(i => i.Patient);
            return View(await dbHospitalManagementContext.ToListAsync());
        }

        // GET: InpatientAdmissions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inpatientAdmission = await _context.InpatientAdmissions
                .Include(i => i.Bed)
                .Include(i => i.Doctor)
                .Include(i => i.Patient)
                .FirstOrDefaultAsync(m => m.AdmissionId == id);
            if (inpatientAdmission == null)
            {
                return NotFound();
            }

            return View(inpatientAdmission);
        }

        // GET: InpatientAdmissions/Create
        public IActionResult Create()
        {
            ViewData["BedId"] = new SelectList(_context.Beds, "BedId", "BedId");
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId");
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId");
            return View();
        }

        // POST: InpatientAdmissions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AdmissionId,PatientId,DoctorId,BedId,AdmissionDate,DischargeDate,Status,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] InpatientAdmission inpatientAdmission)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inpatientAdmission);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BedId"] = new SelectList(_context.Beds, "BedId", "BedId", inpatientAdmission.BedId);
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId", inpatientAdmission.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", inpatientAdmission.PatientId);
            return View(inpatientAdmission);
        }

        // GET: InpatientAdmissions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inpatientAdmission = await _context.InpatientAdmissions.FindAsync(id);
            if (inpatientAdmission == null)
            {
                return NotFound();
            }
            ViewData["BedId"] = new SelectList(_context.Beds, "BedId", "BedId", inpatientAdmission.BedId);
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId", inpatientAdmission.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", inpatientAdmission.PatientId);
            return View(inpatientAdmission);
        }

        // POST: InpatientAdmissions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AdmissionId,PatientId,DoctorId,BedId,AdmissionDate,DischargeDate,Status,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] InpatientAdmission inpatientAdmission)
        {
            if (id != inpatientAdmission.AdmissionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inpatientAdmission);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InpatientAdmissionExists(inpatientAdmission.AdmissionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BedId"] = new SelectList(_context.Beds, "BedId", "BedId", inpatientAdmission.BedId);
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId", inpatientAdmission.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", inpatientAdmission.PatientId);
            return View(inpatientAdmission);
        }

        // GET: InpatientAdmissions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inpatientAdmission = await _context.InpatientAdmissions
                .Include(i => i.Bed)
                .Include(i => i.Doctor)
                .Include(i => i.Patient)
                .FirstOrDefaultAsync(m => m.AdmissionId == id);
            if (inpatientAdmission == null)
            {
                return NotFound();
            }

            return View(inpatientAdmission);
        }

        // POST: InpatientAdmissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inpatientAdmission = await _context.InpatientAdmissions.FindAsync(id);
            if (inpatientAdmission != null)
            {
                _context.InpatientAdmissions.Remove(inpatientAdmission);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InpatientAdmissionExists(int id)
        {
            return _context.InpatientAdmissions.Any(e => e.AdmissionId == id);
        }
    }
}
