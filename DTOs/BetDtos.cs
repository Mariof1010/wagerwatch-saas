using System.ComponentModel.DataAnnotations;
using WagerWatch.Models;

namespace WagerWatch.DTOs
{
    public class CreateBetDto
    {
        [Required]
        public int GameId { get; set; }
        [Required]
        public BetType BetType { get; set; }
        [Required, Range(0.01, 10000)]
        public decimal Amount { get; set; }
        [Required]
        public int Odds { get; set; }
        public decimal? Line { get; set; }
        [StringLength(50)]
        public string? Selection { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class BetDisplayDto
    {
        public int Id { get; set; }
        public string HomeTeam { get; set; } = string.Empty;
        public string AwayTeam { get; set; } = string.Empty;
        public DateTime GameTime { get; set; }
        public BetType BetType { get; set; }
        public decimal Amount { get; set; }
        public int Odds { get; set; }
        public decimal PotentialPayout { get; set; }
        public BetStatus Status { get; set; }
        public string AuraColor { get; set; } = string.Empty;
        public string AuraSize { get; set; } = string.Empty;
        public bool ShouldPulse { get; set; }
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
        public string GameStatus { get; set; } = string.Empty;
        public string? Selection { get; set; }
        public decimal? Line { get; set; }
        public string? Description { get; set; }
    }

    public class GameDto
    {
        public int Id { get; set; }
        public string HomeTeam { get; set; } = string.Empty;
        public string AwayTeam { get; set; } = string.Empty;
        public DateTime GameTime { get; set; }
        public string Sport { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
    }

    public class UserStatsDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsPremium { get; set; }
        public int TotalBets { get; set; }
        public decimal TotalWagered { get; set; }
        public decimal TotalWon { get; set; }
        public decimal WinPercentage { get; set; }
        public decimal ROI { get; set; }
    }
}