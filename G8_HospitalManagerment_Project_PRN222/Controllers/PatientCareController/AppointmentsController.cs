using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using G8_HospitalManagerment_Project_PRN222.Models;

namespace G8_HospitalManagerment_Project_PRN222.Controllers.PatientCareController
{
    public class AppointmentsController : Controller
{
    private readonly AppointmentService _service;
    private readonly DbHospitalManagementContext _context;

    public AppointmentsController(AppointmentService service, DbHospitalManagementContext context)
    {
        _service = service;
        _context = context;
    }

    public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber)
    {
        // 1. Giữ trạng thái của thanh tìm kiếm và sắp xếp để truyền lại View
        ViewBag.CurrentSort = sortOrder;
        ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
        ViewBag.CurrentFilter = searchString;

        // 2. Query cơ sở kết nối các bảng
        var appointments = _context.Appointments
            .Include(a => a.Department)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Employee)
                    .ThenInclude(e => e.User)
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .AsQueryable();

        // 3. Tính toán dữ liệu cho 4 thẻ Thống kê (Summary Cards) ở cuối trang
        ViewBag.TotalAppointments = await appointments.CountAsync();
        ViewBag.CompletedCount = await appointments.CountAsync(a => a.Status == "Completed");
        ViewBag.ConfirmedCount = await appointments.CountAsync(a => a.Status == "Confirmed");
        ViewBag.PendingCount = await appointments.CountAsync(a => a.Status == "Pending");

        // 4. Xử lý Tìm kiếm (Search)
        if (!String.IsNullOrEmpty(searchString))
        {
            searchString = searchString.ToLower();
            appointments = appointments.Where(a =>
                (a.Reason != null && a.Reason.ToLower().Contains(searchString)) ||
                (a.Doctor != null && a.Doctor.Employee != null && a.Doctor.Employee.User != null && (a.Doctor.Employee.User.FirstName + " " + a.Doctor.Employee.User.LastName).ToLower().Contains(searchString)) ||
                (a.Patient != null && a.Patient.User != null && (a.Patient.User.FirstName + " " + a.Patient.User.LastName).ToLower().Contains(searchString)));
        }

        // 5. Xử lý Sắp xếp (Sort)
        switch (sortOrder)
        {
            case "date_desc":
                appointments = appointments.OrderByDescending(a => a.AppointmentDate);
                break;
            case "doctor_asc":
                appointments = appointments.OrderBy(a => a.Doctor.Employee.User.FirstName).ThenBy(a => a.Doctor.Employee.User.LastName);
                break;
            case "status_asc":
                appointments = appointments.OrderBy(a => a.Status);
                break;
            default: // Mặc định sắp xếp ngày tăng dần
                appointments = appointments.OrderBy(a => a.AppointmentDate);
                break;
        }

        // 6. Xử lý Phân trang (Pagination)
        int pageSize = 6; // Số lượng record trên mỗi trang giống trong ảnh
        int pageIndex = pageNumber ?? 1;
        int totalItems = await appointments.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        ViewBag.CurrentPage = pageIndex;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;
        ViewBag.ItemStart = (pageIndex - 1) * pageSize + 1;
        ViewBag.ItemEnd = Math.Min(pageIndex * pageSize, totalItems);

        // Lấy dữ liệu của trang hiện tại
        var pagedData = await appointments
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return View(pagedData);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var appointment = await _service.GetByIdAsync(id.Value);
        if (appointment == null) return NotFound();

        return View(appointment);
    }

    public IActionResult Create()
    {
        LoadDropdowns();
        return View();
    }
    
    // GET: /Appointments/Book - simple booking form for logged-in patients
    public IActionResult Book()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Authentication");
        }

        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        ModelState.Remove("Department");
        ModelState.Remove("Doctor");
        ModelState.Remove("Patient");
        if (!ModelState.IsValid)
        {
            LoadDropdowns();
            return View(appointment);
        }
        
        await _service.CreateAsync(appointment);
        return RedirectToAction(nameof(Index));
    }

    // POST: /Appointments/Book
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book([Bind("AppointmentDate,Reason")] Appointment model)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Authentication");
        }

        ModelState.Remove("DoctorId");
        ModelState.Remove("DepartmentId");
        ModelState.Remove("PatientId");
        ModelState.Remove("Department");
        ModelState.Remove("Doctor");
        ModelState.Remove("Patient");
        ModelState.Remove("Status");
        
        // Ensure patient record exists for current user
        var patient = _context.Patients.FirstOrDefault(p => p.UserId == userId.Value);
        if (patient == null)
        {
            patient = new Patient { UserId = userId.Value };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
        }

        // Pick a default doctor and department (assign first available)
        // var doctor = _context.Doctors.FirstOrDefault();
        // var department = _context.Departments.FirstOrDefault();
        // if (doctor == null || department == null)
        // {
        //     ModelState.AddModelError(string.Empty, "No doctors or departments available. Please contact admin.");
        //     return View(model);
        // }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.AppointmentDate <= DateTime.Now)
        {
            ModelState.AddModelError("AppointmentDate", "Appointment date must be in the future.");
            return View(model);
        }
        var appointment = new Appointment
        {
            PatientId = patient.PatientId,
            DoctorId = null,
            DepartmentId = null,
            AppointmentDate = model.AppointmentDate,
            Reason = model.Reason,
            Status = "Pending"
        };

        await _service.CreateAsync(appointment);

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var appointment = await _service.GetByIdAsync(id.Value);
        if (appointment == null) return NotFound();

        LoadDropdowns();
        return View(appointment);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Appointment appointment)
    {
        if (id != appointment.AppointmentId) return NotFound();
        ModelState.Remove("Department");
        ModelState.Remove("Doctor");
        ModelState.Remove("Patient");
        if (!ModelState.IsValid)
        {
            LoadDropdowns();
            return View(appointment);
        }

        await _service.UpdateAsync(appointment);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var appointment = await _service.GetByIdAsync(id.Value);
        if (appointment == null) return NotFound();

        return View(appointment);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _service.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private void LoadDropdowns()
    {
        ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
        ViewData["DoctorId"] = new SelectList(_context.Doctors.Include(d => d.Employee).ThenInclude(e => e.User).Select(d => new { d.DoctorId, Name = d.Employee != null && d.Employee.User != null ? d.Employee.User.FirstName + " " + d.Employee.User.LastName : "Unknown Doctor" }), "DoctorId", "Name");
        ViewData["PatientId"] = new SelectList(_context.Patients.Include(p => p.User).Select(p => new { p.PatientId, Name = p.User != null ? p.User.FirstName + " " + p.User.LastName : "Unknown Patient" }), "PatientId", "Name");
        ViewData["Status"] = new SelectList(new[] { "Confirmed", "Completed", "Cancelled" }, "Confirmed");
    }
}
}
