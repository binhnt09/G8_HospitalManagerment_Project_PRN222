using G8_HospitalManagerment_Project_PRN222_Server.Models;
using G8_HospitalManagerment_Project_PRN222_Server.Models.ViewModels;
using G8_HospitalManagerment_Project_PRN222_Server.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace G8_HospitalManagerment_Project_PRN222_Server.Services
{
    public class TcpServerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TcpListener _listener;

        public TcpServerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _listener = new TcpListener(IPAddress.Any, 13000);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _listener.Start();
            TcpMonitor.AddLog("Server Export đang chạy tại port 13000...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync(stoppingToken);
                    _ = Task.Run(() => HandleExport(client, stoppingToken), stoppingToken);
                }
                catch (Exception ex) { TcpMonitor.AddLog($"Lỗi: {ex.Message}"); }
            }
        }

        private async Task HandleExport(TcpClient client, CancellationToken stoppingToken)
        {
            using (client)
            {
                try
                {
                    var stream = client.GetStream();
                    // Buffer lớn để chứa dữ liệu Tiếng Việt và JSON dài
                    byte[] buffer = new byte[10240];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
                    if (bytesRead == 0) return;

                    // Giải mã UTF-8 để giữ nguyên dấu Tiếng Việt
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Tách lệnh và dữ liệu bằng dấu |
                    int separatorIndex = request.IndexOf('|');
                    if (separatorIndex == -1) return;

                    string command = request.Substring(0, separatorIndex);
                    string jsonData = request.Substring(separatorIndex + 1);

                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<DbHospitalManagementContext>();

                    // ================================================================
                    // 1. LỆNH CREATE_DOCTOR (Thêm mới liên hoàn)
                    // ================================================================
                    if (command == "CREATE_DOCTOR")
                    {
                        var data = JsonConvert.DeserializeObject<DoctorViewModel>(jsonData);
                        using var transaction = await context.Database.BeginTransactionAsync(stoppingToken);
                        try
                        {
                            // Logic tự sinh mã NV: [Khoa][Tên] (Không dấu)
                            string rawPart = RemoveDiacritics(data.Specialization + data.FullName).Replace(" ", "").ToUpper();
                            data.EmployeeCode = await GenerateUniqueCode(context, rawPart);

                            // Bước A: Tạo Employee
                            var emp = new Employee
                            {
                                EmployeeCode = data.EmployeeCode,
                                DepartmentId = data.DepartmentId,
                                UserId = data.UserId,
                                Position = "Doctor",
                                WorkStatus = "Active",
                                HireDate = DateTime.Now,
                                CreatedAt = DateTime.Now
                            };
                            context.Employees.Add(emp);
                            await context.SaveChangesAsync(stoppingToken);

                            // Bước B: Tạo Doctor liên kết EmployeeID vừa tạo
                            var doc = new Doctor
                            {
                                EmployeeId = emp.EmployeeId,
                                Specialization = data.Specialization,
                                YearsExperience = data.YearsExperience,
                                LicenseNumber = data.LicenseNumber,
                                CreatedAt = DateTime.Now
                            };
                            context.Doctors.Add(doc);
                            await context.SaveChangesAsync(stoppingToken);

                            await transaction.CommitAsync(stoppingToken);
                            // Trả về SUCCESS kèm mã NV vừa sinh để Client biết
                            await SendResponse(stream, $"SUCCESS|{data.EmployeeCode}");
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync(stoppingToken);
                            await SendResponse(stream, "ERROR|" + (ex.InnerException?.Message ?? ex.Message));
                        }
                        return;
                    }

                    // ================================================================
                    // 2. LỆNH UPDATE_DOCTOR (Cập nhật thông tin)
                    // ================================================================
                    else if (command == "UPDATE_DOCTOR")
                    {
                        // Giải mã dữ liệu JSON nhận được từ Client thành ViewModel
                        var data = JsonConvert.DeserializeObject<DoctorViewModel>(jsonData);

                        // Validate cơ bản ở Server: Phải có DoctorId
                        if (data.DoctorId <= 0)
                        {
                            await SendResponse(stream, "ERROR|DoctorId không hợp lệ");
                            return;
                        }

                        // Tìm Doctor trong DB, Include cả bảng Employee liên quan
                        var doctor = await context.Doctors.Include(d => d.Employee)
                                                 .FirstOrDefaultAsync(d => d.DoctorId == data.DoctorId, stoppingToken);

                        if (doctor != null)
                        {
                            try
                            {
                                // --- CẬP NHẬT CÁC TRƯỜNG ĐƯỢC PHÉP SỬA ---

                                // 1. Cập nhật bảng Doctor
                                doctor.Specialization = data.Specialization;
                                doctor.YearsExperience = data.YearsExperience;
                                doctor.LicenseNumber = data.LicenseNumber;
                                doctor.UpdatedAt = DateTime.Now; // Cập nhật thời gian sửa

                                // 2. Cập nhật bảng Employee liên quan
                                // Lưu ý: KHÔNG cập nhật EmployeeCode để giữ tính nhất quán định danh
                                doctor.Employee.DepartmentId = data.DepartmentId;
                                doctor.Employee.UpdatedAt = DateTime.Now;

                                // Lưu thay đổi vào Database
                                await context.SaveChangesAsync(stoppingToken);

                                // Phản hồi thành công về Client
                                await SendResponse(stream, "SUCCESS");
                            }
                            catch (Exception ex)
                            {
                                // Trả về lỗi chi tiết (nếu có lỗi ràng buộc DB...)
                                await SendResponse(stream, "ERROR|Lỗi DB: " + (ex.InnerException?.Message ?? ex.Message));
                            }
                        }
                        else
                        {
                            // Không tìm thấy bác sĩ với ID yêu cầu
                            await SendResponse(stream, "ERROR|Không tìm thấy bác sĩ với ID: " + data.DoctorId);
                        }
                        return;
                    }
                    // --- Thêm vào switch/if-else trong hàm HandleExport ---

                    // LỆNH XÓA MỀM (DELETE)
                    else if (command == "DELETE_DOCTOR")
                    {
                        int docId = int.Parse(jsonData); // jsonData lúc này chỉ là cái ID số
                        var doctor = await context.Doctors.Include(d => d.Employee)
                                                 .FirstOrDefaultAsync(d => d.DoctorId == docId);

                        if (doctor != null)
                        {
                            doctor.IsDeleted = true;
                            doctor.Employee.IsDeleted = true; // Xóa luôn nhân viên tương ứng
                            doctor.DeletedAt = DateTime.Now;

                            await context.SaveChangesAsync();
                            await SendResponse(stream, "SUCCESS");
                        }
                        else
                        {
                            await SendResponse(stream, "ERROR|Không tìm thấy ID");
                        }
                        return;
                    }

                    // LỆNH KHÔI PHỤC (RESTORE)
                    else if (command == "RESTORE_DOCTOR")
                    {
                        int docId = int.Parse(jsonData);
                        var doctor = await context.Doctors.Include(d => d.Employee)
                                                 .IgnoreQueryFilters() // Để tìm được bản ghi đã xóa
                                                 .FirstOrDefaultAsync(d => d.DoctorId == docId);

                        if (doctor != null)
                        {
                            doctor.IsDeleted = false;
                            doctor.Employee.IsDeleted = false;
                            doctor.DeletedAt = null;

                            await context.SaveChangesAsync();
                            await SendResponse(stream, "SUCCESS");
                        }
                        return;
                    }
                    else if (command == "EXPORT_EXCEL")
                    {
                        try
                        {
                            var list = await context.Doctors.Include(d => d.Employee).ThenInclude(e => e.User)
                                                           .Where(d => d.IsDeleted != true).ToListAsync(stoppingToken);

                            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                            using (var package = new ExcelPackage())
                            {
                                var ws = package.Workbook.Worksheets.Add("DoctorsReport");
                                ws.Cells["A1"].Value = "Mã NV"; ws.Cells["B1"].Value = "Họ Tên"; ws.Cells["C1"].Value = "Chuyên khoa";
                                ws.Cells["D1"].Value = "Kinh nghiệm"; ws.Cells["E1"].Value = "Số GP";

                                using (var r = ws.Cells["A1:E1"]) { r.Style.Font.Bold = true; r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid; r.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue); }

                                int row = 2;
                                foreach (var d in list)
                                {
                                    ws.Cells[row, 1].Value = d.Employee?.EmployeeCode;
                                    ws.Cells[row, 2].Value = d.Employee?.User?.FirstName + " " + d.Employee?.User?.LastName;
                                    ws.Cells[row, 3].Value = d.Specialization;
                                    ws.Cells[row, 4].Value = d.YearsExperience + " năm";
                                    ws.Cells[row, 5].Value = d.LicenseNumber;
                                    row++;
                                }
                                ws.Cells.AutoFitColumns();

                                byte[] fileBytes = package.GetAsByteArray();
                                // Gửi: [4 byte độ dài] + [Dữ liệu file]
                                await stream.WriteAsync(BitConverter.GetBytes(fileBytes.Length), 0, 4);
                                await stream.WriteAsync(fileBytes, 0, fileBytes.Length);
                                await stream.FlushAsync();
                                await Task.Delay(500); // Chờ Client nhận xong mới ngắt
                            }
                        }
                        catch (Exception ex) { TcpMonitor.AddLog("Lỗi Export: " + ex.Message); }
                        return;
                    }
                    else if (command == "IMPORT_EXCEL")
                    {
                        try
                        {
                            // 1. Đọc độ dài file (4 byte tiếp theo trong luồng)
                            byte[] sizeBuffer = new byte[4];
                            await ReadExactly(stream, sizeBuffer, 4);
                            int size = BitConverter.ToInt32(sizeBuffer, 0);

                            TcpMonitor.AddLog($"[TCP] Nhận lệnh Import file: {size} bytes");

                            // 2. Đọc toàn bộ dữ liệu file Excel
                            byte[] fileBytes = new byte[size];
                            await ReadExactly(stream, fileBytes, size);

                            // 3. Xử lý Excel bằng EPPlus
                            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                            using var ms = new MemoryStream(fileBytes);
                            using var package = new ExcelPackage(ms);

                            var ws = package.Workbook.Worksheets.First();
                            int row = 2;
                            int importedCount = 0;

                            // Dùng Transaction để đảm bảo tính toàn vẹn (lỗi 1 dòng là hủy cả file)
                            using var transaction = await context.Database.BeginTransactionAsync(stoppingToken);
                            try
                            {
                                while (true)
                                {
                                    var empCode = ws.Cells[row, 1].Text.Trim();
                                    var fullName = ws.Cells[row, 2].Text.Trim();
                                    var specialization = ws.Cells[row, 3].Text.Trim();
                                    var expText = ws.Cells[row, 4].Text.Trim();
                                    var license = ws.Cells[row, 5].Text.Trim();

                                    // Dừng lại nếu dòng đó trống
                                    if (string.IsNullOrEmpty(empCode) || string.IsNullOrEmpty(fullName)) break;

                                    // Kiểm tra xem bác sĩ này đã tồn tại chưa (theo Số GP)
                                    bool exists = await context.Doctors.AnyAsync(d => d.LicenseNumber == license);
                                    if (!exists)
                                    {
                                        // --- BƯỚC 1: TẠO USER ---
                                        var nameParts = fullName.Split(' ');
                                        var newUser = new User
                                        {
                                            FirstName = nameParts[0],
                                            LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : " ",
                                            Email = empCode.ToLower() + "@hospital.com", // Tự sinh Email
                                            UserRoleId = 2, // Role Doctor
                                            Verified = true,
                                            CreatedAt = DateTime.Now
                                        };
                                        context.Users.Add(newUser);
                                        await context.SaveChangesAsync();

                                        // --- BƯỚC 2: TẠO MẬT KHẨU (123456) ---
                                        var auth = new Authentication
                                        {
                                            UserId = newUser.UserId,
                                            Password = "lsrjXOipsCRBeL8o5JZsLOG4OFcjqWprg4hYzdbKCh4=", // Hash của 123456
                                            AuthType = "local",
                                            ProviderKey = newUser.Email
                                        };
                                        context.Authentications.Add(auth);

                                        // --- BƯỚC 3: TẠO EMPLOYEE ---
                                        var emp = new Employee
                                        {
                                            UserId = newUser.UserId,
                                            EmployeeCode = empCode,
                                            DepartmentId = 1, // Mặc định khoa đầu tiên
                                            Position = "Doctor",
                                            WorkStatus = "Active",
                                            HireDate = DateTime.Now
                                        };
                                        context.Employees.Add(emp);
                                        await context.SaveChangesAsync();

                                        // --- BƯỚC 4: TẠO DOCTOR ---
                                        int.TryParse(expText.Replace("năm", "").Trim(), out int exp);
                                        var doc = new Doctor
                                        {
                                            EmployeeId = emp.EmployeeId,
                                            Specialization = specialization,
                                            YearsExperience = exp,
                                            LicenseNumber = license,
                                            IsDeleted = false
                                        };
                                        context.Doctors.Add(doc);

                                        importedCount++;
                                    }
                                    row++;
                                }
                                await context.SaveChangesAsync();
                                await transaction.CommitAsync();

                                await SendResponse(stream, $"SUCCESS|Đã import thành công {importedCount} bác sĩ.");
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                throw ex;
                            }
                        }
                        catch (Exception ex)
                        {
                            TcpMonitor.AddLog("Lỗi Import: " + ex.Message);
                            await SendResponse(stream, "ERROR|" + ex.Message);
                        }
                        return;
                    }


                    // ================================================================
                    // 3. LỆNH FILTER (Lấy danh sách / Tìm kiếm)
                    // ================================================================
                    var query = context.Doctors.Include(d => d.Employee).ThenInclude(e => e.User)
                                       .Where(d => d.IsDeleted != true).AsQueryable();

                    if (command == "FILTER")
                    {
                        // Tách các tiêu chí lọc: name=A;spec=B;exp=C
                        var criteria = jsonData.Split(';');
                        bool showDeleted = criteria.Any(c => c.ToLower() == "status=deleted");

                        if (showDeleted)
                        {
                            // Nếu yêu cầu xem hàng đã xóa, ta bỏ lọc IsDeleted
                            query = context.Doctors.IgnoreQueryFilters()
                                           .Include(d => d.Employee).ThenInclude(e => e.User)
                                           .Where(d => d.IsDeleted == true);
                        }
                        foreach (var item in criteria)
                        {
                            var kv = item.Split('=');
                            if (kv.Length < 2 || string.IsNullOrEmpty(kv[1]) || kv[1] == "0") continue;

                            if (kv[0] == "name") query = query.Where(d => (d.Employee.User.FirstName + d.Employee.User.LastName).Contains(kv[1]));
                            if (kv[0] == "spec") query = query.Where(d => d.Specialization == kv[1]);
                            if (kv[0] == "exp") query = query.Where(d => d.YearsExperience >= int.Parse(kv[1]));
                        }
                    }

                    var doctors = await query.Select(d => new DoctorViewModel
                    {
                        DoctorId = d.DoctorId,
                        FullName = d.Employee.User.FirstName + " " + d.Employee.User.LastName,
                        Specialization = d.Specialization,
                        YearsExperience = d.YearsExperience,
                        LicenseNumber = d.LicenseNumber,
                        EmployeeCode = d.Employee.EmployeeCode,
                        DepartmentId = d.Employee.DepartmentId,
                        UserId = d.Employee.UserId
                    }).ToListAsync(stoppingToken);

                    string responseJson = JsonConvert.SerializeObject(doctors);
                    byte[] responseData = Encoding.UTF8.GetBytes(responseJson);
                    await stream.WriteAsync(BitConverter.GetBytes(responseData.Length), 0, 4);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                    await stream.FlushAsync();
                }
                catch (Exception ex) { TcpMonitor.AddLog($"Lỗi: {ex.Message}"); }
            }
        }

        // HÀM HỖ TRỢ 1: Xử lý tăng mã tự động khi trùng (VD: NOIKHOA1, NOIKHOA2...)
        private async Task<string> GenerateUniqueCode(DbHospitalManagementContext context, string baseCode)
        {
            int counter = 0;
            string finalCode = baseCode;
            while (await context.Employees.AnyAsync(e => e.EmployeeCode == finalCode))
            {
                counter++;
                finalCode = baseCode + counter;
            }
            return finalCode;
        }

        // HÀM HỖ TRỢ 2: Xóa dấu Tiếng Việt (Để mã NV không bị lỗi font hệ thống)
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace("đ", "d").Replace("Đ", "D");
        }

        // HÀM HỖ TRỢ 3: Gửi phản hồi chuẩn qua TCP
        private async Task SendResponse(NetworkStream stream, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(BitConverter.GetBytes(data.Length), 0, 4);
            await stream.WriteAsync(data, 0, data.Length);
            await stream.FlushAsync();
        }



        // IP EP
        private async Task ReadExactly(NetworkStream stream, byte[] buffer, int size)
        {
            int read = 0;
            while (read < size)
            {
                int r = await stream.ReadAsync(buffer, read, size - read);
                if (r == 0) throw new Exception("Mất kết nối");
                read += r;
            }
        }
    }
}