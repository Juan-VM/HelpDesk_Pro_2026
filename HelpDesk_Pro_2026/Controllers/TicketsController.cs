using HelpDesk_Pro_2026.Models;
using HelpDesk_Pro_2026.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace HelpDesk_Pro_2026.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        private (Guid userId, string role) GetCurrentUserContext()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub")
                         ?? HttpContext.Session.GetString("UserId");

            // Checks both "Role" and "UserRole" session keys to prevent mismatch
            var role = User.FindFirstValue(ClaimTypes.Role)
                    ?? User.FindFirstValue("role")
                    ?? HttpContext.Session.GetString("Role")
                    ?? HttpContext.Session.GetString("UserRole")
                    ?? "EMPLEADO";

            Guid.TryParse(userIdStr, out var userId);
            return (userId, role.Trim().ToUpperInvariant());
        }

        // GET: /Tickets
        public async Task<IActionResult> Index(
            Guid? searchUserId,
            DateTime? startDate,
            DateTime? endDate,
            int? stateId,
            int? systemId)
        {
            var (userId, role) = GetCurrentUserContext();
            var tickets = await _ticketService.GetTicketsAsync(userId, role, searchUserId, startDate, endDate, stateId, systemId);

            var users = await _ticketService.GetUsersAsync();
            var states = await _ticketService.GetTicketStatesAsync();
            var systems = await _ticketService.GetSystemsAsync();

            ViewBag.Users = new SelectList(users, "UserId", "FullName", searchUserId);
            ViewBag.States = new SelectList(states, "StateId", "Name", stateId);
            ViewBag.Systems = new SelectList(systems, "SystemId", "Name", systemId);

            ViewBag.SearchUserId = searchUserId;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.StateId = stateId;
            ViewBag.SystemId = systemId;

            ViewBag.UserRole = role;
            return View(tickets);
        }

        // GET: /Tickets/Create
        public async Task<IActionResult> Create()
        {
            var (_, role) = GetCurrentUserContext();
            if (role == "SOPORTE")
            {
                TempData["ErrorMessage"] = "Los usuarios de soporte técnico no pueden crear tickets.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync();
            return View();
        }

        // POST: /Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTicketViewModel model)
        {
            var (userId, role) = GetCurrentUserContext();

            if (role == "SOPORTE")
            {
                TempData["ErrorMessage"] = "Los usuarios de soporte técnico no pueden crear tickets.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(model);
            }

            try
            {
                var createdTicket = await _ticketService.CreateTicketAsync(model, userId);
                TempData["SuccessMessage"] = $"Ticket {createdTicket.TicketCode} creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al crear el ticket: {ex.Message}");
                await PopulateDropdownsAsync();
                return View(model);
            }
        }

        // GET: /Tickets/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null) return NotFound();

            var (userId, role) = GetCurrentUserContext();

            var editModel = new EditTicketViewModel
            {
                TicketId = ticket.TicketId,
                TicketCode = ticket.TicketCode,
                SystemId = ticket.SystemId,
                CategoryId = ticket.CategoryId,
                PriorityId = ticket.PriorityId,
                RiskId = ticket.RiskId,
                Subject = ticket.Subject,
                Description = ticket.Description,
                Justification = ticket.Justification,
                ExistingAttachments = ticket.Attachments ?? new List<TicketAttachment>()
            };

            var comments = await _ticketService.GetCommentsByTicketIdAsync(id);

            ViewBag.UserRole = role;
            ViewBag.CurrentUserId = userId;
            ViewBag.Ticket = ticket;
            ViewBag.Comments = comments;

            await PopulateDropdownsAsync();
            return View(editModel);
        }

        // POST: /Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditTicketViewModel model)
        {
            var (userId, role) = GetCurrentUserContext();

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(model);
            }

            try
            {
                var success = await _ticketService.UpdateTicketAsync(model, userId, role);

                if (!success)
                {
                    TempData["ErrorMessage"] = "No se pudo actualizar el ticket. Es posible que el estado ya no permita modificaciones.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["SuccessMessage"] = "Ticket actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al actualizar: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Tickets/UpdateState
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateState(long ticketId, int newStateId)
        {
            var (userId, role) = GetCurrentUserContext();

            var success = await _ticketService.UpdateTicketStateAsync(ticketId, newStateId, userId, role);

            if (success)
            {
                TempData["SuccessMessage"] = "Estado del ticket actualizado exitosamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo realizar la transición de estado. Verifique las reglas del flujo de trabajo.";
            }

            return RedirectToAction(nameof(Edit), new { id = ticketId });
        }

        // POST: /Tickets/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(long ticketId, string commentText)
        {
            var (userId, _) = GetCurrentUserContext();

            if (!string.IsNullOrWhiteSpace(commentText))
            {
                await _ticketService.AddCommentAsync(ticketId, userId, commentText);
                TempData["SuccessMessage"] = "Comentario agregado.";
            }

            return RedirectToAction(nameof(Edit), new { id = ticketId });
        }

        // POST: /Tickets/UpdateComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateComment(long commentId, long ticketId, string commentText)
        {
            var (userId, _) = GetCurrentUserContext();

            var success = await _ticketService.UpdateCommentAsync(commentId, userId, commentText);
            if (success)
            {
                TempData["SuccessMessage"] = "Comentario actualizado.";
            }
            else
            {
                TempData["ErrorMessage"] = "No tiene permisos para editar este comentario.";
            }

            return RedirectToAction(nameof(Edit), new { id = ticketId });
        }

        // POST: /Tickets/DeleteComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(long commentId, long ticketId)
        {
            var (userId, _) = GetCurrentUserContext();

            var success = await _ticketService.DeleteCommentAsync(commentId, userId);
            if (success)
            {
                TempData["SuccessMessage"] = "Comentario eliminado.";
            }
            else
            {
                TempData["ErrorMessage"] = "No tiene permisos para eliminar este comentario.";
            }

            return RedirectToAction(nameof(Edit), new { id = ticketId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttachment(long attachmentId)
        {
            var success = await _ticketService.DeleteAttachmentAsync(attachmentId);
            if (success)
            {
                return Json(new { success = true, message = "Archivo eliminado del almacenamiento y registro." });
            }
            return Json(new { success = false, message = "No se pudo eliminar el archivo." });
        }

        private async Task PopulateDropdownsAsync()
        {
            var systems = await _ticketService.GetSystemsAsync();
            var categories = await _ticketService.GetCategoriesAsync();
            var priorities = await _ticketService.GetPrioritiesAsync();
            var riskLevels = await _ticketService.GetRiskLevelsAsync();
            var ticketStates = await _ticketService.GetTicketStatesAsync();

            ViewBag.Systems = new SelectList(systems, "SystemId", "Name");
            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
            ViewBag.Priorities = new SelectList(priorities, "PriorityId", "Name");
            ViewBag.RiskLevels = new SelectList(riskLevels, "RiskId", "Name");
            ViewBag.TicketStates = new SelectList(ticketStates, "StateId", "Name");
        }
    }
}