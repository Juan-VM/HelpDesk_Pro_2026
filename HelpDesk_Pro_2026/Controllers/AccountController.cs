using HelpDesk_Pro_2026.Models;
using HelpDesk_Pro_2026.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk_Pro_2026.Controllers
{
    public class AccountController : Controller
    {
        private const string DEFAULT_PHOTO_URL =
            "AQUI_VA_LA_URL_DE_DEFAULT_USER";

        // ==========================================
        // INDEX
        // ==========================================

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ==========================================
        // LOGIN - GET
        // ==========================================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ==========================================
        // LOGIN - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = await SupabClient.GetClient();

                var authService = new AuthService(client);

                var session = await authService.LoginAsync(
                    model.Email,
                    model.Password
                );

                if (session == null || session.User == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Correo o contraseña incorrectos."
                    );

                    return View(model);
                }

                Guid userId = Guid.Parse(session.User.Id);

                var userService = new UserService(client);

                var usuario = await userService.ObtenerPorId(userId);

                if (usuario == null)
                {
                    ModelState.AddModelError(
                        "",
                        "El usuario está autenticado pero no tiene un perfil registrado."
                    );

                    return View(model);
                }

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

                HttpContext.Session.SetString(
                    "PhotoUrl",
                    usuario.PhotoUrl ?? ""
                );

                HttpContext.Session.SetString(
                    "AccessToken",
                    session.AccessToken
                );

                HttpContext.Session.SetString(
                    "RefreshToken",
                    session.RefreshToken
                );

                // ✅ REDIRECT TO FAST TICKETS PAGE AFTER LOGIN
                return RedirectToAction(
                    "Index",
                    "Tickets"
                );
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    "Error al iniciar sesión. Intentalo nuevamente... "
                );

                return View(model);
            }
        }

        // ==========================================
        // REGISTER - GET
        // ==========================================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ==========================================
        // REGISTER - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = await SupabClient.GetClient();

                var authService = new AuthService(client);

                var session = await authService.RegisterAsync(
                     model.Email,
                     model.Password,
                     model.Name
                 );

                if (session == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Supabase no devolvió una sesión después del registro."
                    );

                    return View(model);
                }
                if (session.User == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Supabase no devolvió el usuario."
                    );

                    return View(model);
                }

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    "Ocurrió un error: " + ex.Message
                );

                return View(model);
            }
        }

        // ==========================================
        // LOGOUT
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var client = await SupabClient.GetClient();

                var authService = new AuthService(client);

                await authService.LogoutAsync();
            }
            catch (Exception ex)
            {
                // No impedimos el cierre de sesión local
                Console.WriteLine("Error al cerrar sesión en Supabase: " + ex.Message);
            }

            // Limpiar sesión local
            HttpContext.Session.Clear();

            // Volver al Login
            return RedirectToAction("Login", "Account");
        }
    }
}