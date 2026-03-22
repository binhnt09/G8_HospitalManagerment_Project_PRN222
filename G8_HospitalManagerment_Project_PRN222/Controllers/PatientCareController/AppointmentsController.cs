using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

    public async Task<IActionResult> Index()
    {
        var appointments = await _service.GetAllAsync();
        return View(appointments);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        if (!ModelState.IsValid)
        {
            LoadDropdowns();
            return View(appointment);
        }

        await _service.CreateAsync(appointment);
        return RedirectToAction(nameof(Index));
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
        ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentId");
        ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId");
        ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId");
    }
}
}
