using Microsoft.AspNetCore.Mvc;
using WagerWatch.Services;

namespace WagerWatch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EspnController : ControllerBase
    {
        private readonly IEspnService _espnService;

        public EspnController(IEspnService espnService)
        {
            _espnService = espnService;
        }

        // GET: api/espn/nfl/games
        [HttpGet("nfl/games")]
        public async Task<ActionResult> GetNflGames()
        {
            try
            {
                var games = await _espnService.GetNflGamesAsync();
                return Ok(new
                {
                    message = "Live NFL games from ESPN",
                    count = games.Count,
                    games = games.Select(g => new
                    {
                        id = g.Id,
                        matchup = g.Name,
                        shortName = g.ShortName,
                        date = g.Date,
                        status = g.Status,
                        homeTeam = new
                        {
                            name = g.HomeTeam.Name,
                            abbreviation = g.HomeTeam.Abbreviation,
                            score = g.HomeTeam.Score
                        },
                        awayTeam = new
                        {
                            name = g.AwayTeam.Name,
                            abbreviation = g.AwayTeam.Abbreviation,
                            score = g.AwayTeam.Score
                        },
                        sport = g.Sport
                    })
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to fetch NFL games", details = ex.Message });
            }
        }

        // GET: api/espn/nba/games
        [HttpGet("nba/games")]
        public async Task<ActionResult> GetNbaGames()
        {
            try
            {
                var games = await _espnService.GetNbaGamesAsync();
                return Ok(new
                {
                    message = "Live NBA games from ESPN",
                    count = games.Count,
                    games = games.Select(g => new
                    {
                        id = g.Id,
                        matchup = g.Name,
                        shortName = g.ShortName,
                        date = g.Date,
                        status = g.Status,
                        homeTeam = new
                        {
                            name = g.HomeTeam.Name,
                            abbreviation = g.HomeTeam.Abbreviation,
                            score = g.HomeTeam.Score
                        },
                        awayTeam = new
                        {
                            name = g.AwayTeam.Name,
                            abbreviation = g.AwayTeam.Abbreviation,
                            score = g.AwayTeam.Score
                        },
                        sport = g.Sport
                    })
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to fetch NBA games", details = ex.Message });
            }
        }

        // GET: api/espn/nfl/teams
        [HttpGet("nfl/teams")]
        public async Task<ActionResult> GetNflTeams()
        {
            try
            {
                var teams = await _espnService.GetNflTeamsAsync();
                return Ok(new
                {
                    message = "NFL teams from ESPN",
                    count = teams.Count,
                    teams = teams.Select(t => new
                    {
                        id = t.Id,
                        name = t.Name,
                        abbreviation = t.Abbreviation,
                        location = t.Location,
                        color = t.Color,
                        logo = t.Logo
                    })
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to fetch NFL teams", details = ex.Message });
            }
        }

        // GET: api/espn/nba/teams
        [HttpGet("nba/teams")]
        public async Task<ActionResult> GetNbaTeams()
        {
            try
            {
                var teams = await _espnService.GetNbaTeamsAsync();
                return Ok(new
                {
                    message = "NBA teams from ESPN",
                    count = teams.Count,
                    teams = teams.Select(t => new
                    {
                        id = t.Id,
                        name = t.Name,
                        abbreviation = t.Abbreviation,
                        location = t.Location,
                        color = t.Color,
                        logo = t.Logo
                    })
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to fetch NBA teams", details = ex.Message });
            }
        }

        // GET: api/espn/test
        [HttpGet("test")]
        public ActionResult TestEndpoint()
        {
            return Ok(new
            {
                message = "ESPN service is working!",
                timestamp = DateTime.UtcNow,
                availableEndpoints = new[]
                {
                    "GET /api/espn/nfl/games - Live NFL games",
                    "GET /api/espn/nba/games - Live NBA games", 
                    "GET /api/espn/nfl/teams - All NFL teams",
                    "GET /api/espn/nba/teams - All NBA teams"
                }
            });
        }
    }
}