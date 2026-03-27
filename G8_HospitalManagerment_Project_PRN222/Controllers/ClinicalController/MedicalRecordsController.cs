using G8_HospitalManagerment_Project_PRN222.Models;
using G8_HospitalManagerment_Project_PRN222.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using G8_HospitalManagerment_Project_PRN222.Hubs;

namespace G8_HospitalManagerment_Project_PRN222.Controllers.ClinicalController
{
    public class MedicalRecordsController : Controller
    {
        private readonly DbHospitalManagementContext _context;
        private readonly IHubContext<DataHub> _hubContext;

        public MedicalRecordsController(DbHospitalManagementContext context, IHubContext<DataHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // =====================================================================
        // FEATURE 1 — Xem chi tiết hồ sơ bệnh án
        // GET: MedicalRecords/Details/5
        // =====================================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var record = await _context.MedicalRecords
                .Include(r => r.Doctor)
                    .ThenInclude(d => d.Employee)
                        .ThenInclude(e => e.User)
                .Include(r => r.Patient)
                    .ThenInclude(p => p.User)
                .Include(r => r.Prescriptions)
                    .ThenInclude(p => p.PrescriptionItems)
                        .ThenInclude(pi => pi.Drug)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RecordId == id && r.IsDeleted != true);

            if (record == null)
                return NotFound();

            var vm = new MedicalRecordDetailsViewModel
            {
                RecordId = record.RecordId,
                RecordDate = record.RecordDate,
                Diagnosis = record.Diagnosis,
                Symptoms = record.Symptoms,
                Treatment = record.Treatment,
                DoctorId = record.DoctorId,
                DoctorName = record.Doctor?.Employee?.User != null
                                         ? $"{record.Doctor.Employee.User.FirstName} {record.Doctor.Employee.User.LastName}"
                                         : "N/A",
                DoctorSpecialization = record.Doctor?.Specialization,
                PatientId = record.PatientId,
                PatientName = record.Patient?.User != null
                                         ? $"{record.Patient.User.FirstName} {record.Patient.User.LastName}"
                                         : "N/A",
                Prescriptions = record.Prescriptions
                    .Where(p => p.IsDeleted != true)
                    .Select(p => new PrescriptionDetailBlock
                    {
                        PrescriptionId = p.PrescriptionId,
                        PrescriptionDate = p.PrescriptionDate,
                        DoctorAdvice = p.DoctorAdvice,
                        Drugs = p.PrescriptionItems
                            .Where(pi => pi.IsDeleted != true)
                            .Select(pi => new DrugDetailRow
                            {
                                DrugName = pi.Drug?.DrugName ?? "N/A",
                                Dosage = pi.Dosage,
                                Quantity = pi.Quantity,
                                Unit = pi.Drug?.Unit,
                                DurationDays = pi.DurationDays,
                                UsageInstructions = pi.UsageInstructions
                            }).ToList()
                    }).ToList()
            };

            return View(vm);
        }

        // =====================================================================
        // FEATURE 2 — Thêm hồ sơ bệnh án
        // GET: MedicalRecords/Create?appointmentId=1&patientId=2&doctorId=3
        // =====================================================================

        [HttpGet]
        public async Task<IActionResult> Create(int appointmentId, int patientId, int doctorId)
        {
            // Load appointment date + patient name + doctor name for display
            var appointment = await _context.Appointments
                .AsNoTracking().FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            var patient = await _context.Patients
                .Include(p => p.User).AsNoTracking().FirstOrDefaultAsync(p => p.PatientId == patientId);

            var doctor = await _context.Doctors
                .Include(d => d.Employee).ThenInclude(e => e.User).AsNoTracking().FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            var availableLabTests = await _context.Tests
                .Select(t => new SelectListItem { Value = t.TestId.ToString(), Text = t.TestName }).ToListAsync();

            var availableImagingServices = await _context.Services
                .Select(i => new SelectListItem { Value = i.ServiceId.ToString(), Text = i.ServiceName }).ToListAsync();

            var vm = new Encounter1CreateViewModel
            {
                AppointmentId = appointmentId,
                PatientId = patientId,
                DoctorId = doctorId,
                AppointmentDate = appointment?.AppointmentDate,
                PatientName = patient?.User != null ? $"{patient.User.FirstName} {patient.User.LastName}" : $"Patient #{patientId}",
                DoctorName = doctor?.Employee?.User != null ? $"{doctor.Employee.User.FirstName} {doctor.Employee.User.LastName}" : $"Doctor #{doctorId}",

                AvailableLabTests = availableLabTests, // 2. TRUYỀN DỮ LIỆU XUỐNG VIEWMODEL ĐỂ HIỂN THỊ CHECKBOX
                AvailableImagingServices = availableImagingServices
            };
            return View(vm);
        }
        // POST: MedicalRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Encounter1CreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Insert new MedicalRecord
                var newRecord = new MedicalRecord
                {
                    AppointmentId = model.AppointmentId,
                    PatientId = model.PatientId,
                    DoctorId = model.DoctorId,
                    Diagnosis = model.Diagnosis,
                    Symptoms = model.Symptoms,
                    Treatment = model.Treatment,
                    RecordDate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                _context.MedicalRecords.Add(newRecord);
                await _context.SaveChangesAsync();  // RecordId populated here

                bool hasLabOrders = false;
                bool hasImagingOrders = false;

                // 2. NẾU CÓ CHỌN XÉT NGHIỆM -> INSERT VÀO BẢNG LabOrders
                if (model.SelectedLabTests != null && model.SelectedLabTests.Any())
                {
                    var labOrder = new LabOrder
                    {
                        MedicalRecordId = newRecord.RecordId,
                        DoctorId = model.DoctorId,
                        PatientId = newRecord.PatientId,
                        OrderDate = DateTime.Now,
                            Status = "Pending"
                    };
                    foreach (var testId in model.SelectedLabTests)
                    {
                        labOrder.LabOrderItems.Add(new LabOrderItem
                        {
                            TestId = testId,
                            Status = "Pending"
                        });
                    }
                    _context.LabOrders.Add(labOrder);
                    hasLabOrders = true;
                }
                if (model.SelectedImagingServices != null && model.SelectedImagingServices.Any())
                {
                    foreach (var serviceId in model.SelectedImagingServices)
                    {
                        var imagingOrder = new ImagingOrder
                        {
                            MedicalRecordId = newRecord.RecordId,
                            PatientId = model.PatientId,
                            DoctorId = model.DoctorId,
                            ServiceId = serviceId,
                            OrderDate = DateTime.Now,
                            Status = "Pending"
                        };
                        _context.ImagingOrders.Add(imagingOrder);
                    }
                    hasImagingOrders = true;
                }

                // 2. Mark Appointment as Completed
                var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == model.AppointmentId);
                if (appointment != null)
                {
                    appointment.Status = "Completed";
                    appointment.UpdatedAt = DateTime.Now;
                    _context.Appointments.Update(appointment);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 5. BẮN SIGNALR REAL-TIME SAU KHI LƯU THÀNH CÔNG
                //if (hasLabOrders)
                //{
                //    // Gửi tin nhắn đến Group những người đang mở trang Kỹ thuật viên Xét nghiệm
                //    await _hubContext.Clients.Group("LabTechnicians").SendAsync("ReceiveNewOrder", newRecord.RecordId, model.PatientName);
                //}
                //if (hasImagingOrders)
                //{
                //    // Gửi tin nhắn đến Group những người đang mở trang Kỹ thuật viên Chẩn đoán hình ảnh
                //    await _hubContext.Clients.Group("ImagingTechnicians").SendAsync("ReceiveNewOrder", newRecord.RecordId, model.PatientName);
                //}
                
                await _hubContext.Clients.All.SendAsync("ReceiveDataChange");

                TempData["Success"] = "Hồ sơ bệnh án đã được tạo thành công.";

                // Redirect to prescription creation with the new RecordId
                return RedirectToAction("Create", "Prescriptions", new { medicalRecordId = newRecord.RecordId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");

                // Nếu lỗi, phải load lại danh sách Checkbox trả về cho View
                model.AvailableLabTests = await _context.Tests.Select(t => new SelectListItem { Value = t.TestId.ToString(), Text = t.TestName }).ToListAsync();
                model.AvailableImagingServices = await _context.Services.Select(i => new SelectListItem { Value = i.ServiceId.ToString(), Text = i.ServiceName }).ToListAsync();
                return View(model);
            }
        }

        // =====================================================================
        // Original Scaffolded CRUD Actions (unchanged except Details replaced above)
        // =====================================================================

        // GET: MedicalRecords
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber)
        {
            // 1. Giữ trạng thái của thanh tìm kiếm và sắp xếp để truyền lại View
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.CurrentFilter = searchString;

            // 2. Query cơ sở kết nối các bảng
            var records = _context.MedicalRecords
                .Include(m => m.Patient).ThenInclude(p => p.User)
                .Include(m => m.Doctor).ThenInclude(d => d.Employee).ThenInclude(e => e.User)
                .Include(m => m.Appointment)
                .AsQueryable();

            // 3. Xử lý Tìm kiếm (Search)
            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                records = records.Where(m =>
                    (m.Patient != null && m.Patient.User != null && (m.Patient.User.FirstName + " " + m.Patient.User.LastName).ToLower().Contains(searchString)) ||
                    (m.Doctor != null && m.Doctor.Employee != null && m.Doctor.Employee.User != null && (m.Doctor.Employee.User.FirstName + " " + m.Doctor.Employee.User.LastName).ToLower().Contains(searchString)) ||
                    (m.Diagnosis != null && m.Diagnosis.ToLower().Contains(searchString))
                );
            }

            // 4. Xử lý Sắp xếp (Sort)
            switch (sortOrder)
            {
                case "date_desc":
                    records = records.OrderByDescending(m => m.RecordDate);
                    break;
                case "doctor_asc":
                    records = records.OrderBy(m => m.Doctor.Employee.User.FirstName).ThenBy(m => m.Doctor.Employee.User.LastName);
                    break;
                case "patient_asc":
                    records = records.OrderBy(m => m.Patient.User.FirstName).ThenBy(m => m.Patient.User.LastName);
                    break;
                default: // Mặc định sắp xếp ngày tăng dần
                    records = records.OrderBy(m => m.RecordDate);
                    break;
            }

            // 5. Xử lý Phân trang (Pagination)
            int pageSize = 6;
            int pageIndex = pageNumber ?? 1;
            int totalItems = await records.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.CurrentPage = pageIndex;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.ItemStart = totalItems == 0 ? 0 : (pageIndex - 1) * pageSize + 1;
            ViewBag.ItemEnd = Math.Min(pageIndex * pageSize, totalItems);

            // Lấy dữ liệu của trang hiện tại
            var pagedData = await records
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(pagedData);
        }

        // GET: MedicalRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord == null)
                return NotFound();

            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "AppointmentId", "AppointmentId", medicalRecord.AppointmentId);
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId", medicalRecord.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", medicalRecord.PatientId);
            return View(medicalRecord);
        }

        // POST: MedicalRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RecordId,AppointmentId,PatientId,DoctorId,Diagnosis,Symptoms,Treatment,RecordDate,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] MedicalRecord medicalRecord)
        {
            if (id != medicalRecord.RecordId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medicalRecord);
                    await _context.SaveChangesAsync();
                    await _hubContext.Clients.All.SendAsync("ReceiveDataChange");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.MedicalRecords.Any(e => e.RecordId == medicalRecord.RecordId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "AppointmentId", "AppointmentId", medicalRecord.AppointmentId);
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId", medicalRecord.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", medicalRecord.PatientId);
            return View(medicalRecord);
        }

        // GET: MedicalRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Appointment)
                .Include(m => m.Doctor)
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.RecordId == id);

            if (medicalRecord == null)
                return NotFound();

            return View(medicalRecord);
        }

        // POST: MedicalRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord != null)
                _context.MedicalRecords.Remove(medicalRecord);

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveDataChange");
            return RedirectToAction(nameof(Index));
        }
    }
}
