using G8_HospitalManagerment_Project_PRN222.Models;
using G8_HospitalManagerment_Project_PRN222.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MailKit.Net.Smtp;
using MimeKit;

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
            // 1. Kiểm tra tính hợp lệ cơ bản (Required, EmailAddress...)
            if (!ModelState.IsValid)
            {
                ViewBag.Mode = "register";
                return View("Index", userInfo);
            }

            // 2. Kiểm tra khớp mật khẩu
            if (userInfo.Password != userInfo.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
                ViewBag.Mode = "register";
                return View("Index", userInfo);
            }

            // 3. Kiểm tra trùng lặp Email hoặc Số điện thoại trong DB
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userInfo.Email || u.Phone == userInfo.Phone);

            if (existingUser != null)
            {
                if (existingUser.Email == userInfo.Email)
                {
                    // Thêm lỗi vào ModelState để View hiển thị ở phần tổng hợp và dưới ô Email
                    ModelState.AddModelError("Email", "Email này đã được đăng ký sử dụng.");
                    ViewBag.RegisterError = "Email đã tồn tại!";
                }
                else if (existingUser.Phone == userInfo.Phone)
                {
                    // Thêm lỗi vào ModelState để hiển thị dưới ô Phone
                    ModelState.AddModelError("Phone", "Số điện thoại này đã được đăng ký sử dụng.");
                    ViewBag.RegisterError = "Số điện thoại đã tồn tại!";
                }

                ViewBag.Mode = "register";
                return View("Index", userInfo);
            }

            // 4. Tạo thực thể User
            var user = new User
            {
                Email = userInfo.Email,
                FirstName = userInfo.FirstName,
                LastName = userInfo.LastName,
                Phone = userInfo.Phone,
                Gender = userInfo.Gender,
                BirthDay = userInfo.BirthDay,
                Address = userInfo.Address,
                UserRoleId = 7,
                Verified = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 5. Tạo thông tin xác thực
            var auth = new Authentication
            {
                UserId = user.UserId,
                Password = HashPassword(userInfo.Password),
                AuthType = "local"
            };

            _context.Authentications.Add(auth);
            await _context.SaveChangesAsync();

            // 6. Quy trình xác thực Email
            var encodedEmail = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Email));
            var verifyLink = Url.Action("VerifyEmail", "Authentication", new { data = encodedEmail }, Request.Scheme);

            string htmlMessage = $@"
    <h2>Chào mừng bạn đến với ChildrenCare!</h2>
    <p>Vui lòng nhấn vào đường link bên dưới để xác thực tài khoản của bạn:</p>
    <a href='{verifyLink}'>Xác nhận Email ngay</a>";

            await SendEmailAsync(user.Email, "Xác thực tài khoản ChildrenCare", htmlMessage);

            // Sau đó mới redirect
            TempData["Email"] = user.Email;
            return RedirectToAction("VerifyNotice");
        }



private async Task SendEmailAsync(string email, string subject, string message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("ChildrenCare System", "your-gmail@gmail.com"));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("html") { Text = message };

        using (var client = new SmtpClient())
        {
            // Kết nối với server Gmail (Port 587 cho TLS)
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

            // Đăng nhập (Sử dụng App Password của Gmail, không phải mật khẩu chính)
            await client.AuthenticateAsync("he181465dangnguyenhieu@gmail.com", "wgml msqt spfo kfet");

            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }

    [HttpGet]
        public IActionResult VerifyNotice()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string data)
        {
            if (string.IsNullOrEmpty(data))
                return Content("Link không hợp lệ!");

            string email;

            try
            {
                var bytes = Convert.FromBase64String(data);
                email = Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return Content("Link không hợp lệ!");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return Content("User không tồn tại!");

            // ✅ verify
            user.Verified = true;
            await _context.SaveChangesAsync();

            // ✅ login luôn
            var roleName = await _context.UserRoles
                .Where(r => r.UserRoleId == user.UserRoleId)
                .Select(r => r.RoleName)
                .FirstOrDefaultAsync() ?? "Patient";

            await Storage(new StorageInfo
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserRoleId = user.UserRoleId,
                Email = user.Email,
                RoleName = roleName,
                AuthType = "local"
            });

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Mode = "login";
            return View("Index", new RegisterViewModel());
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(RegisterViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                ViewBag.LoginError = "Vui lòng nhập đầy đủ thông tin!";
                ViewBag.Mode = "login"; return View("Index", model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null) { ViewBag.LoginError = "Email không tồn tại!"; ViewBag.Mode = "login"; return View("Index", model); }

            if (!user.Verified)
            {
                ViewBag.LoginError = "Tài khoản chưa được xác thực email.";
                ViewBag.ShowResend = true; // Hiện nút Resend ở View
                ViewBag.UnverifiedEmail = user.Email;
                ViewBag.Mode = "login"; 
                return View("Index", model);
            }

            var auth = await _context.Authentications.FirstOrDefaultAsync(a => a.UserId == user.UserId && a.AuthType == "local");
            if (auth == null) { 
                ViewBag.LoginError = "Tài khoản này dùng Google!"; 
                ViewBag.Mode = "login"; 
                return View("Index", model); 
            }

            if (auth.Password != HashPassword(model.Password)) { 
                ViewBag.LoginError = "Sai mật khẩu!"; 
                ViewBag.Mode = "login"; 
                return View("Index", model); 
            }

            var roleName = await _context.UserRoles.Where(r => r.UserRoleId == user.UserRoleId).Select(r => r.RoleName).FirstOrDefaultAsync() ?? "Patient";
            await Storage(new StorageInfo { 
                UserId = user.UserId, 
                FirstName = user.FirstName, 
                LastName = user.LastName, 
                Email = user.Email, 
                RoleName = roleName, 
                AuthType = "local" });

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerifyEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null && !user.Verified)
            {
                var encodedEmail = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Email));
                var verifyLink = Url.Action("VerifyEmail", "Authentication", new { data = encodedEmail }, Request.Scheme);
                await SendEmailAsync(user.Email, "Gửi lại xác thực tài khoản", $"<p>Link mới: <a href='{verifyLink}'>Xác thực ngay</a></p>");
                TempData["ResendSuccess"] = "Đã gửi lại email xác thực thành công!";
            }
            ViewBag.Mode = "login";
            return View("Index", new RegisterViewModel { Email = email });
        }

        [HttpGet]
        public IActionResult LoginGoogle()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
                return RedirectToAction("Login", new { mode = "login" });

            var claims = result.Principal?.Identities.FirstOrDefault()?.Claims;

            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var firstName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
            var lastName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
            var providerKey = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            // 1. Tìm user theo email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // 2. Auto register
                user = new User
                {
                    Email = email,
                    FirstName = firstName ?? "Google",
                    LastName = lastName ?? "User",
                    UserRoleId = 7
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // 3. Kiểm tra Authentication (Google): 
            var googleAuth = await _context.Authentications
                .FirstOrDefaultAsync(x =>
                    x.UserId == user.UserId &&
                    x.AuthType == "google");

            if (googleAuth == null)
            {
                googleAuth = new Authentication
                {
                    UserId = user.UserId,
                    AuthType = "google",
                    Password = "",
                    ProviderKey = providerKey
                };

                _context.Authentications.Add(googleAuth);
                await _context.SaveChangesAsync();
            }

            // 4. Lấy role 1 lần (tối ưu)
            var roleName = await _context.UserRoles
                .Where(r => r.UserRoleId == user.UserRoleId)
                .Select(r => r.RoleName)
                .FirstOrDefaultAsync() ?? "Patient";

            // 5. Lưu session (Claims)
            await Storage(new StorageInfo
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserRoleId = user.UserRoleId,
                Email = user.Email,
                RoleName = roleName,
                AuthType = "google"
            });

            return RedirectToAction("Index", "Home");
        }


        public async Task Storage(StorageInfo storage)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, storage.UserId.ToString()),
        new Claim(ClaimTypes.Email, storage.Email),
        new Claim(ClaimTypes.Name, $"{storage.FirstName} {storage.LastName}"),
        new Claim(ClaimTypes.Role, storage.RoleName ?? "Patient"),
        new Claim("AuthType", storage.AuthType ?? "local")
    };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true, // giữ login
                    ExpiresUtc = DateTime.UtcNow.AddHours(3)
                });
        }

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Vui lòng nhập email!";
                return RedirectToAction("ForgotPassword");
            }

            // 1. Tìm user theo email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                TempData["Error"] = "Email không tồn tại trong hệ thống!";
                return RedirectToAction("ForgotPassword");
            }

            // 2. Lấy danh sách các phương thức đăng nhập
            var authMethods = await _context.Authentications
                .Where(a => a.UserId == user.UserId)
                .Select(a => a.AuthType)
                .ToListAsync();

            bool hasGoogle = authMethods.Contains("google");
            bool hasLocal = authMethods.Contains("local");

            // ❌ CHỈ GOOGLE → KHÔNG cho reset
            if (hasGoogle && !hasLocal)
            {
                TempData["IsGoogleOnly"] = true;
                TempData["Error"] = "Tài khoản này được quản lý bởi Google. Hãy đăng nhập bằng Google!";
                return RedirectToAction("ForgotPassword");
            }

            // ✅ CÓ LOCAL → cho reset
            if (hasLocal)
            {
                if (hasGoogle)
                {
                    TempData["Warning"] = "Bạn cũng có thể đăng nhập nhanh bằng Google.";
                }

                HttpContext.Session.SetString("ResetEmail", email);
                HttpContext.Session.SetString("ResetExpire", DateTime.UtcNow.AddMinutes(10).ToString());

                return RedirectToAction("ResetPassword");
            }

            TempData["Error"] = "Đã xảy ra lỗi không xác định.";
            return RedirectToAction("ForgotPassword");
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
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdStr == null)
            {
                return RedirectToAction("Login", "Authentication");
            }
            int userId = int.Parse(userIdStr);

            var isGoogle = _context.Authentications
                .Any(x => x.UserId == userId && x.AuthType == "google");

            ViewBag.IsGoogle = isGoogle;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdStr == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            int userId = int.Parse(userIdStr);

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

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CreatePassword()
        {
            // Kiểm tra xem đã Login chưa
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // Kiểm tra xem đã có password local chưa (tránh tạo đè)
            var existingAuth = await _context.Authentications
                .AnyAsync(a => a.UserId == userId && a.AuthType == "local");

            if (existingAuth)
            {
                return RedirectToAction("ChangePassword"); // Nếu có rồi thì sang trang đổi pass
            }

            // Tạo mới bản ghi Authentication Local
            var newAuth = new Authentication
            {
                UserId = userId,
                AuthType = "local",
                Password = HashPassword(model.NewPassword) // Dùng hàm HashPassword của bạn
            };

            _context.Authentications.Add(newAuth);
            await _context.SaveChangesAsync();

            var user = _context.Users.FirstOrDefault(x => x.UserId == userId);
            user.Verified = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thiết lập mật khẩu thành công!";
            return RedirectToAction("Profile", "Users");
        }
    }
}
