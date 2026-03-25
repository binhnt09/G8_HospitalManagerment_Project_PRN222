using G8_HospitalManagerment_Project_PRN222.Models;
using G8_HospitalManagerment_Project_PRN222.Models.ViewModels;
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

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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



        [Authorize(Roles = "1")]
        [HttpGet]
        public IActionResult Profile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdStr == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            int userId = int.Parse(userIdStr);

            var user = _context.Users
                .Include(u => u.UserRole)
                .FirstOrDefault(u => u.UserId == userId);

            if (string.IsNullOrEmpty(user.Address))
            {
                user.Address = "Không có";
            }

            return View(user);
        }


        [HttpGet]
        public IActionResult EditProfile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdStr == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            int userId = int.Parse(userIdStr);

            var user = _context.Users.Find(userId);

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfile model)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdStr == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            int userId = int.Parse(userIdStr);

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Phone = model.Phone;
            user.Gender = model.Gender;
            user.BirthDay = model.BirthDay;
            user.Address = model.Address;
            _context.SaveChanges();

            await Storage(user);

            return RedirectToAction("Profile");
        }
        public async Task Storage(User user)
        {
            var claimsCookie = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // ID hệ thống
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
                new Claim(ClaimTypes.Role, user.UserRoleId.ToString()) // nếu có role
            };

            var identity = new ClaimsIdentity(
                claimsCookie,
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );
        }
    }
}
