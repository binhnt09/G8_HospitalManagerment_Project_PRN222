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
    public class LabResultsController : Controller
    {
        private readonly DbHospitalManagementContext _context;

        public LabResultsController(DbHospitalManagementContext context)
        {
            _context = context;
        }

        // GET: LabResults
        public async Task<IActionResult> Index()
        {
            var dbHospitalManagementContext = _context.LabResults.Include(l => l.OrderItem).Include(l => l.PerformedByNavigation);
            return View(await dbHospitalManagementContext.ToListAsync());
        }

        // GET: LabResults/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labResult = await _context.LabResults
                .Include(l => l.OrderItem)
                .Include(l => l.PerformedByNavigation)
                .FirstOrDefaultAsync(m => m.ResultId == id);
            if (labResult == null)
            {
                return NotFound();
            }

            return View(labResult);
        }

        // GET: LabResults/Create
        public IActionResult Create()
        {
            ViewData["OrderItemId"] = new SelectList(_context.LabOrderItems, "OrderItemId", "OrderItemId");
            ViewData["PerformedBy"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId");
            return View();
        }

        // POST: LabResults/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ResultId,OrderItemId,ResultValue,IsAbnormal,Remarks,PerformedBy,ResultDate")] LabResult labResult)
        {
            if (ModelState.IsValid)
            {
                _context.Add(labResult);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderItemId"] = new SelectList(_context.LabOrderItems, "OrderItemId", "OrderItemId", labResult.OrderItemId);
            ViewData["PerformedBy"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", labResult.PerformedBy);
            return View(labResult);
        }

        // GET: LabResults/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labResult = await _context.LabResults.FindAsync(id);
            if (labResult == null)
            {
                return NotFound();
            }
            ViewData["OrderItemId"] = new SelectList(_context.LabOrderItems, "OrderItemId", "OrderItemId", labResult.OrderItemId);
            ViewData["PerformedBy"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", labResult.PerformedBy);
            return View(labResult);
        }

        // POST: LabResults/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ResultId,OrderItemId,ResultValue,IsAbnormal,Remarks,PerformedBy,ResultDate")] LabResult labResult)
        {
            if (id != labResult.ResultId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(labResult);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LabResultExists(labResult.ResultId))
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
            ViewData["OrderItemId"] = new SelectList(_context.LabOrderItems, "OrderItemId", "OrderItemId", labResult.OrderItemId);
            ViewData["PerformedBy"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", labResult.PerformedBy);
            return View(labResult);
        }

        // GET: LabResults/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labResult = await _context.LabResults
                .Include(l => l.OrderItem)
                .Include(l => l.PerformedByNavigation)
                .FirstOrDefaultAsync(m => m.ResultId == id);
            if (labResult == null)
            {
                return NotFound();
            }

            return View(labResult);
        }

        // POST: LabResults/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var labResult = await _context.LabResults.FindAsync(id);
            if (labResult != null)
            {
                _context.LabResults.Remove(labResult);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LabResultExists(int id)
        {
            return _context.LabResults.Any(e => e.ResultId == id);
        }
    }
}
