using FamilyTaskManagement.Web.Models;
using Newtonsoft.Json;
using System.Text;

namespace FamilyTaskManagement.Web.Services
{
    public interface IApiService
    {
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(string refreshToken);
        Task<ApiResponse<object>> LogoutAsync();
        Task<ApiResponse<UserDto>> GetCurrentUserAsync();
        Task<ApiResponse<List<TaskDto>>> GetTasksAsync();
        Task<ApiResponse<TaskDto>> GetTaskByIdAsync(int id);
        Task<ApiResponse<TaskDto>> CreateTaskAsync(CreateTaskDto createTaskDto);
        Task<ApiResponse<TaskDto>> UpdateTaskStatusAsync(int id,UpdateTaskStatusDto updateTaskStatusDto);
        Task<ApiResponse<List<TaskAssignmentDto>>> GetMyAssignmentsAsync();
        Task<ApiResponse<TaskAssignmentDto>> CreateTaskAssignmentAsync(CreateTaskAssignmentDto createTaskAssignmentDto);
        Task<ApiResponse<List<UserDto>>> GetUsersAsync();
        Task<ApiResponse<List<NotificationDto>>> GetNotificationsAsync();
        Task<ApiResponse<object>> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiService> _logger;
        private readonly string _baseUrl;
        private string? _accessToken; // ✅ جديد

        public ApiService(HttpClient httpClient,IHttpContextAccessor httpContextAccessor,ILogger<ApiService> logger,IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5002";
        }

        private void SetAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
            }
            else
            {
                // Clear authorization header if no token
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        private async Task<ApiResponse<T>> SendRequestAsync<T>(HttpMethod method,string endpoint,object? data = null)
        {
            try
            {
                SetAuthorizationHeader( );

                var request = new HttpRequestMessage(method,$"{_baseUrl}{endpoint}");

                if (data != null)
                {
                    var json = JsonConvert.SerializeObject(data);
                    request.Content = new StringContent(json,Encoding.UTF8,"application/json");
                }

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync( );

                if (response.IsSuccessStatusCode)
                {
                    if (typeof(T) == typeof(object))
                    {
                        return new ApiResponse<T>
                        {
                            Success = true,
                            Message = "Success"
                        };
                    }

                    var result = JsonConvert.DeserializeObject<T>(content);
                    return new ApiResponse<T>
                    {
                        Success = true,
                        Data = result,
                        Message = "Success"
                    };
                }
                else
                {
                    _logger.LogError($"API request failed: {response.StatusCode} - {content}");
                    return new ApiResponse<T>
                    {
                        Success = false,
                        Message = $"Request failed: {response.StatusCode}",
                        Errors = new List<string> { content }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"Error calling API endpoint: {endpoint}");
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = "An error occurred while calling the API",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                SetAuthorizationHeader( );

                var request = new HttpRequestMessage(HttpMethod.Post,$"{_baseUrl}/api/Auth/login");
                var json = JsonConvert.SerializeObject(loginDto);
                request.Content = new StringContent(json,Encoding.UTF8,"application/json");

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync( );

                if (response.IsSuccessStatusCode)
                {
                    // The API returns the LoginResponseDto directly, not wrapped in ApiResponse
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponseDto>(content);

                    // Store the token for immediate use
                    if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        loginResponse.AccessToken = loginResponse.Token;
                        _accessToken = loginResponse.Token;

                        // Store in session immediately
                        _httpContextAccessor.HttpContext?.Session.SetString("AccessToken",loginResponse.Token);
                        if (!string.IsNullOrEmpty(loginResponse.RefreshToken))
                        {
                            _httpContextAccessor.HttpContext?.Session.SetString("RefreshToken",loginResponse.RefreshToken);
                        }
                    }

                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = true,
                        Data = loginResponse,
                        Message = "Login successful"
                    };
                }
                else
                {
                    _logger.LogError($"Login failed: {response.StatusCode} - {content}");
                    return new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = $"Login failed: {response.StatusCode}",
                        Errors = new List<string> { content }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error during login");
                return new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "An error occurred during login",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            var refreshTokenDto = new RefreshTokenDto { RefreshToken = refreshToken };
            return await SendRequestAsync<LoginResponseDto>(HttpMethod.Post,"/api/Auth/refresh",refreshTokenDto);
        }

        public async Task<ApiResponse<object>> LogoutAsync()
        {
            return await SendRequestAsync<object>(HttpMethod.Post,"/api/Auth/logout");
        }

        public async Task<ApiResponse<UserDto>> GetCurrentUserAsync()
        {
            return await SendRequestAsync<UserDto>(HttpMethod.Get,"/api/Auth/me");
        }

        public async Task<ApiResponse<List<TaskDto>>> GetTasksAsync()
        {
            return await SendRequestAsync<List<TaskDto>>(HttpMethod.Get,"/api/Tasks");
        }

        public async Task<ApiResponse<TaskDto>> GetTaskByIdAsync(int id)
        {
            return await SendRequestAsync<TaskDto>(HttpMethod.Get,$"/api/Tasks/{id}");
        }

        public async Task<ApiResponse<TaskDto>> CreateTaskAsync(CreateTaskDto createTaskDto)
        {
            return await SendRequestAsync<TaskDto>(HttpMethod.Post,"/api/Tasks",createTaskDto);
        }

        public async Task<ApiResponse<TaskDto>> UpdateTaskStatusAsync(int id,UpdateTaskStatusDto updateTaskStatusDto)
        {
            return await SendRequestAsync<TaskDto>(HttpMethod.Put,$"/api/Tasks/{id}/status",updateTaskStatusDto);
        }

        public async Task<ApiResponse<List<TaskAssignmentDto>>> GetMyAssignmentsAsync()
        {
            return await SendRequestAsync<List<TaskAssignmentDto>>(HttpMethod.Get,"/api/TaskAssignments/my-assignments");
        }

        public async Task<ApiResponse<TaskAssignmentDto>> CreateTaskAssignmentAsync(CreateTaskAssignmentDto createTaskAssignmentDto)
        {
            return await SendRequestAsync<TaskAssignmentDto>(HttpMethod.Post,"/api/TaskAssignments",createTaskAssignmentDto);
        }

        public async Task<ApiResponse<List<UserDto>>> GetUsersAsync()
        {
            return await SendRequestAsync<List<UserDto>>(HttpMethod.Get,"/api/Users");
        }

        public async Task<ApiResponse<List<NotificationDto>>> GetNotificationsAsync()
        {
            return await SendRequestAsync<List<NotificationDto>>(HttpMethod.Get,"/api/Users/notifications");
        }

        public async Task<ApiResponse<object>> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            return await SendRequestAsync<object>(HttpMethod.Post,"/api/Auth/change-password",changePasswordDto);
        }
    }
}

