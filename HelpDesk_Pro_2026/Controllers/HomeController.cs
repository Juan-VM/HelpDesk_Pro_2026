using System.Diagnostics;
using HelpDesk_Pro_2026.Data;
using HelpDesk_Pro_2026.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk_Pro_2026.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // INDEX (HANDLES HOME ROUTE SECURITY)
        // ==========================================
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // ==========================================
        // DASHBOARD (ON-DEMAND EF CORE VIEW)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // 1. Session Auth Check
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Role Authorization Guard
            if (userRole != "SUPERUSUARIO" && userRole != "SOPORTE")
            {
                return RedirectToAction("Index", "Tickets"); // Redirect standard users away
            }

            // 3. Fetch Tickets from Database
            var tickets = await _context.Tickets
                .Include(t => t.Priority)
                .ToListAsync();

            // 4. Perform Grouping in C# (In-Memory)
            var priorityStats = tickets
                .GroupBy(t => t.Priority != null ? t.Priority.Name : "Sin Prioridad")
                .Select(g => new
                {
                    PriorityName = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // 5. Send metrics to Dashboard View
            ViewBag.TotalTickets = tickets.Count;
            ViewBag.PriorityLabels = priorityStats.Select(p => p.PriorityName).ToList();
            ViewBag.PriorityData = priorityStats.Select(p => p.Count).ToList();

            return View();
        }
    }
}