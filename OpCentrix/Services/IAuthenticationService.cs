using OpCentrix.Models;

namespace OpCentrix.Services
{
    public interface IAuthenticationService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<bool> LoginAsync(HttpContext context, User user);
        Task LogoutAsync(HttpContext context);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
        Task<User?> GetCurrentUserAsync(HttpContext context);
        Task<User?> GetCurrentUserWithSettingsAsync(HttpContext context);
        Task<List<User>> GetAllUsersAsync();
        Task<User?> CreateUserAsync(User user, string password);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, string newPassword);
    }
}