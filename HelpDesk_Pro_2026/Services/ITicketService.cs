using HelpDesk_Pro_2026.Models;

namespace HelpDesk_Pro_2026.Services
{
    public interface ITicketService
    {
        Task<List<SystemCatalog>> GetSystemsAsync();
        Task<List<Category>> GetCategoriesAsync();
        Task<List<Priority>> GetPrioritiesAsync();
        Task<List<RiskLevel>> GetRiskLevelsAsync();
        Task<List<TicketState>> GetTicketStatesAsync();
        Task<List<Usuario>> GetUsersAsync();

        Task<List<Ticket>> GetTicketsAsync(
            Guid userId,
            string userRole,
            Guid? searchUserId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? stateId = null,
            int? systemId = null);

        Task<Ticket?> GetTicketByIdAsync(long ticketId);
        Task<Ticket> CreateTicketAsync(CreateTicketViewModel model, Guid requesterId);
        Task<bool> UpdateTicketAsync(EditTicketViewModel model, Guid userId, string userRole);
        Task<bool> UpdateTicketStateAsync(long ticketId, int newStateId, Guid userId, string userRole);
        Task<bool> DeleteAttachmentAsync(long attachmentId);

        Task<List<Comment>> GetCommentsByTicketIdAsync(long ticketId);
        Task<Comment?> AddCommentAsync(long ticketId, Guid userId, string commentText);
        Task<bool> UpdateCommentAsync(long commentId, Guid userId, string commentText);
        Task<bool> DeleteCommentAsync(long commentId, Guid userId);
    }
}