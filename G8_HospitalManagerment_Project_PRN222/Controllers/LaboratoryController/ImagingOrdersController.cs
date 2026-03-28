using G8_HospitalManagerment_Project_PRN222.Hubs;
using G8_HospitalManagerment_Project_PRN222.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G8_HospitalManagerment_Project_PRN222.Controllers.LaboratoryController
{
    public class ImagingOrdersController : Controller
    {
        private readonly DbHospitalManagementContext _context;
        private readonly IHubContext<DataHub> _hubContext;
        public ImagingOrdersController(DbHospitalManagementContext context, IHubContext<DataHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: ImagingOrders
        public async Task<IActionResult> Index(string searchString, string sortOrder, string filterStatus, int? pageNumber)
        {
            int pageSize = 6;
            int pageIndex = pageNumber ?? 1;

            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "date_asc" : "";
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentStatusFilter = filterStatus;
            ViewBag.CurrentPage = pageIndex;

            var query = _context.ImagingOrders
                .Include(l => l.Doctor).ThenInclude(d => d.Employee).ThenInclude(e => e.User)
                .Include(l => l.Patient).ThenInclude(p => p.User)
                .Include(l => l.MedicalRecord)
                .Include(l => l.Service)
                .AsQueryable();

            // 3. XỬ LÝ LỌC (FILTER) THEO YÊU CẦU CỦA BẠN
            if (!string.IsNullOrEmpty(filterStatus))
            {
                if (filterStatus == "Today")
                    query = query.Where(l => l.OrderDate.HasValue && l.OrderDate.Value.Date == DateTime.Today);
                else if (filterStatus == "Pending")
                    query = query.Where(l => l.Status == "Pending");
                else if (filterStatus == "Completed")
                    query = query.Where(l => l.Status == "Completed");
            }

            // 4. XỬ LÝ TÌM KIẾM (SEARCH)
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(l =>
                    (l.Service.ServiceName != null && l.Service.ServiceName.ToLower().Contains(searchString)) ||
                    l.DoctorId.ToString().Contains(searchString) || l.PatientId.ToString().Contains(searchString));
            }

            // 5. XỬ LÝ SẮP XẾP (SORT)
            switch (sortOrder)
            {
                case "doctor_asc": query = query.OrderBy(l => l.DoctorId); break;
                case "status_asc": query = query.OrderBy(l => l.Status); break;
                case "date_asc": query = query.OrderBy(l => l.OrderDate); break;
                default: query = query.OrderByDescending(l => l.OrderDate); break; // Mặc định mới nhất lên đầu
            }

            // 6. XỬ LÝ PHÂN TRANG (PAGINATION)
            int totalItems = await query.CountAsync();
            var records = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            // 7. TÍNH TOÁN CARD THỐNG KÊ BÊN DƯỚI (Không bị ảnh hưởng bởi bộ lọc)
            ViewBag.TotalOrders = await _context.ImagingOrders.CountAsync();
            ViewBag.CompletedCount = await _context.ImagingOrders.CountAsync(l => l.Status == "Completed");
            ViewBag.PendingCount = await _context.ImagingOrders.CountAsync(l => l.Status == "Pending");
            ViewBag.ActiveCount = await _context.ImagingOrders.CountAsync(l => l.IsDeleted != true);

            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.TotalItems = totalItems;
            ViewBag.ItemStart = totalItems == 0 ? 0 : (pageIndex - 1) * pageSize + 1;
            ViewBag.ItemEnd = Math.Min(pageIndex * pageSize, totalItems);

            return View(records);
        }

        // GET: ImagingOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var imagingOrder = await _context.ImagingOrders
                .Include(i => i.Doctor)
                .Include(i => i.MedicalRecord)
                .Include(i => i.Patient)
                .Include(i => i.Service)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (imagingOrder == null)
            {
                return NotFound();
            }

            return View(imagingOrder);
        }

        // GET: ImagingOrders/Create
        public IActionResult Create()
        {
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId");
            ViewData["MedicalRecordId"] = new SelectList(_context.MedicalRecords, "RecordId", "RecordId");
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId");
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceId");
            return View();
        }

        // POST: ImagingOrders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,MedicalRecordId,PatientId,DoctorId,ServiceId,OrderDate,Status,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] ImagingOrder imagingOrder)
        {
            if (ModelState.IsValid)
            {
                _context.Add(imagingOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId", imagingOrder.DoctorId);
            ViewData["MedicalRecordId"] = new SelectList(_context.MedicalRecords, "RecordId", "RecordId", imagingOrder.MedicalRecordId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", imagingOrder.PatientId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceId", imagingOrder.ServiceId);
            return View(imagingOrder);
        }

        // GET: ImagingOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var imagingOrder = await _context.ImagingOrders
                // Kéo thông tin Bệnh nhân
                .Include(i => i.Patient).ThenInclude(p => p.User)
                // Kéo thông tin Bác sĩ
                .Include(i => i.Doctor).ThenInclude(d => d.Employee).ThenInclude(e => e.User)
                // Kéo thông tin Dịch vụ (Chụp X-Quang, Siêu âm...)
                .Include(i => i.Service)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (imagingOrder == null) return NotFound();

            // Không cần dùng ViewBag nữa vì View dạng Modal không sửa các trường này
            return View(imagingOrder);
        }

        // 2. POST: ImagingOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string Status) // Chỉ nhận id và Status
        {
            var existingOrder = await _context.ImagingOrders.Include(i => i.Patient).ThenInclude(p => p.User)
                                        .FirstOrDefaultAsync(i => i.OrderId == id);
            if (existingOrder == null) return NotFound();

            // Chỉ cập nhật Trạng thái và Thời gian sửa
            existingOrder.Status = Status;
            existingOrder.UpdatedAt = DateTime.Now;

            try
            {
                _context.Update(existingOrder);
                await _context.SaveChangesAsync();

                // BẮN SIGNALR ĐỂ BÁC SĨ NHẬN ĐƯỢC THÔNG BÁO TỰ ĐỘNG LOAD LẠI TRANG
                // (Dùng chung "ReceiveLabOrderUpdate" để tab MedicalRecord dùng chung 1 sự kiện cho gọn, 
                // hoặc bạn có thể đổi thành "ReceiveImagingOrderUpdate" tùy ý)
                if (_hubContext != null)
                {
                    string fullName = existingOrder.Patient?.User?.FirstName + " " + existingOrder.Patient?.User?.LastName;
                    await _hubContext.Clients.All.SendAsync("ReceiveImagingOrderUpdate", fullName);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImagingOrderExists(existingOrder.OrderId)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ImagingOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var imagingOrder = await _context.ImagingOrders
                .Include(i => i.Doctor)
                .Include(i => i.MedicalRecord)
                .Include(i => i.Patient)
                .Include(i => i.Service)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (imagingOrder == null)
            {
                return NotFound();
            }

            return View(imagingOrder);
        }

        // POST: ImagingOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var imagingOrder = await _context.ImagingOrders.FindAsync(id);
            if (imagingOrder != null)
            {
                _context.ImagingOrders.Remove(imagingOrder);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ImagingOrderExists(int id)
        {
            return _context.ImagingOrders.Any(e => e.OrderId == id);
        }
    }
}
