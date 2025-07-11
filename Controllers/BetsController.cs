using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WagerWatch.Data;
using WagerWatch.Models;
using WagerWatch.DTOs;

namespace WagerWatch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all endpoints
    public class BetsController : ControllerBase
    {
        private readonly WagerWatchDbContext _context;

        public BetsController(WagerWatchDbContext context)
        {
            _context = context;
        }

        // Helper method to get current user ID from JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        // GET: api/bets - Only user's own bets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BetDisplayDto>>> GetBets()
        {
            var userId = GetCurrentUserId();
            
            var bets = await _context.Bets
                .Where(b => b.UserId == userId) // Only this user's bets
                .Include(b => b.Game)
                    .ThenInclude(g => g.HomeTeam)
                .Include(b => b.Game)
                    .ThenInclude(g => g.AwayTeam)
                .Include(b => b.User)
                .Select(b => new BetDisplayDto
                {
                    Id = b.Id,
                    HomeTeam = b.Game.HomeTeam.Name,
                    AwayTeam = b.Game.AwayTeam.Name,
                    GameTime = b.Game.GameTime,
                    BetType = b.BetType,
                    Amount = b.Amount,
                    Odds = b.Odds,
                    PotentialPayout = b.PotentialPayout,
                    Status = b.Status,
                    AuraColor = b.AuraColor,
                    AuraSize = b.AuraSize,
                    ShouldPulse = b.ShouldPulse,
                    HomeScore = b.Game.HomeScore,
                    AwayScore = b.Game.AwayScore,
                    GameStatus = b.Game.Status,
                    Selection = b.Selection,
                    Line = b.Line,
                    Description = b.Description
                })
                .ToListAsync();

            return Ok(bets);
        }

        // GET: api/bets/active - Only user's active bets
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BetDisplayDto>>> GetActiveBets()
        {
            var userId = GetCurrentUserId();
            
            var activeBets = await _context.Bets
                .Where(b => b.UserId == userId && b.Status == BetStatus.Active) // User's active bets only
                .Include(b => b.Game)
                    .ThenInclude(g => g.HomeTeam)
                .Include(b => b.Game)
                    .ThenInclude(g => g.AwayTeam)
                .Select(b => new BetDisplayDto
                {
                    Id = b.Id,
                    HomeTeam = b.Game.HomeTeam.Name,
                    AwayTeam = b.Game.AwayTeam.Name,
                    GameTime = b.Game.GameTime,
                    BetType = b.BetType,
                    Amount = b.Amount,
                    Odds = b.Odds,
                    PotentialPayout = b.PotentialPayout,
                    Status = b.Status,
                    AuraColor = b.AuraColor,
                    AuraSize = b.AuraSize,
                    ShouldPulse = b.ShouldPulse,
                    HomeScore = b.Game.HomeScore,
                    AwayScore = b.Game.AwayScore,
                    GameStatus = b.Game.Status,
                    Selection = b.Selection,
                    Line = b.Line,
                    Description = b.Description
                })
                .ToListAsync();

            return Ok(activeBets);
        }

        // GET: api/bets/settled - Only user's settled bets
        [HttpGet("settled")]
        public async Task<ActionResult<IEnumerable<BetDisplayDto>>> GetSettledBets()
        {
            var userId = GetCurrentUserId();
            
            var settledBets = await _context.Bets
                .Where(b => b.UserId == userId && b.Status != BetStatus.Active) // User's settled bets only
                .Include(b => b.Game)
                    .ThenInclude(g => g.HomeTeam)
                .Include(b => b.Game)
                    .ThenInclude(g => g.AwayTeam)
                .OrderByDescending(b => b.SettledAt)
                .Select(b => new BetDisplayDto
                {
                    Id = b.Id,
                    HomeTeam = b.Game.HomeTeam.Name,
                    AwayTeam = b.Game.AwayTeam.Name,
                    GameTime = b.Game.GameTime,
                    BetType = b.BetType,
                    Amount = b.Amount,
                    Odds = b.Odds,
                    PotentialPayout = b.PotentialPayout,
                    Status = b.Status,
                    AuraColor = b.AuraColor,
                    AuraSize = b.AuraSize,
                    ShouldPulse = b.ShouldPulse,
                    HomeScore = b.Game.HomeScore,
                    AwayScore = b.Game.AwayScore,
                    GameStatus = b.Game.Status,
                    Selection = b.Selection,
                    Line = b.Line,
                    Description = b.Description
                })
                .ToListAsync();

            return Ok(settledBets);
        }

        // GET: api/bets/{id} - Only if bet belongs to current user
        [HttpGet("{id}")]
        public async Task<ActionResult<BetDisplayDto>> GetBet(int id)
        {
            var userId = GetCurrentUserId();
            
            var bet = await _context.Bets
                .Where(b => b.Id == id && b.UserId == userId) // Security check
                .Include(b => b.Game)
                    .ThenInclude(g => g.HomeTeam)
                .Include(b => b.Game)
                    .ThenInclude(g => g.AwayTeam)
                .Select(b => new BetDisplayDto
                {
                    Id = b.Id,
                    HomeTeam = b.Game.HomeTeam.Name,
                    AwayTeam = b.Game.AwayTeam.Name,
                    GameTime = b.Game.GameTime,
                    BetType = b.BetType,
                    Amount = b.Amount,
                    Odds = b.Odds,
                    PotentialPayout = b.PotentialPayout,
                    Status = b.Status,
                    AuraColor = b.AuraColor,
                    AuraSize = b.AuraSize,
                    ShouldPulse = b.ShouldPulse,
                    HomeScore = b.Game.HomeScore,
                    AwayScore = b.Game.AwayScore,
                    GameStatus = b.Game.Status,
                    Selection = b.Selection,
                    Line = b.Line,
                    Description = b.Description
                })
                .FirstOrDefaultAsync();

            if (bet == null)
            {
                return NotFound("Bet not found or access denied");
            }

            return Ok(bet);
        }

        // POST: api/bets - Create bet for current user
        [HttpPost]
        public async Task<ActionResult<BetDisplayDto>> CreateBet(CreateBetDto createBetDto)
        {
            var userId = GetCurrentUserId();
            
            // Calculate potential payout (simplified calculation)
            var potentialPayout = CalculatePotentialPayout(createBetDto.Amount, createBetDto.Odds);

            var bet = new Bet
            {
                UserId = userId, // Use authenticated user's ID
                GameId = createBetDto.GameId,
                BetType = createBetDto.BetType,
                Amount = createBetDto.Amount,
                Odds = createBetDto.Odds,
                PotentialPayout = potentialPayout,
                Line = createBetDto.Line,
                Selection = createBetDto.Selection,
                Description = createBetDto.Description,
                Status = BetStatus.Active,
                Volatility = VolatilityLevel.Medium // Default volatility
            };

            _context.Bets.Add(bet);
            await _context.SaveChangesAsync();

            // Return the created bet with all display information
            var createdBet = await GetBet(bet.Id);
            return CreatedAtAction(nameof(GetBet), new { id = bet.Id }, createdBet.Value);
        }

        // PUT: api/bets/{id}/settle - Only if bet belongs to current user
        [HttpPut("{id}/settle")]
        public async Task<IActionResult> SettleBet(int id, [FromBody] SettleBetRequest request)
        {
            var userId = GetCurrentUserId();
            
            var bet = await _context.Bets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId); // Security check
            
            if (bet == null)
            {
                return NotFound("Bet not found or access denied");
            }

            if (bet.Status != BetStatus.Active)
            {
                return BadRequest("Bet is already settled");
            }

            bet.Status = request.Status;
            bet.SettledAt = DateTime.UtcNow;
            
            if (request.Status == BetStatus.Won)
            {
                bet.ActualPayout = bet.PotentialPayout;
            }
            else if (request.Status == BetStatus.Lost)
            {
                bet.ActualPayout = 0;
            }
            else if (request.Status == BetStatus.Push)
            {
                bet.ActualPayout = bet.Amount; // Return original bet amount
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/bets/{id}/volatility - Only if bet belongs to current user
        [HttpPut("{id}/volatility")]
        public async Task<IActionResult> UpdateVolatility(int id, [FromBody] UpdateVolatilityRequest request)
        {
            var userId = GetCurrentUserId();
            
            var bet = await _context.Bets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId); // Security check
            
            if (bet == null)
            {
                return NotFound("Bet not found or access denied");
            }

            bet.Volatility = request.Volatility;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/bets/{id} - Only if bet belongs to current user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBet(int id)
        {
            var userId = GetCurrentUserId();
            
            var bet = await _context.Bets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId); // Security check
            
            if (bet == null)
            {
                return NotFound("Bet not found or access denied");
            }

            _context.Bets.Remove(bet);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/bets/stats - User's betting statistics
        [HttpGet("stats")]
        public async Task<ActionResult<UserStatsDto>> GetUserStats()
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return NotFound();
            }

            var userBets = await _context.Bets
                .Where(b => b.UserId == userId)
                .ToListAsync();

            var totalBets = userBets.Count;
            var totalWagered = userBets.Sum(b => b.Amount);
            var totalWon = userBets.Where(b => b.ActualPayout.HasValue).Sum(b => b.ActualPayout.Value);
            var settledBets = userBets.Where(b => b.Status != BetStatus.Active).ToList();
            var wonBets = settledBets.Where(b => b.Status == BetStatus.Won).Count();
            
            var winPercentage = settledBets.Count > 0 ? (decimal)wonBets / settledBets.Count * 100 : 0;
            var roi = totalWagered > 0 ? (totalWon - totalWagered) / totalWagered * 100 : 0;

            return Ok(new UserStatsDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsPremium = user.IsPremium,
                TotalBets = totalBets,
                TotalWagered = totalWagered,
                TotalWon = totalWon,
                WinPercentage = winPercentage,
                ROI = roi
            });
        }

        private decimal CalculatePotentialPayout(decimal amount, int odds)
        {
            if (odds > 0)
            {
                // American odds positive (underdog)
                return amount + (amount * odds / 100);
            }
            else
            {
                // American odds negative (favorite)
                return amount + (amount * 100 / Math.Abs(odds));
            }
        }
    }

    public class SettleBetRequest
    {
        public BetStatus Status { get; set; }
    }

    public class UpdateVolatilityRequest
    {
        public VolatilityLevel Volatility { get; set; }
    }
}