// In project: Aerocat.Server
// File: Data/AerocatDbContext.cs
using Aerocat.Shared;
using Microsoft.EntityFrameworkCore;

namespace Aerocat.Server.Data
{
    public class AerocatDbContext : DbContext
    {
        public AerocatDbContext(DbContextOptions<AerocatDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        // We'll add Friends later if needed, starting simple
    }
}