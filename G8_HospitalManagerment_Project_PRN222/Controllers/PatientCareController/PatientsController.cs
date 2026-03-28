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

namespace G8_HospitalManagerment_Project_PRN222.Controllers.PatientCare
{
    public class PatientsController : Controller
    {
        private readonly DbHospitalManagementContext _context;
        private readonly IHubContext<DataHub> _hubContext;

        public PatientsController(DbHospitalManagementContext context, IHubContext<DataHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: Patients
        public async Task<IActionResult> Index(string searchString, string sortOrder, int pg = 1)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "DateOfBirth" ? "DateOfBirth_desc" : "DateOfBirth";
            ViewBag.CreatedSortParm = sortOrder == "Created" ? "Created_desc" : "Created";
            ViewBag.CurrentFilter = searchString;

            var patientsQuery = _context.Patients.Include(p => p.User).AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                patientsQuery = patientsQuery.Where(p =>
                    p.User.FirstName.Contains(searchString) ||
                    p.User.LastName.Contains(searchString) ||
                    p.User.Phone.Contains(searchString) ||
                    p.PatientId.ToString().Contains(searchString));
            }

            switch (sortOrder)
            {
                case "Name_desc":
                    patientsQuery = patientsQuery.OrderByDescending(p => p.User.FirstName);
                    break;
                case "DateOfBirth":
                    patientsQuery = patientsQuery.OrderBy(p => p.User.BirthDay);
                    break;
                case "DateOfBirth_desc":
                    patientsQuery = patientsQuery.OrderByDescending(p => p.User.BirthDay);
                    break;
                case "Created":
                    patientsQuery = patientsQuery.OrderBy(p => p.CreatedAt);
                    break;
                case "Created_desc":
                    patientsQuery = patientsQuery.OrderByDescending(p => p.CreatedAt);
                    break;
                default:
                    patientsQuery = patientsQuery.OrderByDescending(p => p.CreatedAt); // Default to newest first
                    break;
            }

            const int pageSize = 10;
            if (pg < 1) pg = 1;
            int totalRecords = await patientsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var patients = await patientsQuery
                .Skip((pg - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = pg;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalRecords;
            ViewBag.ItemStart = totalRecords == 0 ? 0 : (pg - 1) * pageSize + 1;
            ViewBag.ItemEnd = Math.Min(pg * pageSize, totalRecords);

            ViewBag.TotalAllRecords = await _context.Patients.CountAsync();

            return View(patients);
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PatientId == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // GET: Patients/Create
        public IActionResult Create()
        {
            var users = _context.Users.Select(u => new { u.UserId, FullName = u.FirstName + " " + u.LastName }).ToList();
            ViewData["UserId"] = new SelectList(users, "UserId", "FullName");
            return View();
        }

        // POST: Patients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PatientId,BloodType,Allergies,InsuranceNumber,UserId,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] Patient patient)
        {
            ModelState.Remove("User"); // Ignore navigation property validation
            
            if (ModelState.IsValid)
            {
                patient.CreatedAt = DateTime.Now;
                _context.Add(patient);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("ReceiveDataChange");
                return RedirectToAction(nameof(Index));
            }
            
            var users = _context.Users.Select(u => new { u.UserId, FullName = u.FirstName + " " + u.LastName }).ToList();
            ViewData["UserId"] = new SelectList(users, "UserId", "FullName", patient.UserId);
            return View(patient);
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            
            var users = _context.Users.Select(u => new { u.UserId, FullName = u.FirstName + " " + u.LastName }).ToList();
            ViewData["UserId"] = new SelectList(users, "UserId", "FullName", patient.UserId);
            return View(patient);
        }

        // POST: Patients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PatientId,BloodType,Allergies,InsuranceNumber,UserId,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] Patient patient)
        {
            if (id != patient.PatientId)
            {
                return NotFound();
            }

            ModelState.Remove("User"); // Ignore navigation property validation

            if (ModelState.IsValid)
            {
                try
                {
                    patient.UpdatedAt = DateTime.Now;
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                    await _hubContext.Clients.All.SendAsync("ReceiveDataChange");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.PatientId))
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
            
            var users = _context.Users.Select(u => new { u.UserId, FullName = u.FirstName + " " + u.LastName }).ToList();
            ViewData["UserId"] = new SelectList(users, "UserId", "FullName", patient.UserId);
            return View(patient);
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PatientId == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
            }

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveDataChange");
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.PatientId == id);
        }
    }
}
