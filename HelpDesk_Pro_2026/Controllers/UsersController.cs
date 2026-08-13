using HelpDesk_Pro_2026.Models;
using HelpDesk_Pro_2026.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk_Pro_2026.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserService _userService;
        private readonly StorageService _storageService;

        public UsersController(
            UserService userService,
            StorageService storageService)
        {
            _userService = userService;
            _storageService = storageService;
        }

        // GET: /Users/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var usuario = await _userService.ObtenerPorId(
                Guid.Parse(userId)
            );

            if (usuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(usuario);
        }

        // GET: /Users/EditProfile
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var usuario = await _userService.ObtenerPorId(
                Guid.Parse(userId)
            );

            if (usuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new EditProfileViewModel
            {
                FullName = usuario.FullName,
                Email = usuario.Email
            };

            return View(model);
        }
    }
}