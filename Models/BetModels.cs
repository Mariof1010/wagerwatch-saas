using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WagerWatch.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Username { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsPremium { get; set; } = false;
        [StringLength(50)]
        public string TimeZone { get; set; } = "America/New_York";
        public ICollection<Bet> Bets { get; set; } = new List<Bet>();
    }

    public class Team
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [StringLength(10)]
        public string Abbreviation { get; set; } = string.Empty;
        [StringLength(50)]
        public string City { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        [Required, StringLength(50)]
        public string Sport { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class Game
    {
        public int Id { get; set; }
        [Required]
        public int HomeTeamId { get; set; }
        [Required]
        public int AwayTeamId { get; set; }
        [Required]
        public DateTime GameTime { get; set; }
        [Required, StringLength(50)]
        public string Sport { get; set; } = string.Empty;
        [StringLength(20)]
        public string Status { get; set; } = "Scheduled";
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
        [StringLength(20)]
        public string? GamePeriod { get; set; }
        public DateTime? LastUpdated { get; set; }
        
        public Team HomeTeam { get; set; } = null!;
        public Team AwayTeam { get; set; } = null!;
        public ICollection<Bet> Bets { get; set; } = new List<Bet>();
    }

    public enum BetType { Moneyline = 1, PointSpread = 2, OverUnder = 3, Parlay = 4, Prop = 5 }
    public enum BetStatus { Active = 1, Won = 2, Lost = 3, Push = 4, Cancelled = 5 }
    public enum VolatilityLevel { Low = 1, Medium = 2, High = 3, Extreme = 4 }

    public class Bet
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int GameId { get; set; }
        [Required]
        public BetType BetType { get; set; }
        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [Required]
        public int Odds { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PotentialPayout { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }
        public decimal? Line { get; set; }
        [StringLength(50)]
        public string? Selection { get; set; }
        public BetStatus Status { get; set; } = BetStatus.Active;
        public VolatilityLevel Volatility { get; set; } = VolatilityLevel.Medium;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SettledAt { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualPayout { get; set; }
        
        public User User { get; set; } = null!;
        public Game Game { get; set; } = null!;
        
        // Aura system properties
        [NotMapped]
        public string AuraColor
        {
            get
            {
                if (Status == BetStatus.Won) return "green";
                if (Status == BetStatus.Lost) return "red";
                return "yellow";
            }
        }
        
        [NotMapped]
        public string AuraSize
        {
            get
            {
                return Volatility switch
                {
                    VolatilityLevel.Low => "large",
                    VolatilityLevel.Medium => "medium",
                    VolatilityLevel.High => "small",
                    VolatilityLevel.Extreme => "tiny",
                    _ => "medium"
                };
            }
        }
        
        [NotMapped]
        public bool ShouldPulse => Status == BetStatus.Active && Volatility >= VolatilityLevel.High;
    }

    // Authentication DTOs
    public class RegisterDto
    {
        [Required, StringLength(100)]
        public string Username { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        [StringLength(50)]
        public string TimeZone { get; set; } = "America/New_York"; // Default to Eastern
    }

    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
        public DateTime Expires { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsPremium { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeZone { get; set; } = string.Empty;
    }
}