using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Diplom.Data;
using Diplom.Models;
using Diplom.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


namespace Diplom.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userDto)
        {
            // Проверяем, существует ли пользователь
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == userDto.Username);
            if (account == null)
            {
                return Unauthorized("Пользователь не найден.");
            }

            // Проверяем совпадение пароля
            var passwordEntry = await _context.AccountPasswords.FirstOrDefaultAsync(p => p.AccountId == account.Id);
            if (passwordEntry == null || passwordEntry.PasswordHash != HashPassword(userDto.Password))
            {
                return Unauthorized("Неправильный пароль.");
            }

            // Генерируем JWT токен
            var token = GenerateJwtToken(account);
            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(Accounts account)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString())
            };

            // Замените ключ на строку длиной не менее 16 символов
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"] ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(256),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        // Новый метод для хэширования паролей
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        [HttpPost("create-account")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountPasswordDto dto)
        {
            // Проверяем, существует ли аккаунт с указанным AccountId
            var existingAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == dto.AccountId);
            if (existingAccount == null)
            {
                return NotFound(new { Message = "Аккаунт с указанным AccountId не найден." });
            }

            // Проверяем, существует ли уже запись в AccountPasswords для этого аккаунта
            var existingPasswordEntry = await _context.AccountPasswords.FirstOrDefaultAsync(p => p.AccountId == dto.AccountId);
            if (existingPasswordEntry != null)
            {
                return BadRequest(new { Message = "Пароль для этого аккаунта уже установлен." });
            }

            // Генерируем хэш пароля
            var hashedPassword = HashPassword(dto.Password);

            // Создаём запись в таблице AccountPasswords
            var accountPassword = new AccountPasswords
            {
                AccountId = dto.AccountId,
                PasswordHash = hashedPassword
            };

            await _context.AccountPasswords.AddAsync(accountPassword);
            await _context.SaveChangesAsync();

            // Возвращаем успешный результат
            return Ok(new
            {
                Message = "Пароль успешно создан и связан с аккаунтом.",
                AccountId = dto.AccountId
            });
        }


    }
}