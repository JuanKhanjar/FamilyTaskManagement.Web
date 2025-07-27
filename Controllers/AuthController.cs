using FamilyTaskManagement.Web.Models;
using FamilyTaskManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTaskManagement.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IApiService apiService,ILogger<AuthController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If user is already logged in, redirect to dashboard
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Index","Dashboard");
            }

            return View( );
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }

            try
            {
                var response = await _apiService.LoginAsync(loginDto);

                if (response.Success && response.Data != null)
                {
                    // Store tokens in session
                    HttpContext.Session.SetString("AccessToken",response.Data.AccessToken);
                    HttpContext.Session.SetString("RefreshToken",response.Data.RefreshToken);
                    HttpContext.Session.SetString("UserId",response.Data.User.Id.ToString( ));
                    HttpContext.Session.SetString("Username",response.Data.User.Username);
                    HttpContext.Session.SetString("UserRole",response.Data.User.Role);

                    TempData["SuccessMessage"] = "Login successful!";
                    return RedirectToAction("Index","Dashboard");
                }
                else
                {
                    ModelState.AddModelError("",response.Message);
                    if (response.Errors.Any( ))
                    {
                        foreach (var error in response.Errors)
                        {
                            ModelState.AddModelError("",error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error during login");
                ModelState.AddModelError("","An error occurred during login. Please try again.");
            }

            return View(loginDto);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Call API logout endpoint
                await _apiService.LogoutAsync( );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error during logout API call");
            }
            finally
            {
                // Clear session regardless of API call result
                HttpContext.Session.Clear( );
                TempData["SuccessMessage"] = "You have been logged out successfully.";
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login");
            }

            return View( );
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(changePasswordDto);
            }

            try
            {
                var response = await _apiService.ChangePasswordAsync(changePasswordDto);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Password changed successfully!";
                    return RedirectToAction("Index","Dashboard");
                }
                else
                {
                    ModelState.AddModelError("",response.Message);
                    if (response.Errors.Any( ))
                    {
                        foreach (var error in response.Errors)
                        {
                            ModelState.AddModelError("",error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error during password change");
                ModelState.AddModelError("","An error occurred while changing password. Please try again.");
            }

            return View(changePasswordDto);
        }

        private async Task<bool> RefreshTokenAsync()
        {
            try
            {
                var refreshToken = HttpContext.Session.GetString("RefreshToken");
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return false;
                }

                var response = await _apiService.RefreshTokenAsync(refreshToken);

                if (response.Success && response.Data != null)
                {
                    // Update tokens in session
                    HttpContext.Session.SetString("AccessToken",response.Data.AccessToken);
                    HttpContext.Session.SetString("RefreshToken",response.Data.RefreshToken);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error during token refresh");
            }

            return false;
        }
    }
}

