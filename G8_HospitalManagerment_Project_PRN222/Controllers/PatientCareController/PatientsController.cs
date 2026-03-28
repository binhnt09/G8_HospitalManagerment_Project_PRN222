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

        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            var patients = await _context.Patients
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Patients");
                var currentRow = 1;

                // Headers
                worksheet.Cell(currentRow, 1).Value = "Mã BN";
                worksheet.Cell(currentRow, 2).Value = "Họ";
                worksheet.Cell(currentRow, 3).Value = "Tên";
                worksheet.Cell(currentRow, 4).Value = "Email";
                worksheet.Cell(currentRow, 5).Value = "SĐT";
                worksheet.Cell(currentRow, 6).Value = "Giới tính";
                worksheet.Cell(currentRow, 7).Value = "Ngày sinh";
                worksheet.Cell(currentRow, 8).Value = "Địa chỉ";
                worksheet.Cell(currentRow, 9).Value = "Nhóm máu";
                worksheet.Cell(currentRow, 10).Value = "Dị ứng";
                worksheet.Cell(currentRow, 11).Value = "Số BHYT";

                // Format headers
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 11);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                foreach (var patient in patients)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = patient.PatientId;
                    worksheet.Cell(currentRow, 2).Value = patient.User?.FirstName;
                    worksheet.Cell(currentRow, 3).Value = patient.User?.LastName;
                    worksheet.Cell(currentRow, 4).Value = patient.User?.Email;
                    worksheet.Cell(currentRow, 5).Value = patient.User?.Phone;
                    worksheet.Cell(currentRow, 6).Value = patient.User?.Gender;
                    worksheet.Cell(currentRow, 7).Value = patient.User?.BirthDay?.ToString("yyyy-MM-dd");
                    worksheet.Cell(currentRow, 8).Value = patient.User?.Address;
                    worksheet.Cell(currentRow, 9).Value = patient.BloodType;
                    worksheet.Cell(currentRow, 10).Value = patient.Allergies;
                    worksheet.Cell(currentRow, 11).Value = patient.InsuranceNumber;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DanhSachBenhNhan_{DateTime.Now:yyyyMMdd}.xlsx");
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
                // Get the Patient role id
                var patientRole = await _context.UserRoles.FirstOrDefaultAsync(r => r.RoleName == "Patient");
                int roleId = patientRole?.UserRoleId ?? 2; // Assuming 2 as fallback or another appropriate default ID

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header row

                        foreach (var row in rows)
                        {
                            var firstName = row.Cell(2).GetString()?.Trim();
                            var lastName = row.Cell(3).GetString()?.Trim();
                            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName)) continue;

                            var email = row.Cell(4).GetString()?.Trim();
                            var phone = row.Cell(5).GetString()?.Trim();
                            var gender = row.Cell(6).GetString()?.Trim();
                            
                            DateTime? birthDay = null;
                            if (DateTime.TryParse(row.Cell(7).GetString()?.Trim(), out var pDate)) birthDay = pDate;
                            
                            var address = row.Cell(8).GetString()?.Trim();
                            var bloodType = row.Cell(9).GetString()?.Trim();
                            var allergies = row.Cell(10).GetString()?.Trim();
                            var insurance = row.Cell(11).GetString()?.Trim();

                            User matchingUser = null;
                            if (!string.IsNullOrEmpty(email))
                                matchingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                            if (matchingUser == null && !string.IsNullOrEmpty(phone))
                                matchingUser = await _context.Users.FirstOrDefaultAsync(u => u.Phone == phone);

                            if (matchingUser == null)
                            {
                                // Create new User
                                matchingUser = new User
                                {
                                    FirstName = firstName,
                                    LastName = lastName,
                                    Email = email,
                                    Phone = phone,
                                    Gender = gender,
                                    BirthDay = birthDay,
                                    Address = address,
                                    UserRoleId = roleId,
                                    CreatedAt = DateTime.Now
                                };
                                _context.Users.Add(matchingUser);
                                await _context.SaveChangesAsync(); // save immediately to get UserId
                            }

                            // Check if this user is already a patient
                            var existingPatient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == matchingUser.UserId);
                            if (existingPatient == null)
                            {
                                var newPatient = new Patient
                                {
                                    UserId = matchingUser.UserId,
                                    BloodType = bloodType,
                                    Allergies = allergies,
                                    InsuranceNumber = insurance,
                                    CreatedAt = DateTime.Now
                                };
                                _context.Patients.Add(newPatient);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("ReceiveDataChange");
                TempData["Success"] = "Nhập dữ liệu bệnh nhân thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi nhập dữ liệu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.PatientId == id);
        }
    }
}
