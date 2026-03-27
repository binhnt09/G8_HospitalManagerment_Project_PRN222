using G8_HospitalManagerment_Project_PRN222.Models;
using G8_HospitalManagerment_Project_PRN222.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace G8_HospitalManagerment_Project_PRN222.Controllers.HRController
{
    public class DoctorsController : Controller
    {
        private readonly DbHospitalManagementContext _context; 
        private readonly HttpClient _httpClient;

        public DoctorsController(DbHospitalManagementContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        // GET: Doctors
        [AllowAnonymous]
        // Hàm gọi Server TCP
        public async Task<List<DoctorViewModel>> FetchDoctorsFromServer(string name = "", string spec = "", int exp = 0)
        {
            try
            {
                using (TcpClient client = new TcpClient("127.0.0.1", 13000))
                using (NetworkStream stream = client.GetStream())
                {
                    // 1. Gửi lệnh lọc (Định dạng tự định nghĩa)
                    string command = $"FILTER|name={name};spec={spec};exp={exp}";
                    byte[] cmdBytes = Encoding.UTF8.GetBytes(command);
                    await stream.WriteAsync(cmdBytes, 0, cmdBytes.Length);
                    await stream.FlushAsync();

                    // 2. Đọc Header độ dài (4 byte)
                    byte[] header = new byte[4];
                    int hRead = 0;
                    while (hRead < 4)
                    {
                        int r = await stream.ReadAsync(header, hRead, 4 - hRead);
                        if (r == 0) return null;
                        hRead += r;
                    }
                    int dataSize = BitConverter.ToInt32(header, 0);

                    // 3. Đọc dữ liệu JSON (Body)
                    byte[] buffer = new byte[dataSize];
                    int bRead = 0;
                    while (bRead < dataSize)
                    {
                        int r = await stream.ReadAsync(buffer, bRead, dataSize - bRead);
                        if (r == 0) break;
                        bRead += r;
                    }

                    string json = Encoding.UTF8.GetString(buffer);
                    return JsonConvert.DeserializeObject<List<DoctorViewModel>>(json);
                }
            }
            catch { return new List<DoctorViewModel>(); }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchName = "", string specialty = "", int minExp = 0)
        {
            // Gọi hàm TCP với các tham số (nếu rỗng thì TCP Server sẽ trả về ALL)
            var doctors = await FetchDoctorsFromServer(searchName, specialty, minExp);

            // In Log để kiểm tra giá trị nhận được từ Form
            Console.WriteLine($"Search: {searchName}, Spec: {specialty}, Exp: {minExp}");

            if (doctors != null)
            {
                Console.WriteLine($"Đã lấy được {doctors.Count} bác sĩ từ TCP");

                // Lưu lại để hiển thị trên Form
                ViewBag.SearchName = searchName;
                ViewBag.Specialty = specialty;
                ViewBag.MinExp = minExp;

                return View(doctors);
            }

            Console.WriteLine("Không lấy được dữ liệu từ TCP Server");
            return View(new List<DoctorViewModel>());
        }

        // GET: Doctors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(m => m.DoctorId == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // GET: Doctors/Create
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId");
            return View();
        }

        // POST: Doctors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DoctorId,Specialization,YearsExperience,LicenseNumber,EmployeeId,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", doctor.EmployeeId);
            return View(doctor);
        }

        // GET: Doctors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", doctor.EmployeeId);
            return View(doctor);
        }

        // POST: Doctors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DoctorId,Specialization,YearsExperience,LicenseNumber,EmployeeId,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] Doctor doctor)
        {
            if (id != doctor.DoctorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.DoctorId))
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", doctor.EmployeeId);
            return View(doctor);
        }

        // GET: Doctors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(m => m.DoctorId == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.DoctorId == id);
        }
    }
}
