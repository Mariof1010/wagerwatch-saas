using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WagerWatch.Data;
using WagerWatch.Models;
using WagerWatch.DTOs;
using WagerWatch.Services;
using System.Security.Claims;

namespace WagerWatch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly WagerWatchDbContext _context;
        private readonly IGameSyncService _gameSyncService;
        private readonly ITimeZoneService _timeZoneService;

        public GamesController(WagerWatchDbContext context, IGameSyncService gameSyncService, ITimeZoneService timeZoneService)
        {
            _context = context;
            _gameSyncService = gameSyncService;
            _timeZoneService = timeZoneService;
        }

        private string GetUserTimeZone()
        {
            // Try to get timezone from JWT token (authenticated user)
            var timeZoneClaim = User.FindFirst("TimeZone")?.Value;
            
            if (!string.IsNullOrEmpty(timeZoneClaim))
            {
                return timeZoneClaim;
            }

            // Default to Eastern Time for unauthenticated users or if timezone missing
            return "America/New_York";
        }

        // GET: api/games
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetGames()
        {
            var userTimeZone = GetUserTimeZone();
            
            var games = await _context.Games
                .Include(g => g.HomeTeam)
                .Include(g => g.AwayTeam)
                .OrderBy(g => g.GameTime)
                .ToListAsync();

            // Convert times to user timezone for display only
            var gamesWithUserTime = games.Select(g => new
            {
                id = g.Id,
                homeTeam = g.HomeTeam.Name,
                awayTeam = g.AwayTeam.Name,
                gameTime = _timeZoneService.ConvertToUserTimeZone(g.GameTime, userTimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
                sport = g.Sport,
                status = g.Status,
                homeScore = g.HomeScore,
                awayScore = g.AwayScore
            }).ToList();

            return Ok(gamesWithUserTime);
        }

        // GET: api/games/upcoming - Flexible upcoming games with timezone support
        [HttpGet("upcoming")]
        public async Task<ActionResult> GetUpcomingGames(
            [FromQuery] int? hours = null,
            [FromQuery] int? days = null,
            [FromQuery] string? sport = null,
            [FromQuery] string? status = null)
        {
            var userTimeZone = GetUserTimeZone();
            
            // === FILTERING: Work in UTC ===
            var utcNow = DateTime.UtcNow;
            var utcEnd = utcNow.AddDays(7); // Default: next 7 days
            
            // Apply time filters in UTC directly
            if (hours.HasValue)
            {
                utcEnd = utcNow.AddHours(hours.Value);
            }
            else if (days.HasValue)
            {
                utcEnd = utcNow.AddDays(days.Value);
            }
            
            // Build query using UTC times
            var query = _context.Games
                .Include(g => g.HomeTeam)
                .Include(g => g.AwayTeam)
                .Where(g => g.GameTime >= utcNow && g.GameTime <= utcEnd);
            
            // Apply sport filter
            if (!string.IsNullOrEmpty(sport))
            {
                query = query.Where(g => g.Sport.ToLower() == sport.ToLower());
            }
            
            // Apply status filter (default to Scheduled, but allow override)
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(g => g.Status.ToLower() == status.ToLower());
            }
            else
            {
                // Default: only scheduled games (not live/final)
                query = query.Where(g => g.Status == "Scheduled");
            }
            
            // Execute query and get raw data from database
            var gamesFromDb = await query
                .OrderBy(g => g.GameTime)
                .ToListAsync();

            // === DISPLAY: Convert to user timezone ===
            var gamesWithUserTime = gamesFromDb.Select(g => new
            {
                id = g.Id,
                homeTeam = g.HomeTeam.Name,
                awayTeam = g.AwayTeam.Name,
                gameTime = _timeZoneService.ConvertToUserTimeZone(g.GameTime, userTimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
                sport = g.Sport,
                status = g.Status,
                homeScore = g.HomeScore,
                awayScore = g.AwayScore
            }).ToList();

            // Convert filter times to user timezone for display
            var userNow = _timeZoneService.ConvertToUserTimeZone(utcNow, userTimeZone);
            var userEnd = _timeZoneService.ConvertToUserTimeZone(utcEnd, userTimeZone);

            return Ok(new
            {
                filters = new
                {
                    fromTime = userNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    toTime = userEnd.ToString("yyyy-MM-dd HH:mm:ss"),
                    timeZone = userTimeZone,
                    currentUserTime = userNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    sport = sport ?? "all",
                    status = status ?? "scheduled",
                    appliedHours = hours,
                    appliedDays = days,
                    debugInfo = new
                    {
                        utcNow = utcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        utcEnd = utcEnd.ToString("yyyy-MM-dd HH:mm:ss"),
                        filteringInUtc = true
                    }
                },
                count = gamesWithUserTime.Count,
                games = gamesWithUserTime
            });
        }

        // GET: api/games/today - Games happening today in user timezone
        [HttpGet("today")]
        public async Task<ActionResult> GetTodaysGames([FromQuery] string? sport = null)
        {
            var userTimeZone = GetUserTimeZone();
            
            // User's "today" needs to be converted to UTC range
            var userNow = _timeZoneService.ConvertToUserTimeZone(DateTime.UtcNow, userTimeZone);
            var startOfUserDay = userNow.Date;
            var endOfUserDay = startOfUserDay.AddDays(1).AddTicks(-1);
            
            // Convert user's day boundaries to UTC for filtering
            var startOfDayUtc = _timeZoneService.ConvertFromUserTimeZone(startOfUserDay, userTimeZone);
            var endOfDayUtc = _timeZoneService.ConvertFromUserTimeZone(endOfUserDay, userTimeZone);
            
            var query = _context.Games
                .Include(g => g.HomeTeam)
                .Include(g => g.AwayTeam)
                .Where(g => g.GameTime >= startOfDayUtc && g.GameTime <= endOfDayUtc);
            
            if (!string.IsNullOrEmpty(sport))
            {
                query = query.Where(g => g.Sport.ToLower() == sport.ToLower());
            }
            
            var gamesFromDb = await query
                .OrderBy(g => g.GameTime)
                .ToListAsync();

            // Convert times to user timezone for display
            var gamesWithUserTime = gamesFromDb.Select(g => new
            {
                id = g.Id,
                homeTeam = g.HomeTeam.Name,
                awayTeam = g.AwayTeam.Name,
                gameTime = _timeZoneService.ConvertToUserTimeZone(g.GameTime, userTimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
                sport = g.Sport,
                status = g.Status,
                homeScore = g.HomeScore,
                awayScore = g.AwayScore
            }).ToList();

            return Ok(new
            {
                message = "Games today",
                date = startOfUserDay.ToString("yyyy-MM-dd"),
                timeZone = userTimeZone,
                currentTime = userNow.ToString("yyyy-MM-dd HH:mm:ss"),
                sport = sport ?? "all",
                count = gamesWithUserTime.Count,
                games = gamesWithUserTime
            });
        }

        // GET: api/games/on-date/{date} - Games on specific date in user timezone
        [HttpGet("on-date/{date}")]
        public async Task<ActionResult> GetGamesOnDate(string date, [FromQuery] string? sport = null)
        {
            var userTimeZone = GetUserTimeZone();
            
            try
            {
                // Parse user's date (they think in their timezone)
                var userDate = DateTime.Parse(date);
                var startOfUserDay = userDate.Date;
                var endOfUserDay = startOfUserDay.AddDays(1).AddTicks(-1);
                
                // Convert user's day boundaries to UTC for filtering
                var startOfDayUtc = _timeZoneService.ConvertFromUserTimeZone(startOfUserDay, userTimeZone);
                var endOfDayUtc = _timeZoneService.ConvertFromUserTimeZone(endOfUserDay, userTimeZone);
                
                var query = _context.Games
                    .Include(g => g.HomeTeam)
                    .Include(g => g.AwayTeam)
                    .Where(g => g.GameTime >= startOfDayUtc && g.GameTime <= endOfDayUtc);
                
                if (!string.IsNullOrEmpty(sport))
                {
                    query = query.Where(g => g.Sport.ToLower() == sport.ToLower());
                }
                
                var gamesFromDb = await query
                    .OrderBy(g => g.GameTime)
                    .ToListAsync();

                var gamesWithUserTime = gamesFromDb.Select(g => new
                {
                    id = g.Id,
                    homeTeam = g.HomeTeam.Name,
                    awayTeam = g.AwayTeam.Name,
                    gameTime = _timeZoneService.ConvertToUserTimeZone(g.GameTime, userTimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
                    sport = g.Sport,
                    status = g.Status,
                    homeScore = g.HomeScore,
                    awayScore = g.AwayScore
                }).ToList();

                return Ok(new
                {
                    message = $"Games on {date}",
                    date = startOfUserDay.ToString("yyyy-MM-dd"),
                    timeZone = userTimeZone,
                    sport = sport ?? "all",
                    count = gamesWithUserTime.Count,
                    games = gamesWithUserTime
                });
            }
            catch (FormatException)
            {
                return BadRequest(new { error = "Invalid date format. Use YYYY-MM-DD" });
            }
        }

        // GET: api/games/next-hours/{hours} - Games in next X hours in user timezone
        [HttpGet("next-hours/{hours}")]
        public async Task<ActionResult> GetGamesInNextHours(int hours, [FromQuery] string? sport = null)
        {
            var userTimeZone = GetUserTimeZone();
            
            // === FILTERING: Work in UTC ===
            var utcNow = DateTime.UtcNow;
            var utcEnd = utcNow.AddHours(hours);
            
            var query = _context.Games
                .Include(g => g.HomeTeam)
                .Include(g => g.AwayTeam)
                .Where(g => g.GameTime >= utcNow && g.GameTime <= utcEnd && g.Status == "Scheduled");
            
            if (!string.IsNullOrEmpty(sport))
            {
                query = query.Where(g => g.Sport.ToLower() == sport.ToLower());
            }
            
            var gamesFromDb = await query
                .OrderBy(g => g.GameTime)
                .ToListAsync();

            // === DISPLAY: Convert to user timezone ===
            var gamesWithUserTime = gamesFromDb.Select(g => new
            {
                id = g.Id,
                homeTeam = g.HomeTeam.Name,
                awayTeam = g.AwayTeam.Name,
                gameTime = _timeZoneService.ConvertToUserTimeZone(g.GameTime, userTimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
                sport = g.Sport,
                status = g.Status,
                homeScore = g.HomeScore,
                awayScore = g.AwayScore
            }).ToList();

            var userNow = _timeZoneService.ConvertToUserTimeZone(utcNow, userTimeZone);
            var userEnd = _timeZoneService.ConvertToUserTimeZone(utcEnd, userTimeZone);

            return Ok(new
            {
                message = $"Games in next {hours} hours",
                timeWindow = $"{userNow:yyyy-MM-dd HH:mm} to {userEnd:yyyy-MM-dd HH:mm}",
                timeZone = userTimeZone,
                sport = sport ?? "all",
                count = gamesWithUserTime.Count,
                games = gamesWithUserTime
            });
        }

        // GET: api/games/betting-opportunities - Quick betting games in user timezone
        [HttpGet("betting-opportunities")]
        public async Task<ActionResult> GetBettingOpportunities([FromQuery] int? hoursAhead = 12)
        {
            var userTimeZone = GetUserTimeZone();
            
            // === FILTERING: Work in UTC ===
            var utcNow = DateTime.UtcNow;
            var utcEnd = utcNow.AddHours(hoursAhead ?? 12);
            
            var bettingGames = await _context.Games
                .Where(g => g.GameTime >= utcNow && 
                           g.GameTime <= utcEnd && 
                           g.Status == "Scheduled")
                .Include(g => g.HomeTeam)
                .Include(g => g.AwayTeam)
                .OrderBy(g => g.GameTime)
                .ToListAsync();

            // === DISPLAY: Convert to user timezone ===
            var result = bettingGames.Select(g => new
            {
                id = g.Id,
                matchup = $"{g.AwayTeam.Abbreviation} @ {g.HomeTeam.Abbreviation}",
                fullName = $"{g.AwayTeam.Name} @ {g.HomeTeam.Name}",
                gameTime = _timeZoneService.ConvertToUserTimeZone(g.GameTime, userTimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
                sport = g.Sport,
                hoursUntilGame = Math.Round((g.GameTime - utcNow).TotalHours, 1),
                homeTeam = new { id = g.HomeTeam.Id, name = g.HomeTeam.Name, abbreviation = g.HomeTeam.Abbreviation },
                awayTeam = new { id = g.AwayTeam.Id, name = g.AwayTeam.Name, abbreviation = g.AwayTeam.Abbreviation }
            }).ToList();

            var userNow = _timeZoneService.ConvertToUserTimeZone(utcNow, userTimeZone);

            return Ok(new
            {
                message = "Available betting opportunities",
                timeWindow = $"Next {hoursAhead ?? 12} hours",
                timeZone = userTimeZone,
                currentTime = userNow.ToString("yyyy-MM-dd HH:mm:ss"),
                count = result.Count,
                games = result
            });
        }

        // GET: api/games/live
        [HttpGet("live")]
        public async Task<ActionResult> GetLiveGames()
        {
            var userTimeZone = GetUserTimeZone();
            
            var liveGames = await _context.Games
                .Where(g => g.Status == "Live" || g.Status == "In Progress")
                .Include(g => g.HomeTeam)
                .Include(g => g.AwayTeam)
                .ToListAsync();

            // Convert times to user timezone for display
            var gamesWithUserTime = liveGames.Select(g => new
            {
                id = g.Id,
                homeTeam = g.HomeTeam.Name,
                awayTeam = g.AwayTeam.Name,
                gameTime = _timeZoneService.ConvertToUserTimeZone(g.GameTime, userTimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
                sport = g.Sport,
                status = g.Status,
                homeScore = g.HomeScore,
                awayScore = g.AwayScore
            }).ToList();

            return Ok(gamesWithUserTime);
        }

        // GET: api/games/completed
        [HttpGet("completed")]
        public async Task<ActionResult> GetCompletedGames()
        {
            var userTimeZone = GetUserTimeZone();
            
            var completedGames = await _context.Games
                .Where(g => g.Status == "Final" || g.Status == "Completed")
                .Include(g => g.HomeTeam)
                .Include(g => g.AwayTeam)
                .OrderByDescending(g => g.GameTime)
                .ToListAsync();

            // Convert times to user timezone for display
            var gamesWithUserTime = completedGames.Select(g => new
            {
                id = g.Id,
                homeTeam = g.HomeTeam.Name,
                awayTeam = g.AwayTeam.Name,
                gameTime = _timeZoneService.ConvertToUserTimeZone(g.GameTime, userTimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
                sport = g.Sport,
                status = g.Status,
                homeScore = g.HomeScore,
                awayScore = g.AwayScore
            }).ToList();

            return Ok(gamesWithUserTime);
        }

        // GET: api/games/sport/{sport}
        [HttpGet("sport/{sport}")]
        public async Task<ActionResult> GetGamesBySport(string sport)
        {
            var userTimeZone = GetUserTimeZone();
            
            var games = await _context.Games
                .Where(g => g.Sport.ToLower() == sport.ToLower())
                .Include(g => g.HomeTeam)
                .Include(g => g.AwayTeam)
                .OrderBy(g => g.GameTime)
                .ToListAsync();

            // Convert times to user timezone for display
            var gamesWithUserTime = games.Select(g => new
            {
                id = g.Id,
                homeTeam = g.HomeTeam.Name,
                awayTeam = g.AwayTeam.Name,
                gameTime = _timeZoneService.ConvertToUserTimeZone(g.GameTime, userTimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
                sport = g.Sport,
                status = g.Status,
                homeScore = g.HomeScore,
                awayScore = g.AwayScore
            }).ToList();

            return Ok(gamesWithUserTime);
        }

        // GET: api/games/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult> GetGame(int id)
        {
            var userTimeZone = GetUserTimeZone();
            
            var game = await _context.Games
                .Include(g => g.HomeTeam)
                .Include(g => g.AwayTeam)
                .Where(g => g.Id == id)
                .FirstOrDefaultAsync();

            if (game == null)
            {
                return NotFound();
            }

            // Convert time to user timezone for display
            var gameWithUserTime = new
            {
                id = game.Id,
                homeTeam = game.HomeTeam.Name,
                awayTeam = game.AwayTeam.Name,
                gameTime = _timeZoneService.ConvertToUserTimeZone(game.GameTime, userTimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
                sport = game.Sport,
                status = game.Status,
                homeScore = game.HomeScore,
                awayScore = game.AwayScore
            };

            return Ok(gameWithUserTime);
        }

        // PUT: api/games/{id}/score
        [HttpPut("{id}/score")]
        public async Task<IActionResult> UpdateGameScore(int id, [FromBody] UpdateScoreRequest request)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            game.HomeScore = request.HomeScore;
            game.AwayScore = request.AwayScore;
            game.Status = request.Status ?? game.Status;
            game.GamePeriod = request.GamePeriod;
            game.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/games/{id}/bets
        [HttpGet("{id}/bets")]
        public async Task<ActionResult<IEnumerable<BetDisplayDto>>> GetGameBets(int id)
        {
            var userTimeZone = GetUserTimeZone();
            
            var gameBets = await _context.Bets
                .Where(b => b.GameId == id)
                .Include(b => b.Game)
                    .ThenInclude(g => g.HomeTeam)
                .Include(b => b.Game)
                    .ThenInclude(g => g.AwayTeam)
                .ToListAsync();

            // Convert game times to user timezone
            var betsWithUserTime = gameBets.Select(b => new BetDisplayDto
            {
                Id = b.Id,
                HomeTeam = b.Game.HomeTeam.Name,
                AwayTeam = b.Game.AwayTeam.Name,
                GameTime = _timeZoneService.ConvertToUserTimeZone(b.Game.GameTime, userTimeZone),
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
            }).ToList();

            return Ok(betsWithUserTime);
        }

        // GET: api/games/timezone/info - Get current timezone info
        [HttpGet("timezone/info")]
        public ActionResult GetTimeZoneInfo()
        {
            var userTimeZone = GetUserTimeZone();
            var utcNow = DateTime.UtcNow;
            var userNow = _timeZoneService.ConvertToUserTimeZone(utcNow, userTimeZone);
            
            return Ok(new
            {
                userTimeZone = userTimeZone,
                utcTime = utcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                userTime = userNow.ToString("yyyy-MM-dd HH:mm:ss"),
                availableTimeZones = _timeZoneService.GetAvailableTimeZones()
            });
        }

        // GET: api/games/debug/timezone-test - Debug timezone conversions
        [HttpGet("debug/timezone-test")]
        public ActionResult DebugTimezoneTest()
        {
            var userTimeZone = GetUserTimeZone();
            var utcNow = DateTime.UtcNow;
            
            // Test specific times
            var easternTime4PM = new DateTime(2025, 7, 11, 16, 0, 0); // 4:00 PM Eastern
            var easternTime410PM = new DateTime(2025, 7, 11, 16, 10, 0); // 4:10 PM Eastern
            
            var utc4PM = _timeZoneService.ConvertFromUserTimeZone(easternTime4PM, userTimeZone);
            var utc410PM = _timeZoneService.ConvertFromUserTimeZone(easternTime410PM, userTimeZone);
            var backToEastern = _timeZoneService.ConvertToUserTimeZone(utc4PM, userTimeZone);
            
            return Ok(new
            {
                userTimeZone = userTimeZone,
                currentUtc = utcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                currentUser = _timeZoneService.ConvertToUserTimeZone(utcNow, userTimeZone).ToString("yyyy-MM-dd HH:mm:ss"),
                
                tests = new
                {
                    eastern4PM = easternTime4PM.ToString("yyyy-MM-dd HH:mm:ss"),
                    utc4PM = utc4PM.ToString("yyyy-MM-dd HH:mm:ss"),
                    expectedUtc4PM = "2025-07-11 20:00:00", // Should be 8 PM UTC
                    
                    eastern410PM = easternTime410PM.ToString("yyyy-MM-dd HH:mm:ss"),
                    utc410PM = utc410PM.ToString("yyyy-MM-dd HH:mm:ss"),
                    expectedUtc410PM = "2025-07-11 20:10:00", // Should be 8:10 PM UTC
                    
                    backToEastern = backToEastern.ToString("yyyy-MM-dd HH:mm:ss"),
                    roundTripWorking = backToEastern.ToString("HH:mm") == "16:00"
                },
                
                conversionWorking = utc4PM.ToString("HH:mm") == "20:00"
            });
        }

        // POST: api/games/sync/teams - Sync teams from ESPN
        [HttpPost("sync/teams")]
        public async Task<IActionResult> SyncTeams()
        {
            try
            {
                await _gameSyncService.SyncTeamsAsync();
                
                var teamCount = await _context.Teams.CountAsync();
                return Ok(new
                {
                    message = "Teams synced successfully from ESPN",
                    totalTeams = teamCount,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = "Failed to sync teams",
                    details = ex.Message
                });
            }
        }

        // POST: api/games/sync/games - Sync games from ESPN
        [HttpPost("sync/games")]
        public async Task<IActionResult> SyncGames()
        {
            try
            {
                await _gameSyncService.SyncTodaysGamesAsync();
                
                var gameCount = await _context.Games.CountAsync();
                var todaysGames = await _context.Games
                    .Where(g => g.GameTime.Date == DateTime.Today)
                    .CountAsync();
                
                return Ok(new
                {
                    message = "Games synced successfully from ESPN",
                    totalGames = gameCount,
                    todaysGames = todaysGames,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = "Failed to sync games",
                    details = ex.Message
                });
            }
        }

        // POST: api/games/sync/scores - Update live scores from ESPN
        [HttpPost("sync/scores")]
        public async Task<IActionResult> UpdateScores()
        {
            try
            {
                await _gameSyncService.UpdateLiveScoresAsync();
                
                var liveGames = await _context.Games
                    .Where(g => g.Status == "Live" || g.Status == "Halftime")
                    .Include(g => g.HomeTeam)
                    .Include(g => g.AwayTeam)
                    .Select(g => new
                    {
                        game = $"{g.AwayTeam.Name} @ {g.HomeTeam.Name}",
                        score = $"{g.AwayScore}-{g.HomeScore}",
                        status = g.Status
                    })
                    .ToListAsync();

                return Ok(new
                {
                    message = "Live scores updated from ESPN",
                    liveGamesCount = liveGames.Count,
                    liveGames = liveGames,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = "Failed to update scores",
                    details = ex.Message
                });
            }
        }

        // POST: api/games/sync/all - Full sync from ESPN
        [HttpPost("sync/all")]
        public async Task<IActionResult> SyncAll()
        {
            try
            {
                // Sync teams first
                await _gameSyncService.SyncTeamsAsync();
                
                // Then sync games
                await _gameSyncService.SyncTodaysGamesAsync();
                
                // Finally update scores
                await _gameSyncService.UpdateLiveScoresAsync();
                
                var stats = new
                {
                    totalTeams = await _context.Teams.CountAsync(),
                    totalGames = await _context.Games.CountAsync(),
                    todaysGames = await _context.Games.Where(g => g.GameTime.Date == DateTime.Today).CountAsync(),
                    liveGames = await _context.Games.Where(g => g.Status == "Live").CountAsync()
                };

                return Ok(new
                {
                    message = "Full sync completed successfully",
                    stats = stats,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = "Failed to complete full sync",
                    details = ex.Message
                });
            }
        }
    }

    public class UpdateScoreRequest
    {
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public string? Status { get; set; }
        public string? GamePeriod { get; set; }
    }
}