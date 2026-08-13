using HelpDesk_Pro_2026.Models;
using HelpDesk_Pro_2026.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk_Pro_2026.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;

        public AccountController(
            UserService userService,
            AuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        // GET: /Account/Index
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                bool registrado = await _userService.RegistrarUsuario(
                    model.Name,
                    model.Email,
                    model.Password
                );

                if (!registrado)
                {
                    ViewBag.Error = "Ya existe un usuario con ese correo.";
                    return View(model);
                }

                TempData["Success"] =
                    "Cuenta creada correctamente. Ahora puedes iniciar sesión.";

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error: " + ex.Message;

                return View(model);
            }
        }


        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var usuario = await _authService.Login(
                    model.Email,
                    model.Password
                );

                if (usuario == null)
                {
                    ViewBag.Error =
                        "El correo o la contraseña son incorrectos.";

                    return View(model);
                }

                // Guardar información del usuario en sesión
                HttpContext.Session.SetString(
                    "UserId",
                    usuario.UserId.ToString()
                );

                HttpContext.Session.SetString(
                    "FullName",
                    usuario.FullName
                );

                HttpContext.Session.SetString(
                    "Email",
                    usuario.Email
                );

                HttpContext.Session.SetString(
                    "Role",
                    usuario.Role
                );

                // Redirección después del login
                return RedirectToAction("Dashboard", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                    "Ocurrió un error al iniciar sesión: "
                    + ex.Message;

                return View(model);
            }
        }


        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }
    }
}