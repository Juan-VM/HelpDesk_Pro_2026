using Supabase;

namespace HelpDesk_Pro_2026.Services
{
    public static class SupabClient
    {
        private static string url = "https://dgrlsgmyefeijwczbyfi.supabase.co";

        private static string key = "sb_publishable_fpa4WoG6wyCD8d4LFoTy2w_lpFjud65";

        private static Client? _client;

        public static async Task<Client> GetClient()
        {
            if (_client == null)
            {
                _client = new Client(url, key);

                await _client.InitializeAsync();
            }

            return _client;
        }
    }
}
