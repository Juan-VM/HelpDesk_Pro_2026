using HelpDesk_Pro_2026.Models;

namespace HelpDesk_Pro_2026.Services
{
    public class UserService
    {
        public async Task<Usuario?> ObtenerPorEmail(string email)
        {
            var client = await SupabClient.GetClient();

            // Normalizar el correo antes de enviarlo a Supabase
            string emailNormalizado = email.Trim().ToLower();

            var response = await client
                .From<Usuario>()
                .Where(x => x.Email == emailNormalizado)
                .Get();

            return response.Models.FirstOrDefault();
        }

        public async Task<bool> RegistrarUsuario(
            string nombre,
            string email,
            string password)
        {
            // Normalizar los datos
            string emailNormalizado = email.Trim().ToLower();
            string nombreNormalizado = nombre.Trim();

            // Verificar si ya existe
            var usuarioExistente = await ObtenerPorEmail(emailNormalizado);

            if (usuarioExistente != null)
            {
                return false;
            }

            var client = await SupabClient.GetClient();

            var usuario = new Usuario
            {
                FullName = nombreNormalizado,
                Email = emailNormalizado,
                Password = password,

                // El registro público siempre crea EMPLEADOS
                Role = "EMPLEADO",

                PhotoUrl = null,
                CreatedAt = DateTime.Now
            };

            await client
                .From<Usuario>()
                .Insert(usuario);

            return true;
        }

        public async Task<Usuario?> ObtenerPorId(Guid userId)
        {
            var client = await SupabClient.GetClient();

            var response = await client
                .From<Usuario>()
                .Where(x => x.UserId == userId)
                .Get();

            return response.Models.FirstOrDefault();
        }

        public async Task<bool> ActualizarPerfil(
            Guid userId,
            string fullName,
            string email)
        {
            var client = await SupabClient.GetClient();

            var usuario = await ObtenerPorId(userId);

            if (usuario == null)
            {
                return false;
            }

            usuario.FullName = fullName.Trim();
            usuario.Email = email.Trim().ToLower();

            await client
                .From<Usuario>()
                .Update(usuario);

            return true;
        }
    }
}