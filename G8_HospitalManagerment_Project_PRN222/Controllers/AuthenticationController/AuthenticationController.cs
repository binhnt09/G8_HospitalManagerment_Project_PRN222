using G8_HospitalManagerment_Project_PRN222.Models;
using G8_HospitalManagerment_Project_PRN222.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel userInfo)
        {
            if (!ModelState.IsValid)
            {
                return View(userInfo);
            }
            if (userInfo.Password != userInfo.ConfirmPassword)
            {
                ViewBag.RegisterError = "Mật khẩu không khớp";
                return View(userInfo);
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
            _context.SaveChanges();

            var authentication = new Authentication
            {
                UserId = user.UserId,
                Password = userInfo.Password,
                AuthType = "local",
                ProviderKey = ""
            };

            _context.Authentications.Add(authentication);
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }
    }
}
