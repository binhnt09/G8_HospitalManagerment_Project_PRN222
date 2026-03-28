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
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using System.IO;

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

        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            var drugs = await _context.Drugs.OrderByDescending(d => d.CreatedAt).ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Drugs");
                var currentRow = 1;

                // Headers
                worksheet.Cell(currentRow, 1).Value = "Mã Thuốc";
                worksheet.Cell(currentRow, 2).Value = "Tên Thuốc";
                worksheet.Cell(currentRow, 3).Value = "Đơn vị";
                worksheet.Cell(currentRow, 4).Value = "Giá";
                worksheet.Cell(currentRow, 5).Value = "Số lượng tồn";
                worksheet.Cell(currentRow, 6).Value = "Mô tả";

                // Format headers
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                foreach (var drug in drugs)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = drug.DrugId;
                    worksheet.Cell(currentRow, 2).Value = drug.DrugName;
                    worksheet.Cell(currentRow, 3).Value = drug.Unit;
                    worksheet.Cell(currentRow, 4).Value = drug.Price;
                    worksheet.Cell(currentRow, 5).Value = drug.StockQuantity;
                    worksheet.Cell(currentRow, 6).Value = drug.Description;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DanhSachThuoc_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file Excel.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header row

                        foreach (var row in rows)
                        {
                            var drugName = row.Cell(2).GetString()?.Trim();
                            if (string.IsNullOrWhiteSpace(drugName)) continue;

                            var unitStr = row.Cell(3).GetString()?.Trim();
                            var priceStr = row.Cell(4).GetString()?.Trim();
                            var qtyStr = row.Cell(5).GetString()?.Trim();
                            var descStr = row.Cell(6).GetString()?.Trim();

                            var existingDrug = await _context.Drugs.FirstOrDefaultAsync(d => d.DrugName.ToLower() == drugName.ToLower());

                            if (existingDrug != null)
                            {
                                // Update existing
                                if (!string.IsNullOrEmpty(unitStr)) existingDrug.Unit = unitStr;
                                if (decimal.TryParse(priceStr, out var price)) existingDrug.Price = price;
                                if (int.TryParse(qtyStr, out var qty)) existingDrug.StockQuantity = qty;
                                if (!string.IsNullOrEmpty(descStr)) existingDrug.Description = descStr;
                                existingDrug.UpdatedAt = DateTime.Now;
                            }
                            else
                            {
                                // Add new
                                var newDrug = new Drug
                                {
                                    DrugName = drugName,
                                    Unit = unitStr,
                                    Price = decimal.TryParse(priceStr, out var nprice) ? (decimal?)nprice : null,
                                    StockQuantity = int.TryParse(qtyStr, out var nqty) ? (int?)nqty : null,
                                    Description = descStr,
                                    CreatedAt = DateTime.Now
                                };
                                _context.Drugs.Add(newDrug);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("ReceiveDataChange");
                TempData["Success"] = "Nhập dữ liệu thuốc thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi nhập dữ liệu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DrugExists(int id)
        {
            return _context.Drugs.Any(e => e.DrugId == id);
        }
    }
}
