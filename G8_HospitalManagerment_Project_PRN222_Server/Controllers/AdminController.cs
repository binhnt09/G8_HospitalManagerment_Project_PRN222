using G8_HospitalManagerment_Project_PRN222_Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace G8_HospitalManagerment_Project_PRN222_Server.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            // Lấy danh sách Log từ Static class
            var logs = TcpMonitor.Logs;
            return View(logs);
        }
    }
}
