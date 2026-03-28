using G8_HospitalManagerment_Project_PRN222.Models;
using G8_HospitalManagerment_Project_PRN222.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222.Controllers.LaboratoryController
{
    public class LabOrdersController : Controller
    {
        private readonly ILabOrderService _service;
        private readonly DbHospitalManagementContext _context;

        public LabOrdersController(ILabOrderService service, DbHospitalManagementContext context)
        {
                _service = service;
            _context = context;
        }

        // GET: LabOrders
        public async Task<IActionResult> Index(string searchString, string sortOrder, string filterStatus, int? pageNumber)
        {
            int pageSize = 6;
            int pageIndex = pageNumber ?? 1;

            // 1. Lưu trạng thái Search/Sort/Filter cho View
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "date_asc" : "";
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentStatusFilter = filterStatus;
            ViewBag.CurrentPage = pageIndex;

            // 2. Khởi tạo Query trực tiếp tại Controller để xử lý Lọc chính xác
            var query = _context.LabOrders
                .Include(l => l.Doctor)
                .Include(l => l.Patient)
                .Include(l => l.MedicalRecord)
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
                    (l.Reason != null && l.Reason.ToLower().Contains(searchString)) ||
                    l.DoctorId.ToString().Contains(searchString) ||
                    l.PatientId.ToString().Contains(searchString));
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
            ViewBag.TotalOrders = await _context.LabOrders.CountAsync();
            ViewBag.CompletedCount = await _context.LabOrders.CountAsync(l => l.Status == "Completed");
            ViewBag.PendingCount = await _context.LabOrders.CountAsync(l => l.Status == "Pending");
            ViewBag.ActiveCount = await _context.LabOrders.CountAsync(l => l.IsDeleted != true);

            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.TotalItems = totalItems;
            ViewBag.ItemStart = totalItems == 0 ? 0 : (pageIndex - 1) * pageSize + 1;
            ViewBag.ItemEnd = Math.Min(pageIndex * pageSize, totalItems);

            return View(records);
        }

        // GET: LabOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var labOrder = await _service.GetLabOrderDetailsAsync(id.Value);
            if (labOrder == null) return NotFound();

            return View(labOrder);
        }

        // GET: LabOrders/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        // POST: LabOrders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,MedicalRecordId,PatientId,DoctorId,OrderDate,Status,Reason,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] LabOrder labOrder)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateLabOrderAsync(labOrder);
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns(labOrder);
            return View(labOrder);
        }

        // GET: LabOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var labOrder = await _service.GetLabOrderDetailsAsync(id.Value);
            if (labOrder == null) return NotFound();

            await PopulateDropdowns(labOrder);
            return View(labOrder);
        }

        // POST: LabOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,MedicalRecordId,PatientId,DoctorId,OrderDate,Status,Reason,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] LabOrder labOrder)
        {
            if (id != labOrder.OrderId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _service.UpdateLabOrderAsync(labOrder);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_service.CheckLabOrderExists(labOrder.OrderId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdowns(labOrder);
            return View(labOrder);
        }

        // GET: LabOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var labOrder = await _service.GetLabOrderDetailsAsync(id.Value);
            if (labOrder == null) return NotFound();

            return View(labOrder);
        }

        // POST: LabOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteLabOrderAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // --- Helper Method giúp Controller gọi Dropdown gọn hơn ---
        private async Task PopulateDropdowns(LabOrder labOrder = null)
        {
            var dropdownData = await _service.GetDropdownDataAsync();
            ViewData["DoctorId"] = new SelectList(dropdownData.Doctors, "DoctorId", "DoctorId", labOrder?.DoctorId);
            ViewData["MedicalRecordId"] = new SelectList(dropdownData.MedicalRecords, "RecordId", "RecordId", labOrder?.MedicalRecordId);
            ViewData["PatientId"] = new SelectList(dropdownData.Patients, "PatientId", "PatientId", labOrder?.PatientId);
        }
    }
}
