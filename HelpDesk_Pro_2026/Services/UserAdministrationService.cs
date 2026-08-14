using HelpDesk_Pro_2026.Models;

namespace HelpDesk_Pro_2026.Services
{
    public class UserAdministrationService
    {
        private readonly Supabase.Client _client;

        public UserAdministrationService(Supabase.Client client)
        {
            _client = client;
        }

        // ==========================================================
        // OBTENER TODOS LOS USUARIOS
        // ==========================================================

        public async Task<List<Usuario>> ObtenerTodosAsync()
        {
            var response = await _client
                .From<Usuario>()
                .Get();

            return response.Models.ToList();
        }

        // ==========================================================
        // CAMBIAR ROL
        // ==========================================================

        public async Task CambiarRolAsync(
            Guid userId,
            string nuevoRol)
        {
            if (nuevoRol != "EMPLEADO" &&
                nuevoRol != "SOPORTE")
            {
                throw new ArgumentException(
                    "El rol seleccionado no es válido."
                );
            }

            var parameters = new Dictionary<string, object>
            {
                { "target_user_id", userId },
                { "new_role", nuevoRol }
            };

            await _client.Rpc(
                "admin_change_user_role",
                parameters
            );
        }
    }
}