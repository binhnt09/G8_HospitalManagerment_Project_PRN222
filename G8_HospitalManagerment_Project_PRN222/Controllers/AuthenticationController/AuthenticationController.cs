using G8_HospitalManagerment_Project_PRN222.Models;
using G8_HospitalManagerment_Project_PRN222.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace G8_HospitalManagerment_Project_PRN222.Controllers.AuthenticationController
{
    public class AuthenticationController : Controller
    {
        private DbHospitalManagementContext _context;

        public AuthenticationController(DbHospitalManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string mode = "login")
        {
            // Truyền mode sang View qua ViewBag để hiển thị đúng Form 
            ViewBag.Mode = mode;
            return View(new RegisterViewModel());
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Mode = "register";
            return View("Index", new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel userInfo)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Mode = "register";
                return View("Index", userInfo);
            }
            if (userInfo.Password != userInfo.ConfirmPassword)
            {
                ViewBag.RegisterError = "Mật khẩu không khớp";
                ViewBag.Mode = "register";
                return View("Index", userInfo);
            }
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == userInfo.Email);
            if (existingUser != null)
            {
                ViewBag.RegisterError = "Email đã tồn tại!";
                ViewBag.Mode = "register";
                return View("Index", userInfo);
            }

            var user = new User
            {
                Email = userInfo.Email,
                FirstName = userInfo.FirstName,
                LastName = userInfo.LastName,
                Phone = userInfo.Phone,
                Gender = userInfo.Gender,
                BirthDay = userInfo.BirthDay,
                Address = userInfo.Address,
                UserRoleId = 1
            };

            _context.Users.Add(user);

            var authentication = new Authentication
            {
                UserId = user.UserId,
                Password = HashPassword(userInfo.Password),
                AuthType = "local",
                ProviderKey = ""
            };

            _context.Authentications.Add(authentication);
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Mode = "login";
            return View("Index", new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(RegisterViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                ViewBag.LoginError = "Vui lòng nhập đầy đủ thông tin!";
                ViewBag.Mode = "login";
                return View("Index", model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ViewBag.LoginError = "Email không tồn tại!";
                ViewBag.Mode = "login";
                return View("Index", model);
            }

            var auth = await _context.Authentications
                .FirstOrDefaultAsync(a => a.UserId == user.UserId);

            if (auth == null)
            {
                ViewBag.LoginError = "Tài khoản chưa có thông tin xác thực!";
                ViewBag.Mode = "login";
                return View("Index", model);
            }

            // So sánh password đã hash
            if (auth.Password != HashPassword(model.Password))
            {
                ViewBag.LoginError = "Sai mật khẩu!";
                ViewBag.Mode = "login";
                return View("Index", model);
            }

            // 👉 Lưu session (đăng nhập thành công)
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetInt32("UserId", user.UserId);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng nhập email!";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                ViewBag.Error = "Email không tồn tại!";
                return View();
            }

            HttpContext.Session.SetString("ResetEmail", email);

            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var email = HttpContext.Session.GetString("ResetEmail");

            if (email == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(string newPassword, string confirmPassword)
        {
            var email = HttpContext.Session.GetString("ResetEmail");

            if (email == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ!";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu không khớp!";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            var auth = _context.Authentications
                .FirstOrDefault(a => a.UserId == user.UserId);

            if (auth == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản!";
                return View();
            }

            auth.Password = HashPassword(newPassword);

            _context.SaveChanges();

            // 👉 Xóa session sau khi dùng
            HttpContext.Session.Remove("ResetEmail");

            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(oldPassword) ||
                string.IsNullOrEmpty(newPassword) ||
                string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ!";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu mới không khớp!";
                return View();
            }

            var auth = _context.Authentications
                .FirstOrDefault(a => a.UserId == userId);

            if (auth == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản!";
                return View();
            }

            // ✅ Check password cũ
            if (auth.Password != HashPassword(oldPassword))
            {
                ViewBag.Error = "Mật khẩu cũ không đúng!";
                return View();
            }

            // ✅ Update password
            auth.Password = HashPassword(newPassword);

            _context.SaveChanges();

            ViewBag.Success = "Đổi mật khẩu thành công!";

            return View();
        }
    }
}
