using HelpDesk_Pro_2026.Models;
using HelpDesk_Pro_2026.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk_Pro_2026.Controllers
{
    public class AdminUsersController : Controller
    {
        private readonly UserService _userService;

        public AdminUsersController(UserService userService)
        {
            _userService = userService;
        }


        // ==========================================
        // ADMINISTRAR USUARIOS
        // GET: /AdminUsers
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // ==========================================
            // VERIFICAR SESIÓN
            // ==========================================

            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }


            // ==========================================
            // VERIFICAR SUPERUSUARIO
            // ==========================================

            if (role != "SUPERUSUARIO")
            {
                return RedirectToAction("Dashboard", "Home");
            }


            // ==========================================
            // OBTENER USUARIOS
            // ==========================================

            try
            {
                var usuarios = await _userService.ObtenerUsuarios();

                return View(usuarios);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "No se pudieron obtener los usuarios.";

                return View(new List<Usuario>());
            }
        }


        // ==========================================
        // CAMBIAR ROL
        // POST: /AdminUsers/UpdateRole
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(
            Guid userId,
            string role)
        {
            // ==========================================
            // VERIFICAR SESIÓN
            // ==========================================

            var currentUserId =
                HttpContext.Session.GetString("UserId");

            var currentRole =
                HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }


            // ==========================================
            // VERIFICAR SUPERUSUARIO
            // ==========================================

            if (currentRole != "SUPERUSUARIO")
            {
                return RedirectToAction("Dashboard", "Home");
            }


            // ==========================================
            // VALIDAR ID
            // ==========================================

            if (userId == Guid.Empty)
            {
                TempData["Error"] =
                    "El usuario seleccionado no es válido.";

                return RedirectToAction("Index");
            }


            // ==========================================
            // VALIDAR ROL
            // ==========================================

            if (role != "EMPLEADO" &&
                role != "SOPORTE")
            {
                TempData["Error"] =
                    "El rol seleccionado no es válido.";

                return RedirectToAction("Index");
            }


            // ==========================================
            // EVITAR MODIFICARSE A SÍ MISMO
            // ==========================================

            if (Guid.TryParse(currentUserId, out Guid currentGuid))
            {
                if (currentGuid == userId)
                {
                    TempData["Error"] =
                        "No puedes modificar tu propio rol.";

                    return RedirectToAction("Index");
                }
            }


            // ==========================================
            // ACTUALIZAR ROL
            // ==========================================

            try
            {
                var actualizado =
                    await _userService.CambiarRol(
                        userId,
                        role
                    );

                if (!actualizado)
                {
                    TempData["Error"] =
                        "No se pudo actualizar el rol del usuario.";

                    return RedirectToAction("Index");
                }

                TempData["Success"] =
                    "El rol del usuario se actualizó correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Ocurrió un error al actualizar el rol.";
            }

            return RedirectToAction("Index");
        }
    }
}