using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using G8_HospitalManagerment_Project_PRN222.Models;
using Microsoft.AspNetCore.SignalR;
using G8_HospitalManagerment_Project_PRN222.Hubs;

namespace G8_HospitalManagerment_Project_PRN222.Controllers.PharmacyController
{
    public class DrugsController : Controller
    {
        private readonly DbHospitalManagementContext _context;
        private readonly IHubContext<DataHub> _hubContext;

        public DrugsController(DbHospitalManagementContext context, IHubContext<DataHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: Drugs
        public async Task<IActionResult> Index(string searchString, string sortOrder, int pg = 1)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name_desc" : "";
            ViewBag.PriceSortParm = sortOrder == "Price" ? "Price_desc" : "Price";
            ViewBag.CreatedSortParm = sortOrder == "Created" ? "Created_desc" : "Created";
            ViewBag.CurrentFilter = searchString;

            var drugsQuery = _context.Drugs.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                drugsQuery = drugsQuery.Where(d => 
                    d.DrugName.Contains(searchString) || 
                    (d.Description != null && d.Description.Contains(searchString)));
            }

            switch (sortOrder)
            {
                case "Name_desc":
                    drugsQuery = drugsQuery.OrderByDescending(d => d.DrugName);
                    break;
                case "Price":
                    drugsQuery = drugsQuery.OrderBy(d => d.Price);
                    break;
                case "Price_desc":
                    drugsQuery = drugsQuery.OrderByDescending(d => d.Price);
                    break;
                case "Created":
                    drugsQuery = drugsQuery.OrderBy(d => d.CreatedAt);
                    break;
                case "Created_desc":
                    drugsQuery = drugsQuery.OrderByDescending(d => d.CreatedAt);
                    break;
                default:
                    drugsQuery = drugsQuery.OrderByDescending(d => d.CreatedAt); // Default to newest first
                    break;
            }

            const int pageSize = 10;
            if (pg < 1) pg = 1;
            int totalRecords = await drugsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var drugs = await drugsQuery
                .Skip((pg - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = pg;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalRecords;
            ViewBag.ItemStart = totalRecords == 0 ? 0 : (pg - 1) * pageSize + 1;
            ViewBag.ItemEnd = Math.Min(pg * pageSize, totalRecords);

            ViewBag.TotalAllRecords = await _context.Drugs.CountAsync();

            return View(drugs);
        }

        // GET: Drugs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drug = await _context.Drugs
                .FirstOrDefaultAsync(m => m.DrugId == id);
            if (drug == null)
            {
                return NotFound();
            }

            return View(drug);
        }

        // GET: Drugs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Drugs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DrugId,DrugName,Unit,Price,StockQuantity,Description,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] Drug drug)
        {
            ModelState.Remove("PrescriptionItems");

            if (ModelState.IsValid)
            {
                drug.CreatedAt = DateTime.Now;
                _context.Add(drug);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("ReceiveDataChange");
                return RedirectToAction(nameof(Index));
            }
            return View(drug);
        }

        // GET: Drugs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drug = await _context.Drugs.FindAsync(id);
            if (drug == null)
            {
                return NotFound();
            }
            return View(drug);
        }

        // POST: Drugs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DrugId,DrugName,Unit,Price,StockQuantity,Description,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] Drug drug)
        {
            if (id != drug.DrugId)
            {
                return NotFound();
            }

            ModelState.Remove("PrescriptionItems");

            if (ModelState.IsValid)
            {
                try
                {
                    drug.UpdatedAt = DateTime.Now;
                    _context.Update(drug);
                    await _context.SaveChangesAsync();
                    await _hubContext.Clients.All.SendAsync("ReceiveDataChange");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DrugExists(drug.DrugId))
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
            return View(drug);
        }

        // GET: Drugs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drug = await _context.Drugs
                .FirstOrDefaultAsync(m => m.DrugId == id);
            if (drug == null)
            {
                return NotFound();
            }

            return View(drug);
        }

        // POST: Drugs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var drug = await _context.Drugs.FindAsync(id);
            if (drug != null)
            {
                _context.Drugs.Remove(drug);
            }

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveDataChange");
            return RedirectToAction(nameof(Index));
        }

        private bool DrugExists(int id)
        {
            return _context.Drugs.Any(e => e.DrugId == id);
        }
    }
}
