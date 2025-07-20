// In project: Aerocat.Server
// File: Hubs/ChatHub.cs
using Aerocat.Server.Data;
using Aerocat.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Aerocat.Server.Hubs
{
    public class ChatHub : Hub
    {
        // A thread-safe dictionary to map usernames to their SignalR connection IDs
        private static readonly ConcurrentDictionary<string, string> OnlineUsers = new ConcurrentDictionary<string, string>();
        private readonly AerocatDbContext _context;

        public ChatHub(AerocatDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Register(string username, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                return false; // Username already exists
            }

            var user = new User
            {
                Username = username,
                // IMPORTANT: In a real app, hash the password! This is a simplified example.
                // Use a library like BCrypt.Net: PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
                PasswordHash = password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Notify all clients that a new user has registered (for real-time user searching)
            await Clients.All.SendAsync("UserRegistered", username);

            return true;
        }

        public async Task<bool> Login(string username, string password)
        {
            // IMPORTANT: In a real app, you would verify the hashed password.
            // var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            // if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return false;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password);

            if (user == null)
            {
                return false; // Login failed
            }

            // Remove any existing connection for this user
            if (OnlineUsers.ContainsKey(username))
            {
                OnlineUsers.TryRemove(username, out _);
            }

            // Add new connection
            OnlineUsers.TryAdd(username, Context.ConnectionId);

            // Notify other clients that this user is now online
            await Clients.AllExcept(Context.ConnectionId).SendAsync("UserStatusChanged", new UserViewModel { Username = username, Status = UserStatus.Online });

            return true;
        }

        public async Task<List<UserViewModel>> GetOnlineUsers()
        {
            // Return a list of all currently logged-in users
            return OnlineUsers.Keys.Select(username => new UserViewModel { Username = username, Status = UserStatus.Online }).ToList();
        }

        public async Task SendMessage(string receiverUsername, string messageContent)
        {
            var senderUsername = OnlineUsers.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (senderUsername == null) return; // Not logged in

            var message = new Message
            {
                SenderUsername = senderUsername,
                ReceiverUsername = receiverUsername,
                Content = messageContent,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Check if receiver is online and send the message in real-time
            if (OnlineUsers.TryGetValue(receiverUsername, out var receiverConnectionId))
            {
                await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", message);
            }

            // Also send it to yourself so your own chat window updates
            await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", message);
        }

        public async Task<List<string>> SearchUsers(string query)
        {
            return await _context.Users
                .Where(u => u.Username.Contains(query))
                .Select(u => u.Username)
                .Take(10) // Limit results
                .ToListAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = OnlineUsers.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (username != null)
            {
                OnlineUsers.TryRemove(username, out _);
                // Notify other clients that this user is now offline
                await Clients.All.SendAsync("UserStatusChanged", new UserViewModel { Username = username, Status = UserStatus.Offline });
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}