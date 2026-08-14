using Supabase.Gotrue;

namespace HelpDesk_Pro_2026.Services
{
    public class AuthService
    {
        private readonly Supabase.Client _client;

        public AuthService(Supabase.Client client)
        {
            _client = client;
        }

        // ==========================================
        // REGISTRO
        // ==========================================

        public async Task<Session?> RegisterAsync(
            string email,
            string password,
            string fullName)
        {
            var options = new Supabase.Gotrue.SignUpOptions
            {
                Data = new Dictionary<string, object>
                {
                    { "full_name", fullName }
                }
            };

            return await _client.Auth.SignUp(
                email,
                password,
                options
            );
        }

        // ==========================================
        // LOGIN
        // ==========================================

        public async Task<Session?> LoginAsync(
            string email,
            string password)
        {
            var session = await _client.Auth.SignIn(
                email,
                password
            );

            return session;
        }

        // ==========================================
        // CAMBIAR CONTRASEÑA
        // ==========================================

        public async Task<bool> CambiarPasswordAsync(
    string email,
    string currentPassword,
    string newPassword)
        {
            try
            {
                // Verificar que la contraseña actual sea correcta
                await _client.Auth.SignIn(
                    email,
                    currentPassword
                );

                // Cambiar contraseña
                await _client.Auth.Update(
                    new Supabase.Gotrue.UserAttributes
                    {
                        Password = newPassword
                    }
                );

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ==========================================
        // LOGOUT
        // ==========================================

        public async Task LogoutAsync()
        {
            await _client.Auth.SignOut();
        }
    }
}