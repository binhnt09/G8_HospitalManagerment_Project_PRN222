using G8_HospitalManagerment_Project_PRN222.Models;
using G8_HospitalManagerment_Project_PRN222.Models.ViewModels;
using G8_HospitalManagerment_Project_PRN222.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace G8_HospitalManagerment_Project_PRN222.Controllers.AuthenticationController
{
    public class UsersController : Controller
    {
        private readonly DbHospitalManagementContext _context;

        public UsersController(DbHospitalManagementContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var dbHospitalManagementContext = _context.Users.Include(u => u.UserRole);
            return View(await dbHospitalManagementContext.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["UserRoleId"] = new SelectList(_context.UserRoles, "UserRoleId", "UserRoleId");
            return View();
        }

        // Đăng Kí


        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["UserRoleId"] = new SelectList(_context.UserRoles, "UserRoleId", "UserRoleId", user.UserRoleId);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,FirstName,LastName,Email,Phone,Gender,BirthDay,Address,UserRoleId,CreatedAt,UpdatedAt,DeletedAt,IsDeleted")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
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
            ViewData["UserRoleId"] = new SelectList(_context.UserRoles, "UserRoleId", "UserRoleId", user.UserRoleId);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }



        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return RedirectToAction("Login", "Authentication");

            int userId = int.Parse(userIdStr);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            // Kiểm tra các phương thức đăng nhập của user này
            var auths = await _context.Authentications.Where(a => a.UserId == userId).ToListAsync();

            ViewBag.IsGoogle = auths.Any(a => a.AuthType == "google");
            ViewBag.HasLocalPass = auths.Any(a => a.AuthType == "local" && !string.IsNullOrEmpty(a.Password));

            return View(user);
        }


        [HttpGet]
        [Authorize]
        public IActionResult EditProfile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            int userId = int.Parse(userIdStr);
            var user = _context.Users.Find(userId);

            if (user == null) return NotFound();

            // 🔥 SỬA: Chuyển từ kiểu 'User' sang 'EditProfile' trước khi trả về View
            var model = new EditProfile
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Gender = user.Gender,
                BirthDay = user.BirthDay,
                Address = user.Address
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfile model)
        {
            if (!ModelState.IsValid) return View(model);

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return RedirectToAction("Login", "Authentication");

            int userId = int.Parse(userIdStr);
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                // Cập nhật thông tin từ Model vào Database
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Phone = model.Phone;
                user.Gender = model.Gender;
                user.BirthDay = model.BirthDay;
                user.Address = model.Address;
                user.UpdatedAt = DateTime.Now;

                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật hồ sơ cá nhân thành công!";
                return RedirectToAction("Profile"); // Quay lại trang xem thông tin
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            TempData["EnteredEmail"] = email;

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Vui lòng nhập email!";
                return RedirectToAction("ForgotPassword");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                TempData["Error"] = "Email không tồn tại trong hệ thống!";
                return RedirectToAction("ForgotPassword");
            }

            // Kiểm tra tài khoản Google...
            var authMethods = await _context.Authentications
                .Where(a => a.UserId == user.UserId)
                .Select(a => a.AuthType)
                .ToListAsync();

            if (authMethods.Contains("google") && !authMethods.Contains("local"))
            {
                TempData["IsGoogleOnly"] = true;
                TempData["Error"] = "Tài khoản này dùng Google. Hãy đăng nhập bằng Google!";
                return RedirectToAction("ForgotPassword");
            }

            // Nếu mọi thứ hợp lệ, chuyển sang trang ResetPassword
            if (authMethods.Contains("local"))
            {
                HttpContext.Session.SetString("ResetEmail", email);
                return RedirectToAction("ResetPassword");
            }

            return RedirectToAction("ForgotPassword");
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
    }
}
