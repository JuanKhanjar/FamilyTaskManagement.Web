using FamilyTaskManagement.Web.Models;
using FamilyTaskManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTaskManagement.Web.Controllers
{
    public class TasksController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(IApiService apiService, ILogger<TasksController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var response = await _apiService.GetTasksAsync();
                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = response.Message;
                    return View(new List<TaskDto>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tasks");
                TempData["ErrorMessage"] = "Error loading tasks. Please try again.";
                return View(new List<TaskDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var response = await _apiService.GetTaskByIdAsync(id);
                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = response.Message;
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading task details");
                TempData["ErrorMessage"] = "Error loading task details. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Load users for assignment dropdown
            await LoadUsersForViewBag();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTaskDto createTaskDto)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                await LoadUsersForViewBag();
                return View(createTaskDto);
            }

            try
            {
                var response = await _apiService.CreateTaskAsync(createTaskDto);
                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Task created successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", response.Message);
                    if (response.Errors.Any())
                    {
                        foreach (var error in response.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                ModelState.AddModelError("", "An error occurred while creating the task. Please try again.");
            }

            await LoadUsersForViewBag();
            return View(createTaskDto);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var updateStatusDto = new UpdateTaskStatusDto { Status = status };
                var response = await _apiService.UpdateTaskStatusAsync(id, updateStatusDto);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Task status updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = response.Message;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task status");
                TempData["ErrorMessage"] = "Error updating task status. Please try again.";
            }

            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Assign(int taskId)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login", "Auth");
            }

            await LoadUsersForViewBag();
            ViewBag.TaskId = taskId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Assign(CreateTaskAssignmentDto createTaskAssignmentDto)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                await LoadUsersForViewBag();
                ViewBag.TaskId = createTaskAssignmentDto.TaskId;
                return View(createTaskAssignmentDto);
            }

            try
            {
                var response = await _apiService.CreateTaskAssignmentAsync(createTaskAssignmentDto);
                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Task assigned successfully!";
                    return RedirectToAction("Details", new { id = createTaskAssignmentDto.TaskId });
                }
                else
                {
                    ModelState.AddModelError("", response.Message);
                    if (response.Errors.Any())
                    {
                        foreach (var error in response.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning task");
                ModelState.AddModelError("", "An error occurred while assigning the task. Please try again.");
            }

            await LoadUsersForViewBag();
            ViewBag.TaskId = createTaskAssignmentDto.TaskId;
            return View(createTaskAssignmentDto);
        }

        private async Task LoadUsersForViewBag()
        {
            try
            {
                var usersResponse = await _apiService.GetUsersAsync();
                if (usersResponse.Success && usersResponse.Data != null)
                {
                    ViewBag.Users = usersResponse.Data;
                }
                else
                {
                    ViewBag.Users = new List<UserDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                ViewBag.Users = new List<UserDto>();
            }
        }
    }
}

