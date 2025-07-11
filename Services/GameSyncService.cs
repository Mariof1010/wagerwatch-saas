using Microsoft.EntityFrameworkCore;
using WagerWatch.Data;
using WagerWatch.Models;
using WagerWatch.Services;

namespace WagerWatch.Services
{
    public interface IGameSyncService
    {
        Task SyncTodaysGamesAsync();
        Task UpdateLiveScoresAsync();
        Task SyncTeamsAsync();
    }

    public class GameSyncService : IGameSyncService
    {
        private readonly WagerWatchDbContext _context;
        private readonly IEspnService _espnService;
        private readonly ILogger<GameSyncService> _logger;

        public GameSyncService(WagerWatchDbContext context, IEspnService espnService, ILogger<GameSyncService> logger)
        {
            _context = context;
            _espnService = espnService;
            _logger = logger;
        }

        public async Task SyncTodaysGamesAsync()
        {
            try
            {
                _logger.LogInformation("Starting game sync from ESPN...");

                // Get ESPN games from all sports
                var nflGames = await _espnService.GetNflGamesAsync();
                var nbaGames = await _espnService.GetNbaGamesAsync();
                var mlbGames = await _espnService.GetMlbGamesAsync();
                var nhlGames = await _espnService.GetNhlGamesAsync();
                
                var allEspnGames = nflGames.Concat(nbaGames).Concat(mlbGames).Concat(nhlGames).ToList();
                _logger.LogInformation($"Retrieved total of {allEspnGames.Count} games from ESPN");

                foreach (var espnGame in allEspnGames)
                {
                    // Find or create teams
                    var homeTeam = await FindOrCreateTeamAsync(espnGame.HomeTeam, espnGame.Sport);
                    var awayTeam = await FindOrCreateTeamAsync(espnGame.AwayTeam, espnGame.Sport);

                    // Check if game already exists
                    var existingGame = await _context.Games
                        .FirstOrDefaultAsync(g => g.HomeTeamId == homeTeam.Id && 
                                                 g.AwayTeamId == awayTeam.Id && 
                                                 g.GameTime.Date == espnGame.Date.Date);

                    if (existingGame == null)
                    {
                        // Create new game
                        var newGame = new Game
                        {
                            HomeTeamId = homeTeam.Id,
                            AwayTeamId = awayTeam.Id,
                            GameTime = espnGame.Date,
                            Sport = espnGame.Sport,
                            Status = MapEspnStatus(espnGame.Status),
                            HomeScore = espnGame.HomeTeam.Score,
                            AwayScore = espnGame.AwayTeam.Score,
                            LastUpdated = DateTime.UtcNow
                        };

                        _context.Games.Add(newGame);
                        _logger.LogInformation($"Added new game: {espnGame.Name}");
                    }
                    else
                    {
                        // Update existing game
                        existingGame.Status = MapEspnStatus(espnGame.Status);
                        existingGame.HomeScore = espnGame.HomeTeam.Score;
                        existingGame.AwayScore = espnGame.AwayTeam.Score;
                        existingGame.LastUpdated = DateTime.UtcNow;
                        
                        _logger.LogInformation($"Updated game: {espnGame.Name}");
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Game sync completed. Processed {allEspnGames.Count} games.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during game sync");
            }
        }

        public async Task UpdateLiveScoresAsync()
        {
            try
            {
                _logger.LogInformation("Updating live scores...");

                // Get live/recent games from ESPN - all sports
                var nflGames = await _espnService.GetNflGamesAsync();
                var nbaGames = await _espnService.GetNbaGamesAsync();
                var mlbGames = await _espnService.GetMlbGamesAsync();
                var nhlGames = await _espnService.GetNhlGamesAsync();
                
                var allEspnGames = nflGames.Concat(nbaGames).Concat(mlbGames).Concat(nhlGames)
                    .Where(g => g.Status != "STATUS_SCHEDULED") // Only update non-scheduled games
                    .ToList();

                foreach (var espnGame in allEspnGames)
                {
                    var homeTeam = await _context.Teams
                        .FirstOrDefaultAsync(t => t.Name == espnGame.HomeTeam.Name && t.Sport == espnGame.Sport);
                    var awayTeam = await _context.Teams
                        .FirstOrDefaultAsync(t => t.Name == espnGame.AwayTeam.Name && t.Sport == espnGame.Sport);

                    if (homeTeam != null && awayTeam != null)
                    {
                        var game = await _context.Games
                            .FirstOrDefaultAsync(g => g.HomeTeamId == homeTeam.Id && 
                                                     g.AwayTeamId == awayTeam.Id && 
                                                     g.GameTime.Date == espnGame.Date.Date);

                        if (game != null)
                        {
                            game.Status = MapEspnStatus(espnGame.Status);
                            game.HomeScore = espnGame.HomeTeam.Score;
                            game.AwayScore = espnGame.AwayTeam.Score;
                            game.LastUpdated = DateTime.UtcNow;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Live scores updated for {allEspnGames.Count} games.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating live scores");
            }
        }

        public async Task SyncTeamsAsync()
        {
            try
            {
                _logger.LogInformation("Syncing teams from ESPN...");

                var nflTeams = await _espnService.GetNflTeamsAsync();
                var nbaTeams = await _espnService.GetNbaTeamsAsync();
                var mlbTeams = await _espnService.GetMlbTeamsAsync();
                var nhlTeams = await _espnService.GetNhlTeamsAsync();

                await SyncTeamsBySport(nflTeams, "NFL");
                await SyncTeamsBySport(nbaTeams, "NBA");
                await SyncTeamsBySport(mlbTeams, "MLB");
                await SyncTeamsBySport(nhlTeams, "NHL");

                await _context.SaveChangesAsync();
                _logger.LogInformation("Team sync completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during team sync");
            }
        }

        private async Task SyncTeamsBySport(List<EspnTeam> espnTeams, string sport)
        {
            foreach (var espnTeam in espnTeams)
            {
                var existingTeam = await _context.Teams
                    .FirstOrDefaultAsync(t => t.Name == espnTeam.Name && t.Sport == sport);

                if (existingTeam == null)
                {
                    var newTeam = new Team
                    {
                        Name = espnTeam.Name,
                        Abbreviation = espnTeam.Abbreviation,
                        City = espnTeam.Location,
                        Sport = sport,
                        LogoUrl = espnTeam.Logo,
                        IsActive = true
                    };

                    _context.Teams.Add(newTeam);
                    _logger.LogInformation($"Added new team: {espnTeam.Name}");
                }
                else
                {
                    // Update team info
                    existingTeam.Abbreviation = espnTeam.Abbreviation;
                    existingTeam.City = espnTeam.Location;
                    existingTeam.LogoUrl = espnTeam.Logo;
                }
            }
        }

        private async Task<Team> FindOrCreateTeamAsync(EspnTeam espnTeam, string sport)
        {
            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Name == espnTeam.Name && t.Sport == sport);

            if (team == null)
            {
                team = new Team
                {
                    Name = espnTeam.Name,
                    Abbreviation = espnTeam.Abbreviation,
                    City = espnTeam.Location,
                    Sport = sport,
                    LogoUrl = espnTeam.Logo,
                    IsActive = true
                };

                _context.Teams.Add(team);
                await _context.SaveChangesAsync(); // Save to get the ID
            }

            return team;
        }

        private string MapEspnStatus(string espnStatus)
        {
            return espnStatus.ToLower() switch
            {
                "status_scheduled" => "Scheduled",
                "status_in_progress" => "Live",
                "status_halftime" => "Halftime",
                "status_final" => "Final",
                "status_postponed" => "Postponed",
                "status_canceled" => "Cancelled",
                _ => "Scheduled"
            };
        }
    }
}