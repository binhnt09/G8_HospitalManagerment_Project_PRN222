using G8_HospitalManagerment_Project_PRN222_Server.Models;
using G8_HospitalManagerment_Project_PRN222_Server.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

                    // 1. ĐỌC LỆNH TỪ CLIENT (Ví dụ: "FILTER|name=Anh;spec=Nhi;exp=5")
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    TcpMonitor.AddLog($"Yêu cầu nhận được: {request}");

                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<DbHospitalManagementContext>();

                    // Khởi tạo Query gốc
                    var query = context.Doctors
                        .Include(d => d.Employee).ThenInclude(e => e.User)
                        .Where(d => d.IsDeleted != true)
                        .AsQueryable();

                    // 2. PHÂN TÍCH LỆNH VÀ LỌC (FILTER)
                    if (request.StartsWith("FILTER|"))
                    {
                        string filterContent = request.Split('|')[1]; // Lấy phần sau "FILTER|"
                        var criteria = filterContent.Split(';'); // Tách name, spec, exp

                        foreach (var item in criteria)
                        {
                            var kv = item.Split('=');
                            if (kv.Length < 2) continue;

                            string key = kv[0].ToLower();
                            string val = kv[1];

                            if (string.IsNullOrEmpty(val) || val == "null" || val == "0") continue;

                            if (key == "name")
                                query = query.Where(d => (d.Employee.User.FirstName + " " + d.Employee.User.LastName).Contains(val));

                            else if (key == "spec")
                                query = query.Where(d => d.Specialization.Contains(val));

                            else if (key == "exp")
                                query = query.Where(d => d.YearsExperience >= int.Parse(val));
                        }
                    }

                    // 3. THỰC THI TRUY VẤN
                    var doctors = await query.Select(d => new {
                        DoctorId = d.DoctorId,
                        FullName = d.Employee.User.FirstName + " " + d.Employee.User.LastName,
                        Specialization = d.Specialization,
                        YearsExperience = d.YearsExperience,
                        LicenseNumber = d.LicenseNumber,
                        EmployeeCode = d.Employee.EmployeeCode,
                        CreatedAt = d.CreatedAt,
                        IsDeleted = d.IsDeleted
                    }).ToListAsync(stoppingToken);

                    // 4. GỬI DỮ LIỆU VỀ
                    string json = JsonConvert.SerializeObject(doctors);
                    byte[] data = Encoding.UTF8.GetBytes(json);

                    // Gửi 4 byte độ dài (Header)
                    await stream.WriteAsync(BitConverter.GetBytes(data.Length), 0, 4, stoppingToken);
                    // Gửi dữ liệu JSON (Body)
                    await stream.WriteAsync(data, 0, data.Length, stoppingToken);
                    await stream.FlushAsync(stoppingToken);

                    TcpMonitor.AddLog($"Đã gửi {doctors.Count} bác sĩ kết quả lọc.");
                }
                catch (Exception ex) { TcpMonitor.AddLog($"Lỗi xử lý TCP: {ex.Message}"); }
            }
        }
    }
}