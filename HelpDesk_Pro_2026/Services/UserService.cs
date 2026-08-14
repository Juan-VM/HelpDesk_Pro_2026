using HelpDesk_Pro_2026.Models;

namespace HelpDesk_Pro_2026.Services
{
    public class UserService
    {
        private readonly Supabase.Client _client;

        public UserService(Supabase.Client client)
        {
            _client = client;
        }

        // ==========================================
        // OBTENER USUARIO POR ID
        // ==========================================

        public async Task<Usuario?> ObtenerPorId(Guid userId)
        {
            var response = await _client
                .From<Usuario>()
                .Where(x => x.UserId == userId)
                .Get();

            return response.Models.FirstOrDefault();
        }


        // ==========================================
        // OBTENER USUARIO POR EMAIL
        // ==========================================

        public async Task<Usuario?> ObtenerPorEmail(string email)
        {
            var response = await _client
                .From<Usuario>()
                .Where(x => x.Email == email)
                .Get();

            return response.Models.FirstOrDefault();
        }


        // ==========================================
        // OBTENER TODOS LOS USUARIOS
        // ==========================================

        public async Task<List<Usuario>> ObtenerUsuarios()
        {
            var response = await _client
                .From<Usuario>()
                .Get();

            return response.Models;
        }


        // ==========================================
        // CREAR PERFIL
        // ==========================================

        public async Task<Usuario?> CrearPerfil(
            Guid userId,
            string fullName,
            string email,
            string role,
            string photoUrl)
        {
            var usuario = new Usuario
            {
                UserId = userId,
                FullName = fullName,
                Email = email,
                Role = role,
                PhotoUrl = photoUrl
            };

            var response = await _client
                .From<Usuario>()
                .Insert(usuario);

            return response.Models.FirstOrDefault();
        }


        // ==========================================
        // ACTUALIZAR PERFIL
        // ==========================================

        public async Task<Usuario?> ActualizarPerfil(
            Guid userId,
            string fullName,
            string? photoUrl)
        {
            var usuario = await ObtenerPorId(userId);

            if (usuario == null)
                return null;

            usuario.FullName = fullName;

            if (!string.IsNullOrEmpty(photoUrl))
            {
                usuario.PhotoUrl = photoUrl;
            }

            await _client
                .From<Usuario>()
                .Update(usuario);

            return usuario;
        }


        // ==========================================
        // ACTUALIZAR FOTO
        // ==========================================

        public async Task<Usuario?> ActualizarFoto(
            Guid userId,
            string photoUrl)
        {
            var usuario = await ObtenerPorId(userId);

            if (usuario == null)
                return null;

            usuario.PhotoUrl = photoUrl;

            await _client
                .From<Usuario>()
                .Update(usuario);

            return usuario;
        }


        // ==========================================
        // CAMBIAR ROL
        // ==========================================

        public async Task<bool> CambiarRol(
            Guid userId,
            string nuevoRol)
        {
            // Validar que solamente existan estos roles
            if (nuevoRol != "EMPLEADO" &&
                nuevoRol != "SOPORTE")
            {
                return false;
            }

            var usuario = await ObtenerPorId(userId);

            if (usuario == null)
                return false;

            usuario.Role = nuevoRol;

            await _client
                .From<Usuario>()
                .Update(usuario);

            return true;
        }
    }
}