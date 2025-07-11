using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;
using WagerWatch.Data;
using WagerWatch.Models;
using WagerWatch.Services;

namespace WagerWatch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly WagerWatchDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ITimeZoneService _timeZoneService;

        public AuthController(WagerWatchDbContext context, IConfiguration configuration, ITimeZoneService timeZoneService)
        {
            _context = context;
            _configuration = configuration;
            _timeZoneService = timeZoneService;
        }

        // GET: api/auth/timezones - Get available timezones
        [HttpGet("timezones")]
        public ActionResult GetAvailableTimeZones()
        {
            var timezones = _timeZoneService.GetAvailableTimeZones();
            return Ok(new
            {
                message = "Available timezones for registration",
                timezones = timezones
            });
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            // Check if user exists
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest("User with this email already exists");
            }

            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return BadRequest("Username is already taken");
            }

            // Validate timezone
            var availableTimezones = _timeZoneService.GetAvailableTimeZones();
            if (!availableTimezones.Any(tz => tz.Id == registerDto.TimeZone))
            {
                return BadRequest($"Invalid timezone: {registerDto.TimeZone}. Use GET /api/auth/timezones to see available options.");
            }

            // Hash password
            var passwordHash = HashPassword(registerDto.Password);

            // Create user with timezone
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                TimeZone = registerDto.TimeZone,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsPremium = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate JWT token with timezone
            var token = GenerateJwtToken(user);

            var response = new AuthResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    IsPremium = user.IsPremium,
                    CreatedAt = user.CreatedAt,
                    TimeZone = user.TimeZone
                },
                Expires = DateTime.UtcNow.AddDays(7)
            };

            return Ok(response);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            // Find user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password");
            }

            if (!user.IsActive)
            {
                return Unauthorized("Account is deactivated");
            }

            // Generate JWT token with timezone
            var token = GenerateJwtToken(user);

            var response = new AuthResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    IsPremium = user.IsPremium,
                    CreatedAt = user.CreatedAt,
                    TimeZone = user.TimeZone
                },
                Expires = DateTime.UtcNow.AddDays(7)
            };

            return Ok(response);
        }

        // GET: api/auth/me
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsPremium = user.IsPremium,
                CreatedAt = user.CreatedAt,
                TimeZone = user.TimeZone
            });
        }

        // PUT: api/auth/timezone - Update user's timezone
        [HttpPut("timezone")]
        public async Task<IActionResult> UpdateTimeZone([FromBody] UpdateTimeZoneRequest request)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized();
            }

            // Validate timezone
            var availableTimezones = _timeZoneService.GetAvailableTimeZones();
            if (!availableTimezones.Any(tz => tz.Id == request.TimeZone))
            {
                return BadRequest($"Invalid timezone: {request.TimeZone}. Use GET /api/auth/timezones to see available options.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.TimeZone = request.TimeZone;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Timezone updated successfully",
                newTimeZone = request.TimeZone,
                userTime = _timeZoneService.ConvertToUserTimeZone(DateTime.UtcNow, request.TimeZone).ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("WagerWatch-Secret-Key-That-Is-At-Least-32-Characters-Long!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("IsPremium", user.IsPremium.ToString()),
                new Claim("TimeZone", user.TimeZone) // Include timezone in JWT
            };

            var token = new JwtSecurityToken(
                issuer: "WagerWatch",
                audience: "WagerWatch",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "WagerWatch-Salt"));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var computedHash = HashPassword(password);
            return computedHash == hash;
        }
    }

    public class UpdateTimeZoneRequest
    {
        [Required, StringLength(50)]
        public string TimeZone { get; set; } = string.Empty;
    }
}