using G8_HospitalManagerment_Project_PRN222.Hubs;
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
            if (id == null) return NotFound();

            var record = await _context.MedicalRecords.Include(r => r.Doctor).ThenInclude(d => d.Employee)
                        .ThenInclude(e => e.User).Include(r => r.Patient)
                        .ThenInclude(p => p.User).Include(r => r.Prescriptions)
                        .ThenInclude(p => p.PrescriptionItems).ThenInclude(pi => pi.Drug)
                        .Include(r => r.LabOrders)
                        .ThenInclude(lo => lo.LabOrderItems).ThenInclude(loi => loi.Test)
                        .Include(r => r.ImagingOrders)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RecordId == id && r.IsDeleted != true);

            if (record == null) return NotFound();

            var vm = new MedicalRecordDetailsViewModel
            {
                RecordId = record.RecordId,
                RecordDate = record.RecordDate,
                Diagnosis = record.Diagnosis,
                Symptoms = record.Symptoms,
                Treatment = record.Treatment,
                DoctorId = record.DoctorId,
                DoctorName = record.Doctor?.Employee?.User != null ? $"{record.Doctor.Employee.User.FirstName} {record.Doctor.Employee.User.LastName}" : "N/A",
                DoctorSpecialization = record.Doctor?.Specialization,
                PatientId = record.PatientId,
                PatientName = record.Patient?.User != null ? $"{record.Patient.User.FirstName} {record.Patient.User.LastName}" : "N/A",

                LabOrders = record.LabOrders.Select(l => new LabOrderDetailBlock
                {
                    OrderId = l.OrderId,
                    OrderDate = l.OrderDate,
                    Status = l.Status,
                    TestNames = l.LabOrderItems.Select(li => li.Test?.TestName ?? "Test ID: " + li.TestId).ToList()
                }).ToList(),

                // MAP DỮ LIỆU HÌNH ẢNH
                ImagingOrders = record.ImagingOrders.Select(i => new ImagingOrderDetailBlock
                {
                    OrderId = i.OrderId,
                    OrderDate = i.OrderDate,
                    Status = i.Status,
                    ServiceId = i.ServiceId,
                    ServiceName = i.Service?.ServiceName // Mở ra nếu có Include Service
                }).ToList(),

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
                //return RedirectToAction("Create", "Prescriptions", new { medicalRecordId = newRecord.RecordId });
                return RedirectToAction(nameof(Index));
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
        // GET: MedicalRecords
        public async Task<IActionResult> Index(string sortOrder, string searchString, string filterStatus, int? pageNumber)
        {
            // 1. Lưu trạng thái hiện tại để truyền ra View
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "date_asc" : ""; // Mặc định là giảm dần
            ViewBag.NameSortParm = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentStatusFilter = filterStatus;

            // 2. Query cơ bản (Bao gồm các bảng liên đới)
            var query = _context.MedicalRecords
                .Include(m => m.Appointment)
                .Include(m => m.Doctor).ThenInclude(d => d.Employee).ThenInclude(e => e.User)
                .Include(m => m.Patient).ThenInclude(p => p.User)
                .Include(m => m.LabOrders)
                .Include(m => m.ImagingOrders)
                .AsQueryable();

            // 3. XỬ LÝ LỌC (FILTER) theo Nút bấm (Ngày / Trạng thái CLS)
            if (!string.IsNullOrEmpty(filterStatus))
            {
                if (filterStatus == "Today")
                {
                    var today = DateTime.Today;
                    query = query.Where(m => m.RecordDate.HasValue && m.RecordDate.Value.Date == today);
                }
                else if (filterStatus == "PendingCLS")
                {
                    // Hồ sơ đang có xét nghiệm hoặc chụp chiếu bị "Pending"
                    query = query.Where(m =>
                        m.LabOrders.Any(l => l.Status == "Pending") ||
                        m.ImagingOrders.Any(i => i.Status == "Pending"));
                }
                else if (filterStatus == "CompletedCLS")
                {
                    // Hồ sơ có chỉ định và TẤT CẢ đều đã có kết quả (Không có cái nào Pending)
                    query = query.Where(m =>
                        (m.LabOrders.Any() || m.ImagingOrders.Any()) &&
                        !m.LabOrders.Any(l => l.Status == "Pending") &&
                        !m.ImagingOrders.Any(i => i.Status == "Pending"));
                }
            }

            // 4. XỬ LÝ TÌM KIẾM (SEARCH)
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(m =>
                    (m.Patient.User.FirstName + " " + m.Patient.User.LastName).ToLower().Contains(searchString) ||
                    m.Diagnosis.ToLower().Contains(searchString) ||
                    m.Symptoms.ToLower().Contains(searchString)
                );
            }

            // 5. XỬ LÝ SẮP XẾP (SORT)
            switch (sortOrder)
            {
                case "name_asc":
                    query = query.OrderBy(m => m.Patient.User.FirstName);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(m => m.Patient.User.FirstName);
                    break;
                case "date_asc":
                    query = query.OrderBy(m => m.RecordDate);
                    break;
                default: // Mặc định là ngày mới nhất lên đầu
                    query = query.OrderByDescending(m => m.RecordDate);
                    break;
            }

            // 6. XỬ LÝ PHÂN TRANG (PAGINATION)
            int pageSize = 10;
            int pageIndex = pageNumber ?? 1;
            int totalItems = await query.CountAsync();

            var records = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            // Gán dữ liệu phân trang ra ViewBag
            ViewBag.TotalItems = totalItems;
            ViewBag.CurrentPage = pageIndex;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.ItemStart = totalItems == 0 ? 0 : (pageIndex - 1) * pageSize + 1;
            ViewBag.ItemEnd = Math.Min(pageIndex * pageSize, totalItems);

            // Tính toán số liệu thống kê cho Dashboard
            ViewBag.TotalAllRecords = await _context.MedicalRecords.CountAsync(m => m.IsDeleted != true);
            ViewBag.TotalRecordsToday = await _context.MedicalRecords.CountAsync(m => m.RecordDate.HasValue && m.RecordDate.Value.Date == DateTime.Today);
            ViewBag.TotalPending = await _context.MedicalRecords.CountAsync(m =>
                m.IsDeleted != true && m.LabOrders.Any(l => l.Status == "Pending") || m.ImagingOrders.Any(i => i.Status == "Pending"));
            ViewBag.TotalCompleted = await _context.MedicalRecords.CountAsync(m =>
                m.IsDeleted != true && (m.LabOrders.Any() || m.ImagingOrders.Any()) &&
                !m.LabOrders.Any(l => l.Status == "Pending") && !m.ImagingOrders.Any(i => i.Status == "Pending"));

            return View(records);
        }

        // GET: MedicalRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Patient).ThenInclude(p => p.User)
                .Include(m => m.LabOrders)
                .ThenInclude(lo => lo.LabOrderItems)
                .ThenInclude(loi => loi.Test)
                .Include(m => m.ImagingOrders)// .ThenInclude(io => io.Service) // Bỏ comment nếu DB bạn có link tới Service
                .FirstOrDefaultAsync(m => m.RecordId == id);

            if (medicalRecord == null) return NotFound();

            ViewData["AppointmentId"] = new SelectList(_context.Appointments, "AppointmentId", "AppointmentId", medicalRecord.AppointmentId);
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId", medicalRecord.DoctorId);
            ViewData["DoctorName"] = medicalRecord.Doctor?.Employee.User != null
                ? $"{medicalRecord.Doctor.Employee.User.FirstName} {medicalRecord.Doctor.Employee.User.LastName}" : $"Bác sĩ #{medicalRecord.DoctorId}";
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", medicalRecord.PatientId);
            ViewData["PatientName"] = medicalRecord.Patient?.User != null
                ? $"{medicalRecord.Patient.User.FirstName} {medicalRecord.Patient.User.LastName}" : $"Bệnh nhân #{medicalRecord.PatientId}";

            return View(medicalRecord);
        }

        // POST: MedicalRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string DecisionOption, [Bind("RecordId,Diagnosis,Symptoms,Treatment")] MedicalRecord inputRecord)
        {
            if (id != inputRecord.RecordId) return NotFound();

            var existingRecord = await _context.MedicalRecords.FindAsync(id);
            if (existingRecord == null) return NotFound();

            // 1. Chỉ cập nhật thông tin y khoa bình thường
            existingRecord.Diagnosis = inputRecord.Diagnosis;
            existingRecord.Symptoms = inputRecord.Symptoms;
            existingRecord.Treatment = inputRecord.Treatment;
            existingRecord.UpdatedAt = DateTime.Now;

            try
            {
                _context.Update(existingRecord);
                await _context.SaveChangesAsync(); // Lưu chẩn đoán vào DB
                await _hubContext.Clients.All.SendAsync("ReceiveDataChange");
                TempData["Success"] = "Đã lưu chẩn đoán. Đang chuyển hướng...";

                // ========================================================
                // 2. CHỈ DÙNG DECISION OPTION ĐỂ CHUYỂN TRANG (KHÔNG LƯU DB)
                // ========================================================
                switch (DecisionOption)
                {
                    case "Outpatient":
                        // Bác sĩ chọn Nhẹ -> Nhảy sang form thêm mới Đơn Thuốc
                        return RedirectToAction("Create", "Prescriptions", new { medicalRecordId = existingRecord.RecordId });

                    case "Inpatient":
                        // Bác sĩ chọn Nhập viện -> Nhảy sang form làm hồ sơ nhập viện
                        return RedirectToAction("Create", "InpatientRecords", new { medicalRecordId = existingRecord.RecordId });

                    case "Surgery":
                        // Bác sĩ chọn Mổ -> Nhảy sang form xếp lịch mổ
                        return RedirectToAction("Create", "SurgeryOrders", new { medicalRecordId = existingRecord.RecordId });

                    default:
                        // Nếu không chọn gì (phòng hờ lỗi), trả về danh sách
                        return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.MedicalRecords.Any(e => e.RecordId == existingRecord.RecordId))
                    return NotFound();
                throw;
            }
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
