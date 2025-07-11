using Microsoft.EntityFrameworkCore;
using WagerWatch.Models;

namespace WagerWatch.Data
{
    public class WagerWatchDbContext : DbContext
    {
        public WagerWatchDbContext(DbContextOptions<WagerWatchDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Bet> Bets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.TimeZone).HasMaxLength(50).HasDefaultValue("America/New_York");
            });

            // Team configuration
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.Name, e.Sport }).IsUnique();
            });

            // Game configuration
            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasDefaultValue("Scheduled");
                entity.HasOne(e => e.HomeTeam).WithMany().HasForeignKey(e => e.HomeTeamId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.AwayTeam).WithMany().HasForeignKey(e => e.AwayTeamId).OnDelete(DeleteBehavior.Restrict);
            });

            // Bet configuration
            modelBuilder.Entity<Bet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PotentialPayout).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ActualPayout).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Line).HasColumnType("decimal(18,2)"); // Fix the Line decimal warning
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(e => e.User).WithMany(u => u.Bets).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Game).WithMany(g => g.Bets).HasForeignKey(e => e.GameId).OnDelete(DeleteBehavior.Restrict);
            });

            // Seed sample data with static dates (no dynamic DateTime.UtcNow)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Add sample teams
            modelBuilder.Entity<Team>().HasData(
                new Team { Id = 1, Name = "Kansas City Chiefs", Abbreviation = "KC", City = "Kansas City", Sport = "NFL" },
                new Team { Id = 2, Name = "Buffalo Bills", Abbreviation = "BUF", City = "Buffalo", Sport = "NFL" },
                new Team { Id = 3, Name = "Dallas Cowboys", Abbreviation = "DAL", City = "Dallas", Sport = "NFL" },
                new Team { Id = 4, Name = "Green Bay Packers", Abbreviation = "GB", City = "Green Bay", Sport = "NFL" },
                new Team { Id = 5, Name = "Los Angeles Lakers", Abbreviation = "LAL", City = "Los Angeles", Sport = "NBA" },
                new Team { Id = 6, Name = "Boston Celtics", Abbreviation = "BOS", City = "Boston", Sport = "NBA" },
                new Team { Id = 7, Name = "Golden State Warriors", Abbreviation = "GSW", City = "San Francisco", Sport = "NBA" },
                new Team { Id = 8, Name = "Miami Heat", Abbreviation = "MIA", City = "Miami", Sport = "NBA" }
            );

            // Add sample games with STATIC dates (not DateTime.UtcNow)
            modelBuilder.Entity<Game>().HasData(
                new Game { Id = 1, HomeTeamId = 1, AwayTeamId = 2, GameTime = new DateTime(2025, 7, 11, 20, 0, 0, DateTimeKind.Utc), Sport = "NFL", Status = "Scheduled" },
                new Game { Id = 2, HomeTeamId = 3, AwayTeamId = 4, GameTime = new DateTime(2025, 7, 12, 17, 0, 0, DateTimeKind.Utc), Sport = "NFL", Status = "Scheduled" },
                new Game { Id = 3, HomeTeamId = 5, AwayTeamId = 6, GameTime = new DateTime(2025, 7, 11, 23, 0, 0, DateTimeKind.Utc), Sport = "NBA", Status = "Scheduled" },
                new Game { Id = 4, HomeTeamId = 7, AwayTeamId = 8, GameTime = new DateTime(2025, 7, 13, 19, 0, 0, DateTimeKind.Utc), Sport = "NBA", Status = "Scheduled" }
            );

            // Add sample user with static date and timezone
            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1, 
                    Username = "testuser", 
                    Email = "test@wagerwatch.com", 
                    PasswordHash = "hashedpassword123", 
                    IsPremium = true,
                    CreatedAt = new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                    TimeZone = "America/New_York",
                    IsActive = true
                }
            );
        }
    }

    public static class DatabaseExtensions
    {
        public static void ConfigureDatabase(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<WagerWatchDbContext>(options => options.UseSqlServer(connectionString));
        }

        public static async Task InitializeDatabaseAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WagerWatchDbContext>();
            await context.Database.MigrateAsync();
        }
    }
}