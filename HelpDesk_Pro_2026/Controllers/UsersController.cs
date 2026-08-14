using HelpDesk_Pro_2026.Models;
using HelpDesk_Pro_2026.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk_Pro_2026.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserService _userService;
        private readonly StorageService _storageService;
        private readonly AuthService _authService;
        private readonly UserAdministrationService _userAdministrationService;

        public UsersController(
            UserService userService,
            StorageService storageService,
            AuthService authService,
            UserAdministrationService userAdministrationService)
        {
            _userService = userService;
            _storageService = storageService;
            _authService = authService;
            _userAdministrationService = userAdministrationService;
        }

        // ==========================================
        // MOSTRAR PERFIL
        // GET: /Users/Profile
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var accessToken = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(userId) ||
                string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                HttpContext.Session.Clear();

                return RedirectToAction("Login", "Account");
            }

            var usuario = await _userService.ObtenerPorId(userGuid);

            if (usuario == null)
            {
                HttpContext.Session.Clear();

                return RedirectToAction("Login", "Account");
            }

            var model = new EditProfileViewModel
            {
                FullName = usuario.FullName,
                Email = usuario.Email,
                Role = usuario.Role,
                PhotoUrl = usuario.PhotoUrl
            };

            return View(model);
        }


        // ==========================================
        // GUARDAR CAMBIOS DEL PERFIL
        // POST: /Users/Profile
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(
            EditProfileViewModel model)
        {
            // ==========================================
            // VERIFICAR SESIÓN
            // ==========================================

            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                HttpContext.Session.Clear();

                return RedirectToAction("Login", "Account");
            }


            // ==========================================
            // BUSCAR USUARIO ACTUAL
            // ==========================================

            var usuario = await _userService.ObtenerPorId(userGuid);

            if (usuario == null)
            {
                HttpContext.Session.Clear();

                return RedirectToAction("Login", "Account");
            }


            // ==========================================
            // VALIDAR NOMBRE
            // ==========================================

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                ModelState.AddModelError(
                    "FullName",
                    "El nombre completo es obligatorio."
                );
            }


            // ==========================================
            // VALIDAR CONTRASEÑA
            // ==========================================

            // Si escribe contraseña actual pero no nueva
            if (!string.IsNullOrWhiteSpace(model.CurrentPassword) &&
                string.IsNullOrWhiteSpace(model.NewPassword))
            {
                ModelState.AddModelError(
                    "NewPassword",
                    "Debes ingresar una nueva contraseña."
                );
            }

            // Si escribe nueva contraseña
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                // Debe escribir la actual
                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                {
                    ModelState.AddModelError(
                        "CurrentPassword",
                        "Debes ingresar tu contraseña actual."
                    );
                }

                // Debe confirmar
                if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
                {
                    ModelState.AddModelError(
                        "ConfirmPassword",
                        "Debes confirmar la nueva contraseña."
                    );
                }

                // Comparar contraseñas
                if (!string.IsNullOrWhiteSpace(model.ConfirmPassword) &&
                    model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError(
                        "ConfirmPassword",
                        "Las contraseñas no coinciden."
                    );
                }

                // Longitud mínima
                if (model.NewPassword.Length < 6)
                {
                    ModelState.AddModelError(
                        "NewPassword",
                        "La contraseña debe tener al menos 6 caracteres."
                    );
                }
            }


            // ==========================================
            // SI HAY ERRORES DE VALIDACIÓN
            // ==========================================

            if (!ModelState.IsValid)
            {
                model.Email = usuario.Email;
                model.Role = usuario.Role;
                model.PhotoUrl = usuario.PhotoUrl;

                return View(model);
            }


            // ==========================================
            // ACTUALIZAR NOMBRE
            // ==========================================

            var usuarioActualizado =
                await _userService.ActualizarPerfil(
                    userGuid,
                    model.FullName,
                    null
                );

            if (usuarioActualizado == null)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo actualizar el perfil."
                );

                model.Email = usuario.Email;
                model.Role = usuario.Role;
                model.PhotoUrl = usuario.PhotoUrl;

                return View(model);
            }


            // ==========================================
            // SUBIR FOTO
            // ==========================================

            if (model.Photo != null &&
                model.Photo.Length > 0)
            {
                try
                {
                    var accessToken =
                        HttpContext.Session.GetString("AccessToken");

                    var refreshToken =
                        HttpContext.Session.GetString("RefreshToken");

                    if (string.IsNullOrWhiteSpace(accessToken) ||
                        string.IsNullOrWhiteSpace(refreshToken))
                    {
                        ModelState.AddModelError(
                            "Photo",
                            "La sesión de Supabase ha expirado. Cierra sesión e inicia sesión nuevamente."
                        );

                        model.Email = usuarioActualizado.Email;
                        model.Role = usuarioActualizado.Role;
                        model.PhotoUrl = usuarioActualizado.PhotoUrl;

                        return View(model);
                    }

                    var photoUrl =
                        await _storageService.UploadProfilePhoto(
                            userGuid,
                            model.Photo,
                            accessToken,
                            refreshToken
                        );

                    if (!string.IsNullOrWhiteSpace(photoUrl))
                    {
                        var usuarioConFoto =
                            await _userService.ActualizarFoto(
                                userGuid,
                                photoUrl
                            );

                        if (usuarioConFoto != null)
                        {
                            usuarioActualizado =
                                usuarioConFoto;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "Photo",
                        "No se pudo subir la imagen: " + ex.Message
                    );

                    model.Email = usuarioActualizado.Email;
                    model.Role = usuarioActualizado.Role;
                    model.PhotoUrl = usuarioActualizado.PhotoUrl;

                    return View(model);
                }
            }


            // ==========================================
            // CAMBIAR CONTRASEÑA
            // ==========================================

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var passwordChanged =
                    await _authService.CambiarPasswordAsync(
                        usuarioActualizado.Email,
                        model.CurrentPassword!,
                        model.NewPassword
                    );

                if (!passwordChanged)
                {
                    ModelState.AddModelError(
                        "CurrentPassword",
                        "La contraseña actual es incorrecta."
                    );

                    model.Email = usuarioActualizado.Email;
                    model.Role = usuarioActualizado.Role;
                    model.PhotoUrl = usuarioActualizado.PhotoUrl;

                    return View(model);
                }
            }


            // ==========================================
            // ACTUALIZAR SESSION
            // ==========================================

            HttpContext.Session.SetString(
                "FullName",
                usuarioActualizado.FullName
            );

            HttpContext.Session.SetString(
                "Email",
                usuarioActualizado.Email
            );

            HttpContext.Session.SetString(
                "Role",
                usuarioActualizado.Role
            );

            HttpContext.Session.SetString(
                "PhotoUrl",
                usuarioActualizado.PhotoUrl ?? ""
            );


            // ==========================================
            // MENSAJE DE ÉXITO
            // ==========================================

            TempData["Success"] =
                "Tu perfil se actualizó correctamente.";


            // ==========================================
            // REDIRECCIÓN
            // ==========================================

            return RedirectToAction("Profile");
        }





        // ==========================================
        // ADMINISTRAR USUARIOS
        // GET: /Users/Index
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

            if (!string.Equals(
                role,
                "SUPERUSUARIO",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "Home");
            }


            // ==========================================
            // OBTENER USUARIOS
            // ==========================================

            try
            {
                var usuarios =
                    await _userAdministrationService.ObtenerTodosAsync();

                var model = new AdministrarUsuariosViewModel
                {
                    Usuarios = usuarios
                };

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "No se pudieron cargar los usuarios.";

                return View(
                    new AdministrarUsuariosViewModel()
                );
            }
        }



        // ==========================================
        // CAMBIAR ROL DE USUARIO
        // POST: /Users/CambiarRol
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarRol(
            Guid userId,
            string role)
        {
            // ==========================================
            // VERIFICAR SESIÓN
            // ==========================================

            var sessionUserId =
                HttpContext.Session.GetString("UserId");

            var sessionRole =
                HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(sessionUserId))
            {
                return RedirectToAction("Login", "Account");
            }


            // ==========================================
            // VERIFICAR SUPERUSUARIO
            // ==========================================

            if (!string.Equals(
                sessionRole,
                "SUPERUSUARIO",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "Home");
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
            // CAMBIAR ROL
            // ==========================================

            try
            {
                await _userAdministrationService
                    .CambiarRolAsync(userId, role);

                TempData["Success"] =
                    "El rol del usuario se actualizó correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "No se pudo actualizar el rol del usuario.";
            }

            return RedirectToAction("Index");
        }

    }
}