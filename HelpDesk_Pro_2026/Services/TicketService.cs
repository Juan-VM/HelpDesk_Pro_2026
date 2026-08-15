using HelpDesk_Pro_2026.Models;
using Supabase;

namespace HelpDesk_Pro_2026.Services
{
    public class TicketService : ITicketService
    {
        private readonly Client _supabase;

        public TicketService(Client supabase)
        {
            _supabase = supabase;
        }

        #region Catalogs

        public async Task<List<SystemCatalog>> GetSystemsAsync()
        {
            var response = await _supabase.From<SystemCatalog>().Get();
            return response.Models.OrderBy(s => s.Name).ToList();
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            var response = await _supabase.From<Category>().Get();
            return response.Models.OrderBy(c => c.Name).ToList();
        }

        public async Task<List<Priority>> GetPrioritiesAsync()
        {
            var response = await _supabase.From<Priority>().Get();
            return response.Models;
        }

        public async Task<List<RiskLevel>> GetRiskLevelsAsync()
        {
            var response = await _supabase.From<RiskLevel>().Get();
            return response.Models;
        }

        public async Task<List<TicketState>> GetTicketStatesAsync()
        {
            var response = await _supabase.From<TicketState>().Get();
            return response.Models;
        }

        public async Task<List<Usuario>> GetUsersAsync()
        {
            var response = await _supabase.From<Usuario>().Get();
            return response.Models.OrderBy(u => u.FullName).ToList();
        }

        #endregion

        #region Ticket Operations

        public async Task<List<Ticket>> GetTicketsAsync(
            Guid userId,
            string userRole,
            Guid? searchUserId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? stateId = null,
            int? systemId = null)
        {
            List<Ticket> tickets;
            var normalizedRole = userRole?.Trim().ToUpperInvariant() ?? "EMPLEADO";

            if (normalizedRole == "EMPLEADO")
            {
                var response = await _supabase.From<Ticket>()
                    .Where(t => t.RequesterId == userId)
                    .Get();
                tickets = response.Models;
            }
            else
            {
                var allResponse = await _supabase.From<Ticket>().Get();
                tickets = allResponse.Models;
            }

            if (tickets.Any())
            {
                var attachmentsResponse = await _supabase.From<TicketAttachment>().Get();
                var allAttachments = attachmentsResponse.Models;

                var usersResponse = await _supabase.From<Usuario>().Get();
                var allUsers = usersResponse.Models;

                var systemsResponse = await _supabase.From<SystemCatalog>().Get();
                var allSystems = systemsResponse.Models;

                foreach (var ticket in tickets)
                {
                    ticket.Attachments = allAttachments
                        .Where(a => a.TicketId == ticket.TicketId)
                        .ToList();

                    ticket.Requester = allUsers.FirstOrDefault(u => u.UserId == ticket.RequesterId);
                    ticket.AssignedTech = allUsers.FirstOrDefault(u => u.UserId == ticket.AssignedTo);
                    ticket.System = allSystems.FirstOrDefault(s => s.SystemId == ticket.SystemId);
                }
            }

            // --- FILTERING LOGIC ---
            if (searchUserId.HasValue && searchUserId.Value != Guid.Empty)
            {
                tickets = tickets.Where(t => t.RequesterId == searchUserId.Value || t.AssignedTo == searchUserId.Value).ToList();
            }

            if (startDate.HasValue)
            {
                tickets = tickets.Where(t => t.CreatedAt.Date >= startDate.Value.Date).ToList();
            }

            if (endDate.HasValue)
            {
                tickets = tickets.Where(t => t.CreatedAt.Date <= endDate.Value.Date).ToList();
            }

            if (stateId.HasValue && stateId.Value > 0)
            {
                tickets = tickets.Where(t => t.StateId == stateId.Value).ToList();
            }

            if (systemId.HasValue && systemId.Value > 0)
            {
                tickets = tickets.Where(t => t.SystemId == systemId.Value).ToList();
            }

            return tickets.OrderByDescending(t => t.CreatedAt).ToList();
        }

        public async Task<Ticket?> GetTicketByIdAsync(long ticketId)
        {
            var response = await _supabase
                .From<Ticket>()
                .Where(t => t.TicketId == ticketId)
                .Get();

            var ticket = response.Models.FirstOrDefault();

            if (ticket != null)
            {
                var attachmentsResponse = await _supabase.From<TicketAttachment>()
                    .Where(a => a.TicketId == ticketId)
                    .Get();

                ticket.Attachments = attachmentsResponse.Models;

                var usersResponse = await _supabase.From<Usuario>().Get();
                ticket.Requester = usersResponse.Models.FirstOrDefault(u => u.UserId == ticket.RequesterId);
                ticket.AssignedTech = usersResponse.Models.FirstOrDefault(u => u.UserId == ticket.AssignedTo);

                var systemsResponse = await _supabase.From<SystemCatalog>().Get();
                ticket.System = systemsResponse.Models.FirstOrDefault(s => s.SystemId == ticket.SystemId);
            }

            return ticket;
        }

        public async Task<Ticket> CreateTicketAsync(CreateTicketViewModel model, Guid requesterId)
        {
            var ticketCode = await GenerateTicketCodeAsync();
            Guid assignedTechId = await GetAutoAssignedTechAsync();

            var newTicket = new Ticket
            {
                TicketCode = ticketCode,
                SystemId = model.SystemId,
                CategoryId = model.CategoryId,
                PriorityId = model.PriorityId,
                RiskId = model.RiskId,
                StateId = 1, // Default PENDIENTE
                Subject = model.Subject,
                Description = model.Description,
                Justification = model.Justification,
                RequesterId = requesterId,
                AssignedTo = assignedTechId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var response = await _supabase.From<Ticket>().Insert(newTicket);
            var createdTicket = response.Model;

            if (model.Attachments != null && model.Attachments.Any())
            {
                await UploadAttachmentsAsync(createdTicket.TicketId, model.Attachments);
            }

            return createdTicket;
        }

        private async Task<string> GenerateTicketCodeAsync()
        {
            var response = await _supabase.From<Ticket>().Get();
            int count = (response.Models?.Count ?? 0) + 1;
            return $"TCK-{count:D4}";
        }

        public async Task<bool> UpdateTicketAsync(EditTicketViewModel model, Guid userId, string userRole)
        {
            var response = await _supabase.From<Ticket>()
                .Where(t => t.TicketId == model.TicketId)
                .Get();

            var existingTicket = response.Models.FirstOrDefault();
            if (existingTicket == null) return false;

            var normalizedRole = userRole?.Trim().ToUpperInvariant() ?? "EMPLEADO";

            if (existingTicket.StateId != 1 && normalizedRole == "EMPLEADO") return false;

            await _supabase.From<Ticket>()
                .Where(t => t.TicketId == model.TicketId)
                .Set(t => t.SystemId, model.SystemId)
                .Set(t => t.CategoryId, model.CategoryId)
                .Set(t => t.PriorityId, model.PriorityId)
                .Set(t => t.RiskId, model.RiskId)
                .Set(t => t.Subject, model.Subject)
                .Set(t => t.Description, model.Description)
                .Set(t => t.Justification, model.Justification)
                .Set(t => t.UpdatedAt, DateTime.UtcNow)
                .Update();

            if (model.NewAttachments != null && model.NewAttachments.Any())
            {
                await UploadAttachmentsAsync(model.TicketId, model.NewAttachments);
            }

            return true;
        }

        public async Task<bool> UpdateTicketStateAsync(long ticketId, int newStateId, Guid userId, string userRole)
        {
            var normalizedRole = userRole?.Trim().ToUpperInvariant() ?? "EMPLEADO";

            if (normalizedRole != "SOPORTE" && normalizedRole != "SUPERUSUARIO") return false;

            var response = await _supabase.From<Ticket>()
                .Where(t => t.TicketId == ticketId)
                .Get();

            var ticket = response.Models.FirstOrDefault();
            if (ticket == null) return false;

            if (!IsValidStateTransition(ticket.StateId, newStateId))
            {
                return false;
            }

            await _supabase.From<Ticket>()
                .Where(t => t.TicketId == ticketId)
                .Set(t => t.StateId, newStateId)
                .Set(t => t.UpdatedAt, DateTime.UtcNow)
                .Update();

            return true;
        }

        private static bool IsValidStateTransition(int currentState, int newState)
        {
            return currentState switch
            {
                1 => newState == 2 || newState == 5,
                2 => newState == 3 || newState == 5,
                3 => newState == 4 || newState == 5,
                4 => false,
                5 => false,
                _ => false
            };
        }

        public async Task<bool> DeleteAttachmentAsync(long attachmentId)
        {
            var response = await _supabase.From<TicketAttachment>()
                .Where(a => a.AttachmentId == attachmentId)
                .Get();

            var attachment = response.Models.FirstOrDefault();
            if (attachment == null) return false;

            await _supabase.Storage
                .From("ticket-files")
                .Remove(new List<string> { attachment.FilePath });

            await _supabase.From<TicketAttachment>()
                .Where(a => a.AttachmentId == attachmentId)
                .Delete();

            return true;
        }

        private async Task UploadAttachmentsAsync(long ticketId, List<IFormFile> files)
        {
            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                var extension = Path.GetExtension(file.FileName);
                var fileNameInBucket = $"{Guid.NewGuid()}{extension}";

                await _supabase.Storage
                    .From("ticket-files")
                    .Upload(fileBytes, fileNameInBucket);

                var publicUrl = _supabase.Storage
                    .From("ticket-files")
                    .GetPublicUrl(fileNameInBucket);

                var attachmentRecord = new TicketAttachment
                {
                    TicketId = ticketId,
                    FileName = file.FileName,
                    FilePath = fileNameInBucket,
                    FileUrl = publicUrl,
                    CreatedAt = DateTime.UtcNow
                };

                await _supabase.From<TicketAttachment>().Insert(attachmentRecord);
            }
        }

        private async Task<Guid> GetAutoAssignedTechAsync()
        {
            var supportUsersResponse = await _supabase.From<Usuario>()
                .Where(u => u.Role == "SOPORTE")
                .Get();

            var supportUsers = supportUsersResponse.Models;

            if (!supportUsers.Any())
            {
                var superUserResponse = await _supabase.From<Usuario>()
                    .Where(u => u.Role == "SUPERUSUARIO")
                    .Get();

                return superUserResponse.Models.FirstOrDefault()?.UserId ?? Guid.Empty;
            }

            var ticketsResponse = await _supabase.From<Ticket>().Get();
            var activeTickets = ticketsResponse.Models
                .Where(t => t.StateId != 4 && t.StateId != 5)
                .ToList();

            var leastBusyTech = supportUsers
                .Select(tech => new
                {
                    TechId = tech.UserId,
                    ActiveCount = activeTickets.Count(t => t.AssignedTo == tech.UserId)
                })
                .OrderBy(x => x.ActiveCount)
                .FirstOrDefault();

            return leastBusyTech?.TechId ?? supportUsers.First().UserId;
        }

        #endregion

        #region Comment Operations

        public async Task<List<Comment>> GetCommentsByTicketIdAsync(long ticketId)
        {
            var response = await _supabase.From<Comment>()
                .Where(c => c.TicketId == ticketId)
                .Get();

            var comments = response.Models;

            if (comments.Any())
            {
                var usersResponse = await _supabase.From<Usuario>().Get();
                var allUsers = usersResponse.Models;

                foreach (var comment in comments)
                {
                    comment.User = allUsers.FirstOrDefault(u => u.UserId == comment.UserId);
                }
            }

            return comments.OrderBy(c => c.CreatedAt).ToList();
        }

        public async Task<Comment?> AddCommentAsync(long ticketId, Guid userId, string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText)) return null;

            var newComment = new Comment
            {
                TicketId = ticketId,
                UserId = userId,
                CommentText = commentText.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            var response = await _supabase.From<Comment>().Insert(newComment);
            var inserted = response.Model;

            if (inserted != null)
            {
                var userResponse = await _supabase.From<Usuario>()
                    .Where(u => u.UserId == userId)
                    .Get();
                inserted.User = userResponse.Models.FirstOrDefault();
            }

            return inserted;
        }

        public async Task<bool> UpdateCommentAsync(long commentId, Guid userId, string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText)) return false;

            var response = await _supabase.From<Comment>()
                .Where(c => c.CommentId == commentId)
                .Get();

            var comment = response.Models.FirstOrDefault();
            if (comment == null || comment.UserId != userId) return false;

            await _supabase.From<Comment>()
                .Where(c => c.CommentId == commentId)
                .Set(c => c.CommentText, commentText.Trim())
                .Update();

            return true;
        }

        public async Task<bool> DeleteCommentAsync(long commentId, Guid userId)
        {
            var response = await _supabase.From<Comment>()
                .Where(c => c.CommentId == commentId)
                .Get();

            var comment = response.Models.FirstOrDefault();
            if (comment == null || comment.UserId != userId) return false;

            await _supabase.From<Comment>()
                .Where(c => c.CommentId == commentId)
                .Delete();

            return true;
        }

        #endregion
    }
}