namespace CITP.MovieApp.Application.DTOs
{
    public class LoginRequest
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;      
        public string Password { get; set; } = null!;
    }
}