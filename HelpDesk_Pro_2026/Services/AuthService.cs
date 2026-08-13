using HelpDesk_Pro_2026.Models;

namespace HelpDesk_Pro_2026.Services
{
    public class AuthService
    {
        private readonly UserService _userService;

        public AuthService(UserService userService)
        {
            _userService = userService;
        }

        public async Task<Usuario?> Login(
            string email,
            string password)
        {
            var usuario = await _userService.ObtenerPorEmail(email);

            if (usuario == null)
            {
                return null;
            }

            if (usuario.Password != password)
            {
                return null;
            }

            return usuario;
        }
    }
}