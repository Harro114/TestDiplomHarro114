using System.Security.Claims;
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
public class DiscountsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExpController> _logger;

    public DiscountsController(ApplicationDbContext context, ILogger<ExpController> logger)
    {
        _logger = logger;

        try
        {
            _context = context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing ExpController.");
            throw;
        }
    }

    [HttpPost("buyPrimaryDiscount")]
    [Authorize]
    public async Task<ActionResult<UserDiscounts>> BuyPrimaryDiscount(
        [FromBody] BuyPrimaryDiscountDTO buyPrimaryDiscountDto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return BadRequest("Токен недействителен или не содержит необходимую информацию.");
            }

            var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.Id == buyPrimaryDiscountDto.DiscountId);
            var userExp =
                await _context.ExpUsersWallets.FirstOrDefaultAsync(a => a.AccountId == int.Parse(userId));
            if (discount == null || userExp == null)
            {
                return BadRequest("Not found AccountWallet or Discounts");
            }

            if (discount.isActive == false)
            {
                return BadRequest("Discount is not active");
            }

            if (discount.isPrimary == false)
            {
                return BadRequest("Discount is not primary");
            }

            if (discount.Amount <= userExp.ExpValue)
            {
                var newDiscountUser = new UserDiscounts
                {
                    AccountId = int.Parse(userId),
                    DiscountId = buyPrimaryDiscountDto.DiscountId
                };
                await _context.UserDiscounts.AddAsync(newDiscountUser);
                userExp.ExpValue = userExp.ExpValue - discount.Amount;
                var expChange = new ExpChanges
                {
                    AccountId = int.Parse(userId),
                    ExpUserId = userExp.Id,
                    Value = discount.Amount * -1,
                    CurrentBalance = userExp.ExpValue - discount.Amount,
                    Discription = $"Покупка скидки {discount.Name}"
                };
                await _context.ExpChanges.AddAsync(expChange);

                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(BuyPrimaryDiscount), new { newDiscountUser },
                    new { newDiscountUser });
            }

            return BadRequest("Error buying primary discount");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while buying primary discount");
            return BadRequest("Error buying primary discount");
        }
    }

    [HttpPost("CombiningDiscounts")]
    [Authorize]
    public async Task<ActionResult<UserDiscounts>> CombiningDiscounts(
        [FromBody] CombiningDiscountsDTO combiningDiscountsDto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var userExp =
                await _context.ExpUsersWallets.FirstOrDefaultAsync(a => a.AccountId == int.Parse(userId));
            if (userExp == null)
            {
                return BadRequest("Not found AccountWallet");
            }

            var discountExchange = await _context.ExchangeDiscounts.FirstOrDefaultAsync(ed =>
                (ed.DiscountExchangeOneId == combiningDiscountsDto.DiscountOneId
                 && ed.DiscountExchangeTwoId == combiningDiscountsDto.DiscountTwoId) ||
                (ed.DiscountExchangeOneId == combiningDiscountsDto.DiscountTwoId
                 && ed.DiscountExchangeTwoId == combiningDiscountsDto.DiscountOneId));
            if (discountExchange != null)
            {
                var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.Id == discountExchange.DiscountId);
                var oneDiscount = await _context.UserDiscounts.FirstOrDefaultAsync(d =>
                    d.DiscountId == combiningDiscountsDto.DiscountOneId && d.AccountId == int.Parse(userId));
                var twoDiscount = await _context.UserDiscounts.FirstOrDefaultAsync(d =>
                    d.DiscountId == combiningDiscountsDto.DiscountTwoId && d.AccountId == int.Parse(userId));
                if (oneDiscount == null || twoDiscount == null)
                {
                    return BadRequest("Not found Discount");
                }

                if (discount.Amount <= userExp.ExpValue)
                {
                    var newDiscountUser = new UserDiscounts
                    {
                        AccountId = int.Parse(userId),
                        DiscountId = discountExchange.DiscountId
                    };
                    await _context.UserDiscounts.AddAsync(newDiscountUser);
                    userExp.ExpValue = userExp.ExpValue - discount.Amount;
                    var expChange = new ExpChanges
                    {
                        AccountId = int.Parse(userId),
                        ExpUserId = userExp.Id,
                        Value = discount.Amount * -1,
                        CurrentBalance = userExp.ExpValue - discount.Amount,
                        Discription = $"Объединение скидок до {discount.Name}"
                    };
                    await _context.ExpChanges.AddAsync(expChange);
                    var oneDiscountHistory = new UserDiscountsHistory
                    {
                        Id = oneDiscount.Id,
                        AccountId = oneDiscount.AccountId,
                        DiscountId = oneDiscount.DiscountId,
                        DateAccruals = oneDiscount.DateAccruals
                    };
                    var twoDiscountHistory = new UserDiscountsHistory
                    {
                        Id = twoDiscount.Id,
                        AccountId = twoDiscount.AccountId,
                        DiscountId = twoDiscount.DiscountId,
                        DateAccruals = twoDiscount.DateAccruals
                    };
                    await _context.UserDiscountsHistory.AddAsync(oneDiscountHistory);
                    await _context.UserDiscountsHistory.AddAsync(twoDiscountHistory);
                    _context.UserDiscounts.Remove(oneDiscount);
                    _context.UserDiscounts.Remove(twoDiscount);
                    await _context.SaveChangesAsync();
                    return CreatedAtAction(nameof(CombiningDiscounts), new { newDiscountUser },
                        new { newDiscountUser });
                }
                else
                {
                    return BadRequest("Insufficient balance");
                }
            }

            return BadRequest("An error occurred while combining discounts");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while combining discounts");
            return BadRequest("An error occurred while combining discounts");
        }
    }

    [HttpPost("ActivatedDiscount")]
    public async Task<ActionResult<UserDiscountsActivated>> ActivatedDiscount(
        [FromBody] ActivatedDiscountDTO activatedDiscountDto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var usrDiscount = await _context.UserDiscounts.FirstOrDefaultAsync(d =>
                d.Id == activatedDiscountDto.Id && d.AccountId == int.Parse(userId));
            if (usrDiscount != null)
            {
                var usrDiscountActivated = new UserDiscountsActivated
                {
                    AccountId = usrDiscount.AccountId,
                    DiscountId = usrDiscount.DiscountId
                };
                var discountHistory = new UserDiscountsHistory
                {
                    Id = usrDiscount.Id,
                    AccountId = usrDiscount.AccountId,
                    DiscountId = usrDiscount.DiscountId,
                    DateAccruals = usrDiscount.DateAccruals
                };
                await _context.UserDiscountsActivated.AddAsync(usrDiscountActivated);
                await _context.UserDiscountsHistory.AddAsync(discountHistory);
                _context.UserDiscounts.Remove(usrDiscount);
                await _context.SaveChangesAsync();
                return Ok(usrDiscountActivated);
            }

            return BadRequest("An error occurred while activated discount");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while activated discount");
            return BadRequest("An error occurred while activated discount");
        }
    }

    [HttpGet("checkExchange/{discountId1}/{discountId2}")]
    public async Task<ActionResult<checkExchangeDTO>> CheckExchange(int discountId1, int discountId2)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var discount = await _context.ExchangeDiscounts.FirstOrDefaultAsync(ed =>
                (ed.DiscountExchangeOneId == discountId1 && ed.DiscountExchangeTwoId == discountId2) ||
                (ed.DiscountExchangeOneId == discountId2 && ed.DiscountExchangeTwoId == discountId1));
            if (discount == null)
            {
                return Ok(new checkExchangeDTO
                {
                    hasDiscount = false
                });
            }

            var actualeDiscount = await _context.Discounts.FirstOrDefaultAsync(d => d.Id == discount.DiscountId);
            return Ok(new checkExchangeDTO
            {
                hasDiscount = true,
                Discount = actualeDiscount
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest("Что-то пошло не так");
            throw;
        }
    }

    [HttpGet("getPrimaryDiscount")]
    public async Task<ActionResult<GetPrimaryDiscountDTO>> GetPrimaryDiscount()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var discounts = await _context.Discounts
                .Where(d => d.isPrimary == true && (d.EndDate < DateTime.UtcNow || d.EndDate == null)).Select(d =>
                    new Discounts
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        isActive = d.isActive,
                        DiscountSize = d.DiscountSize,
                        StartDate = d.StartDate,
                        EndDate = d.EndDate,
                        ProductsId = d.ProductsId,
                        CategoriesId = d.CategoriesId,
                        Amount = d.Amount,
                        isPrimary = d.isPrimary
                    }).ToListAsync();
            var getDto = discounts;
            return Ok(getDto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while getting primary discount");
            return BadRequest("Что-то пошло не так");
            throw;
        }
    }

    [HttpGet("getAllDiscountsUser")]
    public async Task<ActionResult<GetAllDiscountsUserDTO>> GetAllDiscountsUser()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var discountsNoneActivated = await _context.UserDiscounts.Where(ud => ud.AccountId == int.Parse(userId))
                .Join(_context.Discounts, ud => ud.DiscountId, d => d.Id, (ud, d) => new { ud, d }).Where(x =>
                    x.ud.AccountId == int.Parse(userId) && (x.d.EndDate < DateTime.UtcNow || x.d.EndDate == null))
                .Select(x => new DiscountDTO
                {
                    Id = x.ud.Id,
                    DiscountId = x.d.Id,
                    Name = x.d.Name,
                    Description = x.d.Description,
                    isActive = x.d.isActive,
                    DiscountSize = x.d.DiscountSize,
                    StartDate = x.d.StartDate,
                    EndDate = x.d.EndDate,
                    ProductsId = x.d.ProductsId,
                    CategoriesId = x.d.CategoriesId,
                    Amount = x.d.Amount,
                    isPrimary = x.d.isPrimary
                })
                .ToListAsync();
            var discountsActivated = await _context.UserDiscountsActivated.Where(ud => ud.AccountId == int.Parse(userId))
                .Join(_context.Discounts, ud => ud.DiscountId, d => d.Id, (ud, d) => new { ud, d }).Where(x =>
                    x.ud.AccountId == int.Parse(userId) && (x.d.EndDate < DateTime.UtcNow || x.d.EndDate == null))
                .Select(x => new DiscountDTO
                {
                    Id = x.ud.Id,
                    DiscountId = x.d.Id,
                    Name = x.d.Name,
                    Description = x.d.Description,
                    isActive = x.d.isActive,
                    DiscountSize = x.d.DiscountSize,
                    StartDate = x.d.StartDate,
                    EndDate = x.d.EndDate,
                    ProductsId = x.d.ProductsId,
                    CategoriesId = x.d.CategoriesId,
                    Amount = x.d.Amount,
                    isPrimary = x.d.isPrimary
                })
                .ToListAsync();
            var getDto = new GetAllDiscountsUserDTO()
            {
                Discount = new List<DiscountDTO>()
            };
            
            foreach (var dis in discountsNoneActivated)
            {
                getDto.Discount.Add(dis);
            }

            foreach (var dis in discountsActivated)
            {
                getDto.Discount.Add(dis);
            }
            return Ok(getDto);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}