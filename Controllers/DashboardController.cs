using FamilyTaskManagement.Web.Models;
using FamilyTaskManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTaskManagement.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IApiService apiService,ILogger<DashboardController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login","Auth");
            }

            var viewModel = new DashboardViewModel( );

            try
            {
                // Get tasks
                var tasksResponse = await _apiService.GetTasksAsync( );
                if (tasksResponse.Success && tasksResponse.Data != null)
                {
                    viewModel.RecentTasks = tasksResponse.Data.Take(5).ToList( );
                    viewModel.TotalTasks = tasksResponse.Data.Count;
                    viewModel.CompletedTasks = tasksResponse.Data.Count(t => t.Status.ToLower( ) == "completed");
                    viewModel.PendingTasks = tasksResponse.Data.Count(t => t.Status.ToLower( ) == "pending");
                    viewModel.OverdueTasks = tasksResponse.Data.Count(t =>
                        t.DueDate.HasValue && t.DueDate.Value < DateTime.Now && t.Status.ToLower( ) != "completed");
                }

                // Get my assignments
                var assignmentsResponse = await _apiService.GetMyAssignmentsAsync( );
                if (assignmentsResponse.Success && assignmentsResponse.Data != null)
                {
                    viewModel.MyAssignments = assignmentsResponse.Data.Take(5).ToList( );
                }

                // Get notifications
                var notificationsResponse = await _apiService.GetNotificationsAsync( );
                if (notificationsResponse.Success && notificationsResponse.Data != null)
                {
                    viewModel.Notifications = notificationsResponse.Data.Take(5).ToList( );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error loading dashboard data");
                TempData["ErrorMessage"] = "Error loading dashboard data. Please try again.";
            }

            return View(viewModel);
        }
    }
}

