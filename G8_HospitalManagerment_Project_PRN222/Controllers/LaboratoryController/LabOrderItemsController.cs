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
    public class LabOrderItemsController : Controller
    {
        private readonly DbHospitalManagementContext _context;

        public LabOrderItemsController(DbHospitalManagementContext context)
        {
            _context = context;
        }

        // GET: LabOrderItems
        public async Task<IActionResult> Index()
        {
            var dbHospitalManagementContext = _context.LabOrderItems.Include(l => l.Order).Include(l => l.Test);
            return View(await dbHospitalManagementContext.ToListAsync());
        }

        // GET: LabOrderItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labOrderItem = await _context.LabOrderItems
                .Include(l => l.Order)
                .Include(l => l.Test)
                .FirstOrDefaultAsync(m => m.OrderItemId == id);
            if (labOrderItem == null)
            {
                return NotFound();
            }

            return View(labOrderItem);
        }

        // GET: LabOrderItems/Create
        public IActionResult Create()
        {
            ViewData["OrderId"] = new SelectList(_context.LabOrders, "OrderId", "OrderId");
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId");
            return View();
        }

        // POST: LabOrderItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderItemId,OrderId,TestId,Status,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] LabOrderItem labOrderItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(labOrderItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderId"] = new SelectList(_context.LabOrders, "OrderId", "OrderId", labOrderItem.OrderId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", labOrderItem.TestId);
            return View(labOrderItem);
        }

        // GET: LabOrderItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labOrderItem = await _context.LabOrderItems.FindAsync(id);
            if (labOrderItem == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.LabOrders, "OrderId", "OrderId", labOrderItem.OrderId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", labOrderItem.TestId);
            return View(labOrderItem);
        }

        // POST: LabOrderItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderItemId,OrderId,TestId,Status,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] LabOrderItem labOrderItem)
        {
            if (id != labOrderItem.OrderItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(labOrderItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LabOrderItemExists(labOrderItem.OrderItemId))
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
            ViewData["OrderId"] = new SelectList(_context.LabOrders, "OrderId", "OrderId", labOrderItem.OrderId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", labOrderItem.TestId);
            return View(labOrderItem);
        }

        // GET: LabOrderItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labOrderItem = await _context.LabOrderItems
                .Include(l => l.Order)
                .Include(l => l.Test)
                .FirstOrDefaultAsync(m => m.OrderItemId == id);
            if (labOrderItem == null)
            {
                return NotFound();
            }

            return View(labOrderItem);
        }

        // POST: LabOrderItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var labOrderItem = await _context.LabOrderItems.FindAsync(id);
            if (labOrderItem != null)
            {
                _context.LabOrderItems.Remove(labOrderItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LabOrderItemExists(int id)
        {
            return _context.LabOrderItems.Any(e => e.OrderItemId == id);
        }
    }
}
