using G8_HospitalManagerment_Project_PRN222.Models;
using G8_HospitalManagerment_Project_PRN222.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222.Controllers.LaboratoryController
{
    public class LabOrdersController : Controller
    {
        private readonly ILabOrderService _service;

        public LabOrdersController(ILabOrderService service)
        {
            _service = service;
        }

        // GET: LabOrders
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber)
        {
            int pageSize = 6;
            int pageIndex = pageNumber ?? 1;

            // Lưu trạng thái Search/Sort cho View
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentPage = pageIndex;

            // Gọi Service xử lý toàn bộ logic
            var dataResult = await _service.GetIndexDataAsync(searchString, sortOrder, pageIndex, pageSize);

            // Gán lại ViewBag cho View hiện tại của bạn không bị hỏng
            ViewBag.TotalOrders = dataResult.TotalOrders;
            ViewBag.CompletedCount = dataResult.CompletedCount;
            ViewBag.PendingCount = dataResult.PendingCount;
            ViewBag.ActiveCount = dataResult.ActiveCount;

            ViewBag.TotalPages = dataResult.TotalPages;
            ViewBag.TotalItems = dataResult.TotalOrders;
            ViewBag.ItemStart = dataResult.ItemStart;
            ViewBag.ItemEnd = dataResult.ItemEnd;

            // Trả đúng list PagedData ra cho file Index.cshtml
            return View(dataResult.PagedData);
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
