namespace Aerocat.Shared
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; } // We'll store a hash, not the plain password
    }

    public class UserViewModel // A simple version of the user to show on the client
    {
        public string Username { get; set; }
        public UserStatus Status { get; set; }
    }

    public enum UserStatus
    {
        Offline,
        Online,
        Away,
        Busy
    }
}