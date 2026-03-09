using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using G8_HospitalManagerment_Project_PRN222.Models;

namespace G8_HospitalManagerment_Project_PRN222.Controllers.LaboratoryController
{
    public class LabOrdersController : Controller
    {
        private readonly DbHospitalManagementContext _context;

        public LabOrdersController(DbHospitalManagementContext context)
        {
            _context = context;
        }

        // GET: LabOrders
        public async Task<IActionResult> Index()
        {
            var dbHospitalManagementContext = _context.LabOrders.Include(l => l.Doctor).Include(l => l.MedicalRecord).Include(l => l.Patient);
            return View(await dbHospitalManagementContext.ToListAsync());
        }

        // GET: LabOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labOrder = await _context.LabOrders
                .Include(l => l.Doctor)
                .Include(l => l.MedicalRecord)
                .Include(l => l.Patient)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (labOrder == null)
            {
                return NotFound();
            }

            return View(labOrder);
        }

        // GET: LabOrders/Create
        public IActionResult Create()
        {
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId");
            ViewData["MedicalRecordId"] = new SelectList(_context.MedicalRecords, "RecordId", "RecordId");
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId");
            return View();
        }

        // POST: LabOrders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,MedicalRecordId,PatientId,DoctorId,OrderDate,Status,Reason,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] LabOrder labOrder)
        {
            if (ModelState.IsValid)
            {
                _context.Add(labOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId", labOrder.DoctorId);
            ViewData["MedicalRecordId"] = new SelectList(_context.MedicalRecords, "RecordId", "RecordId", labOrder.MedicalRecordId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", labOrder.PatientId);
            return View(labOrder);
        }

        // GET: LabOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labOrder = await _context.LabOrders.FindAsync(id);
            if (labOrder == null)
            {
                return NotFound();
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId", labOrder.DoctorId);
            ViewData["MedicalRecordId"] = new SelectList(_context.MedicalRecords, "RecordId", "RecordId", labOrder.MedicalRecordId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", labOrder.PatientId);
            return View(labOrder);
        }

        // POST: LabOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,MedicalRecordId,PatientId,DoctorId,OrderDate,Status,Reason,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] LabOrder labOrder)
        {
            if (id != labOrder.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(labOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LabOrderExists(labOrder.OrderId))
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
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId", labOrder.DoctorId);
            ViewData["MedicalRecordId"] = new SelectList(_context.MedicalRecords, "RecordId", "RecordId", labOrder.MedicalRecordId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", labOrder.PatientId);
            return View(labOrder);
        }

        // GET: LabOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labOrder = await _context.LabOrders
                .Include(l => l.Doctor)
                .Include(l => l.MedicalRecord)
                .Include(l => l.Patient)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (labOrder == null)
            {
                return NotFound();
            }

            return View(labOrder);
        }

        // POST: LabOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var labOrder = await _context.LabOrders.FindAsync(id);
            if (labOrder != null)
            {
                _context.LabOrders.Remove(labOrder);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LabOrderExists(int id)
        {
            return _context.LabOrders.Any(e => e.OrderId == id);
        }
    }
}
