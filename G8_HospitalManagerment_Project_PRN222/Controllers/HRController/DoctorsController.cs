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

        // GET: Create
        public IActionResult Create()
        {
            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            ViewBag.Users = new SelectList(_context.Users.Where(u => u.IsDeleted != true), "UserId", "Email");
            // Danh sách khoa cố định hoặc lấy từ DB
            ViewBag.Specializations = new List<string> { "Nội khoa", "Ngoại khoa", "Nhi khoa", "Sản khoa", "Tai Mũi Họng" };
            return View();
        }

        // POST: Create
        [HttpPost]
        public async Task<IActionResult> Create(DoctorViewModel model)
        {
            // Lấy FullName từ UserId để Server tạo Code
            var user = await _context.Users.FindAsync(model.UserId);
            model.FullName = user?.FirstName + user?.LastName;

            string res = await SendTcp("CREATE_DOCTOR", JsonConvert.SerializeObject(model));
            if (res.StartsWith("SUCCESS")) return RedirectToAction("Index");

            ModelState.AddModelError("", res);
            return View(model);
        }

        // GET: Doctors/Edit/5
        // Action hiển thị Form chỉnh sửa với dữ liệu cũ
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) return NotFound();

            // 1. Gọi TCP Server để lấy danh sách bác sĩ (dùng hàm FILTER hoặc FETCH cũ của bạn)
            // Giả sử hàm FetchDoctorsFromServer trả về List<DoctorViewModel>
            var doctors = await FetchDoctorsFromServer();

            // 2. Tìm đúng bác sĩ cần sửa trong danh sách trả về
            var doctor = doctors?.FirstOrDefault(d => d.DoctorId == id);

            if (doctor == null)
            {
                // Có thể hiện thông báo lỗi bằng TempData hoặc ViewBag
                return NotFound();
            }

            // 3. Chuẩn bị dữ liệu cho các Dropdown (Phòng ban, Chuyên khoa)
            // Load Departments từ DB của Web Client để người dùng chọn lại nếu cần
            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", doctor.DepartmentId);

            // Danh sách chuyên khoa cố định (nên trùng với danh sách ở trang Create)
            ViewBag.Specializations = new List<string> { "Nội khoa", "Ngoại khoa", "Nhi khoa", "Sản khoa", "Tai Mũi Họng" };

            // 4. Trả về View Edit kèm dữ liệu bác sĩ
            return View(doctor);
        }

        // POST: Doctors/Edit
        // Action xử lý dữ liệu từ Form gửi lên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DoctorViewModel model)
        {
            // 1. Validate dữ liệu ở phía Client (ModelState)
            if (!ModelState.IsValid)
            {
                // Nếu dữ liệu không hợp lệ (ví dụ: thiếu số CCHN), nạp lại ViewBag và hiện lại Form lỗi
                ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", model.DepartmentId);
                ViewBag.Specializations = new List<string> { "Nội khoa", "Ngoại khoa", "Nhi khoa", "Sản khoa" };
                return View(model);
            }

            try
            {
                // 2. Chuyển đổi Model thành JSON
                string jsonData = JsonConvert.SerializeObject(model);

                // 3. Gửi lệnh UPDATE_DOCTOR sang TCP Server
                string result = await SendTcp("UPDATE_DOCTOR", jsonData);

                // 4. Xử lý phản hồi từ Server
                if (result == "SUCCESS")
                {
                    // Dùng TempData để hiển thị thông báo thành công ở trang Index
                    TempData["SuccessMessage"] = $"Cập nhật thành công bác sĩ {model.EmployeeCode}";
                    return RedirectToAction(nameof(Index));
                }

                // Nếu Server báo lỗi, hiển thị lỗi lên Form
                ModelState.AddModelError("", $"Lỗi từ Server: {result.Replace("ERROR|", "")}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi kết nối TCP: {ex.Message}");
            }

            // Nạp lại ViewBag nếu có lỗi xảy ra
            ViewBag.Departments = new SelectList(_context.Departments, "DepartmentId", "DepartmentName", model.DepartmentId);
            ViewBag.Specializations = new List<string> { "Nội khoa", "Ngoại khoa", "Nhi khoa", "Sản khoa" };
            return View(model);
        }

        private async Task<string> SendTcp(string action, string data)
        {
            try
            {
                using var client = new TcpClient("127.0.0.1", 13000);
                using var stream = client.GetStream();

                // 1. Chuyển lệnh và data sang UTF8 Byte
                byte[] cmdBytes = Encoding.UTF8.GetBytes($"{action}|{data}");
                await stream.WriteAsync(cmdBytes, 0, cmdBytes.Length);
                await stream.FlushAsync();

                // 2. Đọc Header (4 byte độ dài)
                byte[] header = new byte[4];
                int hRead = await stream.ReadAsync(header, 0, 4);
                if (hRead < 4) return "ERROR: Header incomplete";

                int size = BitConverter.ToInt32(header, 0);

                // 3. Đọc Body (Xử lý đọc đủ số lượng byte của Tiếng Việt)
                byte[] body = new byte[size];
                int totalRead = 0;
                while (totalRead < size)
                {
                    int r = await stream.ReadAsync(body, totalRead, size - totalRead);
                    if (r == 0) break;
                    totalRead += r;
                }

                return Encoding.UTF8.GetString(body);
            }
            catch (Exception ex) { return "ERROR: " + ex.Message; }
        }
        // POST: Doctors/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // Gửi ID trực tiếp sang Server với lệnh DELETE_DOCTOR
            string res = await SendTcp("DELETE_DOCTOR", id.ToString());

            if (res == "SUCCESS")
            {
                TempData["Message"] = "Đã chuyển bác sĩ vào danh sách lưu trữ.";
            }
            else
            {
                TempData["Error"] = res;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Doctors/Archive
        public async Task<IActionResult> Archive()
        {
            // Gửi lệnh FILTER với tham số status=deleted sang TCP
            string res = await SendTcp("FILTER", "status=deleted");
            var deletedDoctors = JsonConvert.DeserializeObject<List<DoctorViewModel>>(res);
            return View(deletedDoctors);
        }

        // POST: Doctors/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            // Gửi ID sang Server kèm lệnh RESTORE_DOCTOR
            string result = await SendTcp("RESTORE_DOCTOR", id.ToString());

            if (result == "SUCCESS")
            {
                TempData["Message"] = "Khôi phục bác sĩ thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Lỗi khôi phục: " + result;
            return RedirectToAction(nameof(Archive));
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.DoctorId == id);
        }
    }
}
