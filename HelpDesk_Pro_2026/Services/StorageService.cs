using Supabase.Storage;

namespace HelpDesk_Pro_2026.Services
{
    public class StorageService
    {
        private readonly Supabase.Client _client;

        private const string BUCKET_NAME = "profile-photos";

        public StorageService(Supabase.Client client)
        {
            _client = client;
        }

        // ==========================================
        // SUBIR FOTO DE PERFIL
        // ==========================================

        public async Task<string?> UploadProfilePhoto(
            Guid userId,
            IFormFile photo,
            string accessToken,
            string refreshToken)
        {
            if (photo == null || photo.Length == 0)
                return null;

            // ==========================================
            // VALIDAR TOKENS
            // ==========================================

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new Exception(
                    "No existe el AccessToken de Supabase."
                );
            }

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new Exception(
                    "No existe el RefreshToken de Supabase."
                );
            }

            // ==========================================
            // VALIDAR EXTENSIÓN
            // ==========================================

            var extension = Path
                .GetExtension(photo.FileName)
                .ToLowerInvariant();

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            if (!allowedExtensions.Contains(extension))
            {
                throw new Exception(
                    "El formato de imagen no es válido. Usa JPG, PNG o WEBP."
                );
            }

            // ==========================================
            // VALIDAR TAMAÑO
            // ==========================================

            if (photo.Length > 5 * 1024 * 1024)
            {
                throw new Exception(
                    "La imagen no puede superar los 5 MB."
                );
            }

            // ==========================================
            // ESTABLECER SESIÓN DE SUPABASE AUTH
            // ==========================================

            await _client.Auth.SetSession(
                accessToken,
                refreshToken
            );

            // ==========================================
            // NOMBRE DEL ARCHIVO
            // ==========================================

            var fileName = $"{userId}{extension}";

            // ==========================================
            // LEER IMAGEN
            // ==========================================

            using var memoryStream = new MemoryStream();

            await photo.CopyToAsync(memoryStream);

            var fileBytes = memoryStream.ToArray();

            // ==========================================
            // SUBIR A STORAGE
            // ==========================================

            await _client.Storage
                .From(BUCKET_NAME)
                .Upload(
                    fileBytes,
                    fileName,
                    new Supabase.Storage.FileOptions
                    {
                        Upsert = true
                    }
                );

            // ==========================================
            // OBTENER URL PÚBLICA
            // ==========================================

            var publicUrl = _client.Storage
                .From(BUCKET_NAME)
                .GetPublicUrl(fileName);

            return publicUrl;
        }
    }
}