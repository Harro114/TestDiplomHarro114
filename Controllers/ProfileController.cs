using Diplom.Data;
using Diplom.Models;
using Diplom.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diplom.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger _logger;

    public ProfileController(ApplicationDbContext context, ILogger<ProfileController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        // Извлечение идентификатора пользователя из токена
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
        }

        // Получаем данные пользователя из базы данных
        var user = await _context.Accounts.FirstOrDefaultAsync(a =>
            int.Parse(userId) == a.Id); // Укажите тип ID пользователя, например, int
        if (user == null)
        {
            return NotFound("Пользователь не найден.");
        }

        // Возвращаем данные профиля
        var userProfileDto = new UserProfileDto
        {
            Id = user.Id,
            Name = user.Username,
            LastName = user.UserLastName,
            FirstName = user.UserFirstName
            // Добавьте другие поля, если нужно
        };

        return Ok(userProfileDto);
    }


    [HttpGet("expHistory")]
    public async Task<ActionResult<ExpHistoryUserDTO>> GetExphistoryUser()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var historyDto = new ExpHistoryUserDTO
            {
                Data = new List<DataExpHistoryDTO>()
            };

            var history = await _context.ExpChanges.Where(e => e.AccountId == int.Parse(userId))
                .OrderByDescending(e => e.CreatedAt).ToListAsync();
            foreach (var his in history)
            {
                historyDto.Data.Add(new DataExpHistoryDTO
                {
                    CreatedAt = his.CreatedAt,
                    Value = his.Value,
                    Discription = his.Discription,
                });
            }

            return Ok(historyDto);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("userDiscounts")]
    public async Task<ActionResult<getUserDiscountsDTO>> UserDiscounts()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var noneActivatedDiscount = await _context.UserDiscounts.Where(ud => ud.AccountId == int.Parse(userId))
                .Join(_context.Discounts, ud => ud.DiscountId, d => d.Id, (ud, d) => new
                {
                    Id = ud.Id,
                    Name = d.Name,
                    Description = d.Description,
                    DiscountSize = d.DiscountSize,
                    isActivated = false
                }).ToListAsync();
            var activatedDiscount = await _context.UserDiscountsActivated.Where(ud => ud.AccountId == int.Parse(userId))
                .Join(_context.Discounts, ud => ud.DiscountId, d => d.Id, (ud, d) => new
                {
                    Id = ud.Id,
                    Name = d.Name,
                    Description = d.Description,
                    DiscountSize = d.DiscountSize,
                    isActivated = true
                }).ToListAsync();
            if (!noneActivatedDiscount.Any() && !activatedDiscount.Any())
            {
                return NotFound("Скидки для данного пользователя не найдены.");
            }

            var result = new getUserDiscountsDTO
            {
                Discounts = new List<usrDiscountsDTO>()
            };
            foreach (var nad in noneActivatedDiscount)
            {
                result.Discounts.Add(new usrDiscountsDTO
                {
                    Id = nad.Id,
                    Name = nad.Name,
                    Description = nad.Description ?? "",
                    DiscountSize = nad.DiscountSize,
                    isActivated = nad.isActivated
                });
            }

            foreach (var ad in activatedDiscount)
            {
                result.Discounts.Add(new usrDiscountsDTO
                {
                    Id = ad.Id,
                    Name = ad.Name,
                    Description = ad.Description ?? "",
                    DiscountSize = ad.DiscountSize,
                    isActivated = ad.isActivated
                });
            }

            return Ok(result);
        }
        catch (Exception e)
        {
            return StatusCode(500, "Произошла ошибка при обработке вашего запроса." + e.Message);
        }
    }


    [HttpGet("checkRole")]
    [Authorize]
    public async Task<ActionResult<AccountRole>> CheckRole()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }
            var role = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            return Ok(role);
            
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось вернуть роль");
            return BadRequest("Не удалось вернуть роль");
            throw;
        }
    }

    [HttpGet("getExpCount")]
    [Authorize]
    public async Task<ActionResult<int>> GetExpCount()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }
            var wallet = await _context.ExpUsersWallets.FirstOrDefaultAsync(euw => euw.AccountId == int.Parse(userId));
            if (wallet == null)
            {
                return BadRequest("Кошелек не найден");
            }
            return Ok(wallet.ExpValue);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось вернуть баланс пользователя");
            return BadRequest("Не удалось вернуть баланс пользователя");
            throw;
        }
    }
    
}