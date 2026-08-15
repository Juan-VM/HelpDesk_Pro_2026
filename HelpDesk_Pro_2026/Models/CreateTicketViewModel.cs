using System.ComponentModel.DataAnnotations;

namespace HelpDesk_Pro_2026.Models
{
    public class CreateTicketViewModel
    {
        [Required(ErrorMessage = "El sistema es obligatorio.")]
        public int SystemId { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "La prioridad es obligatoria.")]
        public int PriorityId { get; set; }

        [Required(ErrorMessage = "El nivel de riesgo es obligatorio.")]
        public int RiskId { get; set; }

        [Required(ErrorMessage = "El asunto es obligatorio.")]
        [StringLength(200, ErrorMessage = "El asunto no puede superar los 200 caracteres.")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Description { get; set; } = string.Empty;

        public string? Justification { get; set; }

        public List<IFormFile>? Attachments { get; set; }
    }
}