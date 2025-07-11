using System.Text.Json;
using WagerWatch.Models;

namespace WagerWatch.Services
{
    public interface IEspnService
    {
        Task<List<EspnGame>> GetNflGamesAsync();
        Task<List<EspnGame>> GetNbaGamesAsync();
        Task<List<EspnGame>> GetMlbGamesAsync();
        Task<List<EspnGame>> GetNhlGamesAsync();
        Task<List<EspnTeam>> GetNflTeamsAsync();
        Task<List<EspnTeam>> GetNbaTeamsAsync();
        Task<List<EspnTeam>> GetMlbTeamsAsync();
        Task<List<EspnTeam>> GetNhlTeamsAsync();
    }

    public class EspnService : IEspnService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EspnService> _logger;

        public EspnService(HttpClient httpClient, ILogger<EspnService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<EspnGame>> GetNflGamesAsync()
        {
            return await GetGamesFromEspnAsync("football", "nfl", "NFL");
        }

        public async Task<List<EspnGame>> GetNbaGamesAsync()
        {
            return await GetGamesFromEspnAsync("basketball", "nba", "NBA");
        }

        public async Task<List<EspnGame>> GetMlbGamesAsync()
        {
            return await GetGamesFromEspnAsync("baseball", "mlb", "MLB");
        }

        public async Task<List<EspnGame>> GetNhlGamesAsync()
        {
            return await GetGamesFromEspnAsync("hockey", "nhl", "NHL");
        }

        private DateTime ParseEspnDate(string dateString)
        {
            try
            {
                // Try to parse as DateTimeOffset first (handles timezone info properly)
                if (DateTimeOffset.TryParse(dateString, out var dateTimeOffset))
                {
                    // Convert to UTC for consistent storage
                    var utcDateTime = dateTimeOffset.UtcDateTime;
                    _logger.LogDebug($"Parsed ESPN date: {dateString} -> UTC: {utcDateTime:yyyy-MM-dd HH:mm:ss}");
                    return utcDateTime;
                }
                
                // Fallback to DateTime.Parse if DateTimeOffset fails
                var parsedDate = DateTime.Parse(dateString);
                
                // If DateTime.Parse doesn't specify Kind, assume it's UTC (ESPN default)
                if (parsedDate.Kind == DateTimeKind.Unspecified)
                {
                    // Treat unspecified as UTC (ESPN's typical format)
                    parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                    _logger.LogDebug($"Treated unspecified ESPN date as UTC: {dateString} -> {parsedDate:yyyy-MM-dd HH:mm:ss}");
                }
                else if (parsedDate.Kind == DateTimeKind.Local)
                {
                    // Convert local to UTC
                    parsedDate = parsedDate.ToUniversalTime();
                    _logger.LogDebug($"Converted local ESPN date to UTC: {dateString} -> {parsedDate:yyyy-MM-dd HH:mm:ss}");
                }
                
                return parsedDate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error parsing ESPN date: {dateString}");
                // Return current UTC time as fallback
                return DateTime.UtcNow;
            }
        }

        private async Task<List<EspnGame>> GetGamesFromEspnAsync(string sportType, string league, string sportName)
        {
            try
            {
                var url = $"http://site.api.espn.com/apis/site/v2/sports/{sportType}/{league}/scoreboard";
                _logger.LogInformation($"Fetching {sportName} games from: {url}");
                
                var response = await _httpClient.GetStringAsync(url);
                var espnData = JsonSerializer.Deserialize<EspnScoreboardResponse>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var games = espnData?.Events?.Select(e => {
                    var gameDate = ParseEspnDate(e.Date);
                    
                    return new EspnGame
                    {
                        Id = e.Id,
                        Name = e.Name,
                        ShortName = e.ShortName,
                        Date = gameDate, // Now properly parsed as UTC
                        Status = e.Status?.Type?.Name ?? "Scheduled",
                        HomeTeam = new EspnTeam
                        {
                            Id = e.Competitions?.FirstOrDefault()?.Competitors?.FirstOrDefault(c => c.HomeAway == "home")?.Team?.Id ?? "0",
                            Name = e.Competitions?.FirstOrDefault()?.Competitors?.FirstOrDefault(c => c.HomeAway == "home")?.Team?.DisplayName ?? "Unknown",
                            Abbreviation = e.Competitions?.FirstOrDefault()?.Competitors?.FirstOrDefault(c => c.HomeAway == "home")?.Team?.Abbreviation ?? "UNK",
                            Score = int.TryParse(e.Competitions?.FirstOrDefault()?.Competitors?.FirstOrDefault(c => c.HomeAway == "home")?.Score, out var homeScore) ? homeScore : null
                        },
                        AwayTeam = new EspnTeam
                        {
                            Id = e.Competitions?.FirstOrDefault()?.Competitors?.FirstOrDefault(c => c.HomeAway == "away")?.Team?.Id ?? "0",
                            Name = e.Competitions?.FirstOrDefault()?.Competitors?.FirstOrDefault(c => c.HomeAway == "away")?.Team?.DisplayName ?? "Unknown",
                            Abbreviation = e.Competitions?.FirstOrDefault()?.Competitors?.FirstOrDefault(c => c.HomeAway == "away")?.Team?.Abbreviation ?? "UNK",
                            Score = int.TryParse(e.Competitions?.FirstOrDefault()?.Competitors?.FirstOrDefault(c => c.HomeAway == "away")?.Score, out var awayScore) ? awayScore : null
                        },
                        Sport = sportName
                    };
                }).ToList() ?? new List<EspnGame>();

                _logger.LogInformation($"Retrieved {games.Count} {sportName} games");
                
                // Log sample game times for debugging
                if (games.Any())
                {
                    var sampleGame = games.First();
                    _logger.LogInformation($"Sample {sportName} game: {sampleGame.Name} at {sampleGame.Date:yyyy-MM-dd HH:mm:ss} UTC");
                }
                
                return games;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching {sportName} games from ESPN");
                return new List<EspnGame>();
            }
        }

        public async Task<List<EspnTeam>> GetNflTeamsAsync()
        {
            return await GetTeamsFromEspnAsync("football", "nfl");
        }

        public async Task<List<EspnTeam>> GetNbaTeamsAsync()
        {
            return await GetTeamsFromEspnAsync("basketball", "nba");
        }

        public async Task<List<EspnTeam>> GetMlbTeamsAsync()
        {
            return await GetTeamsFromEspnAsync("baseball", "mlb");
        }

        public async Task<List<EspnTeam>> GetNhlTeamsAsync()
        {
            return await GetTeamsFromEspnAsync("hockey", "nhl");
        }

        private async Task<List<EspnTeam>> GetTeamsFromEspnAsync(string sportType, string league)
        {
            try
            {
                var url = $"http://site.api.espn.com/apis/site/v2/sports/{sportType}/{league}/teams";
                var response = await _httpClient.GetStringAsync(url);
                var espnData = JsonSerializer.Deserialize<EspnTeamsResponse>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return espnData?.Sports?.FirstOrDefault()?.Leagues?.FirstOrDefault()?.Teams?.Select(t => new EspnTeam
                {
                    Id = t.Team?.Id ?? "0",
                    Name = t.Team?.DisplayName ?? "Unknown",
                    Abbreviation = t.Team?.Abbreviation ?? "UNK",
                    Location = t.Team?.Location ?? "",
                    Color = t.Team?.Color ?? "",
                    Logo = t.Team?.Logos?.FirstOrDefault()?.Href ?? ""
                }).ToList() ?? new List<EspnTeam>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching {league.ToUpper()} teams from ESPN");
                return new List<EspnTeam>();
            }
        }
    }

    // ESPN API response models
    public class EspnGame
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public EspnTeam HomeTeam { get; set; } = new();
        public EspnTeam AwayTeam { get; set; } = new();
        public string Sport { get; set; } = string.Empty;
    }

    public class EspnTeam
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Abbreviation { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public int? Score { get; set; }
    }

    // ESPN API JSON structure models
    public class EspnScoreboardResponse
    {
        public List<EspnEvent>? Events { get; set; }
    }

    public class EspnEvent
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public EspnStatus? Status { get; set; }
        public List<EspnCompetition>? Competitions { get; set; }
    }

    public class EspnStatus
    {
        public EspnStatusType? Type { get; set; }
    }

    public class EspnStatusType
    {
        public string Name { get; set; } = string.Empty;
    }

    public class EspnCompetition
    {
        public List<EspnCompetitor>? Competitors { get; set; }
    }

    public class EspnCompetitor
    {
        public string HomeAway { get; set; } = string.Empty;
        public string Score { get; set; } = string.Empty;
        public EspnTeamInfo? Team { get; set; }
    }

    public class EspnTeamInfo
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Abbreviation { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public List<EspnLogo>? Logos { get; set; }
    }

    public class EspnLogo
    {
        public string Href { get; set; } = string.Empty;
    }

    public class EspnTeamsResponse
    {
        public List<EspnSport>? Sports { get; set; }
    }

    public class EspnSport
    {
        public List<EspnLeague>? Leagues { get; set; }
    }

    public class EspnLeague
    {
        public List<EspnTeamWrapper>? Teams { get; set; }
    }

    public class EspnTeamWrapper
    {
        public EspnTeamInfo? Team { get; set; }
    }
}