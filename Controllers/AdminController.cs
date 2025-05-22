using Diplom.Data;
using Diplom.Models;
using Diplom.Models.DTO;
using Hangfire.States;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Diplom.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExpController> _logger;

    public AdminController(ApplicationDbContext context, ILogger<ExpController> logger)
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

    [HttpGet("getAllUsers")]
    [Authorize]
    public async Task<ActionResult<GetAllUsersDTO>> GetAllUsers()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var users = await _context.Accounts
                .Join(_context.AccountRole, a => a.Id, ar => ar.AccountId, (a, ar) => new { a, ar })
                .Join(_context.Role, ar => ar.ar.RoleId, r => r.Id, (ar, r) => new { ar, r })
                .Select(x => new GetAllUsersDTO
                {
                    Id = x.ar.a.Id,
                    Username = x.ar.a.Username,
                    UserLastName = x.ar.a.UserLastName,
                    UserFirstName = x.ar.a.UserFirstName,
                    Sex = x.ar.a.Sex,
                    CreatedAt = x.ar.a.CreatedAt,
                    IsBlocked = x.ar.a.IsBlocked ?? false,
                    RoleId = x.ar.ar.RoleId,
                    RoleName = x.r.Name
                }).ToListAsync();
            return Ok(users);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while getting all users");
            return BadRequest("Непредвиденная ошибка" + e);
            throw;
        }
    }

    [HttpPost("blockedUser")]
    [Authorize]
    public async Task<ActionResult<Accounts>> BlockedUser(
        [FromBody] BlockedUserDTO blockedUserDto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var user = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == blockedUserDto.UserId);
            if (user == null)
            {
                return BadRequest("Пользователь не найден");
            }
            else if (user.IsBlocked == blockedUserDto.IsBlocked)
            {
                return BadRequest("У пользователя уже данный статус блокировки");
            }
            else
            {
                user.IsBlocked = blockedUserDto.IsBlocked;
                await _context.SaveChangesAsync();
                return Ok(user);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось заблокировать пользователя");
            return BadRequest("Не удалось заблокировать пользователя" + e);
            throw;
        }
    }

    [HttpGet("getUserHistory")]
    [Authorize]
    public async Task<ActionResult<ExpChanges>> GetUserHistory(
        [FromQuery] int accountId
    )
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var user = await _context.ExpChanges
                .Where(ec => ec.AccountId == accountId)
                .Select(ec => new ExpChanges
                {
                    Id = ec.Id,
                    AccountId = ec.AccountId,
                    ExpUserId = ec.ExpUserId,
                    Value = ec.Value,
                    CurrentBalance = ec.CurrentBalance,
                    CreatedAt = ec.CreatedAt,
                    Discription = ec.Discription
                })
                .ToListAsync();
            if (user == null)
            {
                return BadRequest("Информация по пользователю не найдена");
            }

            return Ok(user);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось заблокировать пользователя");
            return BadRequest("Возникла непредвиденная ошибка");
            throw;
        }
    }

    [HttpPost("changeRole")]
    [Authorize]
    public async Task<ActionResult<Accounts>> ChangeRole(
        [FromBody] ChangeRoleDTO changeRoleDto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var roleAc = await _context.AccountRole.FirstOrDefaultAsync(ar => ar.AccountId == changeRoleDto.AccountId);
            var role = await _context.Role.FirstOrDefaultAsync(r => r.Id == changeRoleDto.RoleId);
            if (roleAc == null)
            {
                return BadRequest("Данный пользователь не найден");
            }
            else if (role == null)
            {
                return BadRequest("Данной роли нет");
            }
            else if (roleAc.RoleId == changeRoleDto.RoleId)
            {
                return BadRequest("Данная роль уже выбрана");
            }
            else
            {
                roleAc.RoleId = changeRoleDto.RoleId;
                await _context.SaveChangesAsync();
                return Ok(roleAc);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось изменит роль пользователя");
            return BadRequest("Не удалось изменить роль пользователя" + e);
            throw;
        }
    }

    [HttpGet("getRoles")]
    [Authorize]
    public async Task<ActionResult<Roles>> GetRoles()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var roles = await _context.Role.ToListAsync();
            return Ok(roles);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось получить роли");
            return BadRequest("Непредвиденная ошибка" + e);
            throw;
        }
    }

    [HttpGet("getAllProducts")]
    [Authorize]
    public async Task<ActionResult<ProductsStore>> GetAllProducts()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var products = await _context.ProductsStore.ToListAsync();
            return Ok(products);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось вернуть список товаров");
            return BadRequest("Не удалось вернуть список товаров" + e);
            throw;
        }
    }

    [HttpGet("getAllCategories")]
    [Authorize]
    public async Task<ActionResult<CategoriesStore>> GetAllCategories()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var categories = await _context.CategoriesStore.ToListAsync();
            return Ok(categories);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось вернуть список категорий");
            return BadRequest("Не удалось вернуть список категорий" + e);
            throw;
        }
    }

    [HttpPost("createDiscount")]
    [Authorize]
    public async Task<ActionResult<Discounts>> CreatedDiscount([FromBody] Discounts discounts)
    {
        try
        {
            // Проверка пользователя (здесь будет дополнительная логика, если нужно)
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            // Проверяем, есть ли указанные продукты в базе данных
            if (discounts.ProductsId != null && discounts.ProductsId.Count > 0)
            {
                var productIds = discounts.ProductsId.Select(p => p.Id).ToList();
                var validProducts = await _context.ProductsStore
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                if (validProducts.Count != productIds.Count)
                {
                    return BadRequest("Некоторые из указанных продуктов не существуют в базе данных.");
                }

                // Добавляем только существующие продукты
                discounts.ProductsId = validProducts;
            }

            // Проверяем, существуют ли указанные категории в базе данных
            if (discounts.CategoriesId != null && discounts.CategoriesId.Count > 0)
            {
                var categoryIds = discounts.CategoriesId.Select(c => c.Id).ToList();
                var validCategories = await _context.CategoriesStore
                    .Where(c => categoryIds.Contains(c.Id))
                    .ToListAsync();

                if (validCategories.Count != categoryIds.Count)
                {
                    return BadRequest("Некоторые из указанных категорий не существуют в базе данных.");
                }

                // Добавляем только существующие категории
                discounts.CategoriesId = validCategories;
            }

            // Добавляем скидку
            await _context.Discounts.AddAsync(discounts);
            await _context.SaveChangesAsync();
            return Ok(discounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании скидки");
            return BadRequest("Не удалось создать скидку: " + ex.Message);
        }
    }


    [HttpGet("getAllDiscounts")]
    [Authorize]
    public async Task<ActionResult<Discounts>> GetAllDiscounts()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var discounts = await _context.Discounts.ToListAsync();
            return Ok(discounts);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось отправить все скидки");
            return BadRequest("Не удалось вернуть все скидки" + e);
            throw;
        }
    }

    [HttpPost("SwitchActivityDiscount")]
    [Authorize]
    public async Task<ActionResult<Discounts>> SwitchActivityDiscount(
        [FromBody] SwitchActivityDiscountDTO switchActivityDiscountDto
    )
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var discount =
                await _context.Discounts.FirstOrDefaultAsync(d => d.Id == switchActivityDiscountDto.DiscountId);
            if (discount == null)
            {
                return BadRequest("Скидка не найдена");
            }
            else if (discount.isActive == switchActivityDiscountDto.IsActive)
            {
                return BadRequest("Статус активности скидки уже проставлен");
            }

            discount.isActive = switchActivityDiscountDto.IsActive;
            await _context.SaveChangesAsync();
            return Ok(discount);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("getDiscount")]
    [Authorize]
    public async Task<ActionResult<Discounts>> GetDiscount(
        [FromQuery] int discountId)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var discount = await _context.Discounts
                .Include(d => d.ProductsId) // Загрузка связанных продуктов
                .Include(d => d.CategoriesId) // Загрузка связанных категорий
                .FirstOrDefaultAsync(d => d.Id == discountId);


            if (discount == null)
            {
                return BadRequest("Данная скидка не найдена");
            }

            return Ok(discount);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не получилось вернуть данные о скидке");
            return BadRequest("Не удалось получить данные о скидке" + e);
            throw;
        }
    }

    [HttpPost("deleteDiscount")]
    [Authorize]
    public async Task<ActionResult<Discounts>> DeleteDiscount(
        [FromBody] DeleteDiscountDTO deleteDiscountDto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.Id == deleteDiscountDto.DiscountId);
            if (discount == null)
            {
                return BadRequest("Данная скидка не найдена");
            }

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();
            return Ok(discount);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось удалить скидку");
            return BadRequest("Не удалость удалить скидку" + e);
            throw;
        }
    }

    [HttpPost("updateDiscount")]
    [Authorize]
    public async Task<ActionResult<Discounts>> UpdateDiscount([FromBody] Discounts updatedDiscount)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.Id == updatedDiscount.Id);
            if (discount == null)
            {
                return BadRequest("Данная скидка не найдена");
            }

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();

            _context.Discounts.Add(updatedDiscount);
            await _context.SaveChangesAsync();
            return Ok(updatedDiscount);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось обновить скидку");
            return BadRequest("Не удалось обновить скидку");
            throw;
        }
    }


    [HttpPost("chargeExp")]
    [Authorize]
    public async Task<ActionResult<ExpUsersWallets>> ChargeExp(
        [FromBody] ChargeExpDTO chargeExpDto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var wallet =
                await _context.ExpUsersWallets.FirstOrDefaultAsync(euw => euw.AccountId == chargeExpDto.AccountId);
            if (wallet == null)
            {
                return BadRequest("Нет пользователя");
            }

            wallet.ExpValue = wallet.ExpValue + chargeExpDto.Value;
            await _context.ExpChanges.AddAsync(new ExpChanges
            {
                AccountId = chargeExpDto.AccountId,
                ExpUserId = wallet.Id,
                Value = chargeExpDto.Value,
                CurrentBalance = wallet.ExpValue + chargeExpDto.Value,
                Discription = chargeExpDto.Discription ?? ""
            });
            await _context.SaveChangesAsync();
            return Ok(wallet);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось начислить валюту");
            return BadRequest("Не удалось начислить валюту" + e);
            throw;
        }
    }

    [HttpPost("chargeDiscount")]
    [Authorize]
    public async Task<ActionResult<UserDiscounts>> ChargeDiscount(
        [FromBody] ChargeDiscountDTO chargeDiscountDto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var discount = await _context.UserDiscounts.AddAsync(new UserDiscounts
            {
                AccountId = chargeDiscountDto.AccountId,
                DiscountId = chargeDiscountDto.DiscountId
            });
            await _context.SaveChangesAsync();
            return Ok(discount.Entity);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось начислить скидку");
            return BadRequest("Не удалось начислить скидку" + e);
            throw;
        }
    }

    [HttpGet("GetUserDiscounts")]
    [Authorize]
    public async Task<ActionResult<List<UserDiscounts>>> GetUserDiscounts()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var userDiscounts = await _context.UserDiscounts.ToListAsync();
            return Ok(userDiscounts);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось вернуть скидки пользователей");
            return BadRequest("Не удалось вернуть скидки пользователей");
            throw;
        }
    }

    [HttpGet("GetUsersDiscounts")]
    [Authorize]
    public async Task<ActionResult<GetUsersDiscountsDTO>> GetUsersDiscounts()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var userDiscounts = await _context.UserDiscounts
                .Join(_context.Accounts, ud => ud.AccountId, a => a.Id, (ud, a) => new { ud, a })
                .Join(_context.Discounts, ud => ud.ud.DiscountId, d => d.Id, (ud, d) => new { ud, d })
                .Select(x => new GetUsersDiscountsDTO
                {
                    Id = x.ud.ud.Id,
                    Username = x.ud.a.Username,
                    UserLastName = x.ud.a.UserLastName,
                    UserFirstName = x.ud.a.UserFirstName,
                    DiscountName = x.d.Name,
                    DateAccruals = x.ud.ud.DateAccruals
                })
                .OrderByDescending(d => d.DateAccruals)
                .ToListAsync();
            return Ok(userDiscounts);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось вернуть скидки пользователей");
            return BadRequest("Не удалось вернуть скидки пользователей");
            throw;
        }
    }

    [HttpGet("getUserDiscountsHistory")]
    [Authorize]
    public async Task<ActionResult<GetUserDiscountsHistoryDTO>> GetUserDiscountsHistory()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var userDiscounts = await _context.UserDiscountsHistory
                .Join(_context.Accounts, ud => ud.AccountId, a => a.Id, (ud, a) => new { ud, a })
                .Join(_context.Discounts, ud => ud.ud.DiscountId, d => d.Id, (ud, d) => new { ud, d })
                .Select(x => new GetUserDiscountsHistoryDTO
                {
                    Id = x.ud.ud.Id,
                    Username = x.ud.a.Username,
                    UserLastName = x.ud.a.UserLastName,
                    UserFirstName = x.ud.a.UserFirstName,
                    DiscountName = x.d.Name,
                    DateAccruals = x.ud.ud.DateAccruals,
                    DateDelete = x.ud.ud.DateDelete
                })
                .OrderByDescending(d => d.DateDelete)
                .ToListAsync();
            return Ok(userDiscounts);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось вернуть историю скидок пользователей");
            return BadRequest("Не удалось вернуть историю скидок пользователей");
            throw;
        }
    }

    [HttpGet("getUserDiscountsActivated")]
    [Authorize]
    public async Task<ActionResult<GetUserDiscountsActivatedDTO>> GetUserDiscountsActivated()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var userDiscounts = await _context.UserDiscountsActivated
                .Join(_context.Accounts, ud => ud.AccountId, a => a.Id, (ud, a) => new { ud, a })
                .Join(_context.Discounts, ud => ud.ud.DiscountId, d => d.Id, (ud, d) => new { ud, d })
                .Select(x => new GetUserDiscountsActivatedDTO
                {
                    Id = x.ud.ud.Id,
                    Username = x.ud.a.Username,
                    UserLastName = x.ud.a.UserLastName,
                    UserFirstName = x.ud.a.UserFirstName,
                    DiscountName = x.d.Name,
                    DateActivateDiscount = x.ud.ud.DateActivateDiscount,
                })
                .OrderByDescending(d => d.DateActivateDiscount)
                .ToListAsync();
            return Ok(userDiscounts);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось вернуть активированные скидки пользователей");
            return BadRequest("Не удалось вернуть активированные скидки пользователей");
            throw;
        }
    }

    [HttpGet("getUserDiscountsActivatedHistory")]
    [Authorize]
    public async Task<ActionResult<GetUserDiscountsActivatedHistoryDTO>> GetUserDiscountsActivatedHistory()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var userDiscounts = await _context.UserDiscountsActivatedHistory
                .Join(_context.Accounts, ud => ud.AccountId, a => a.Id, (ud, a) => new { ud, a })
                .Join(_context.Discounts, ud => ud.ud.DiscountId, d => d.Id, (ud, d) => new { ud, d })
                .Select(x => new GetUserDiscountsActivatedHistoryDTO
                {
                    Id = x.ud.ud.Id,
                    Username = x.ud.a.Username,
                    UserLastName = x.ud.a.UserLastName,
                    UserFirstName = x.ud.a.UserFirstName,
                    DiscountName = x.d.Name,
                    DateActivateDiscount = x.ud.ud.DateActivateDiscount,
                    DateDelete = x.ud.ud.DateDelete
                })
                .OrderByDescending(d => d.DateDelete)
                .ToListAsync();
            return Ok(userDiscounts);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось вернуть историю активированных скидок пользователей");
            return BadRequest("Не удалось вернуть историю активированных скидок пользователей");
            throw;
        }
    }

    [HttpGet("GetExpChanges")]
    [Authorize]
    public async Task<ActionResult<GetExpChangesDTO>> GetExpChanges()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var expChanges = await _context.ExpChanges
                .Join(_context.Accounts, ec => ec.AccountId, a => a.Id, (ec, a) => new { ec, a })
                .Select(x => new GetExpChangesDTO
                {
                    Id = x.ec.Id,
                    Username = x.a.Username,
                    UserLastName = x.a.UserLastName,
                    UserFirstName = x.a.UserFirstName,
                    Value = x.ec.Value,
                    Discription = x.ec.Discription,
                    CreatedAt = x.ec.CreatedAt,
                })
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
            return Ok(expChanges);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось получить инсторию изменений балансов");
            return BadRequest("Не удалось получить инсторию изменений балансов");
            throw;
        }
    }

    [HttpPost("createDiscountExchange")]
    [Authorize]
    public async Task<ActionResult<ExchangeDiscounts>> CreateDiscountExchange(
        [FromBody] ExchangeDiscountsDTO exchangeDiscountsDto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var mainDiscount =
                await _context.Discounts.FirstOrDefaultAsync(d => d.Id == exchangeDiscountsDto.DiscountId);
            if (mainDiscount == null || mainDiscount.isActive == false || mainDiscount.isPrimary == true)
            {
                return BadRequest("Скидка не найдена или не активна или первична");
            }
            var discountOne =
                await _context.Discounts.FirstOrDefaultAsync(d => d.Id == exchangeDiscountsDto.DiscountExchangeOneId);
            var discountTwo =
                await _context.Discounts.FirstOrDefaultAsync(d => d.Id == exchangeDiscountsDto.DiscountExchangeTwoId);
            if (discountOne == null || discountOne.isActive == false || discountTwo == null ||
                discountTwo.isActive == false)
            {
                return BadRequest("Привязываемые скидки не найдены или они неактивны");
            }
            var discount = new ExchangeDiscounts
            {
                DiscountId = exchangeDiscountsDto.DiscountId,
                DiscountExchangeOneId = exchangeDiscountsDto.DiscountExchangeOneId,
                DiscountExchangeTwoId = exchangeDiscountsDto.DiscountExchangeTwoId
            };
            await _context.ExchangeDiscounts.AddAsync(discount);
            await _context.SaveChangesAsync();
            return Ok(discount);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось создать объединение скидок");
            return BadRequest("Не удалось создать объединение скидок");
            throw;
        }
    }

    [HttpPost("updateDiscountExchange")]
    [Authorize]
    public async Task<ActionResult<ExchangeDiscounts>> UpdateDiscountExchange(
        [FromBody] ExchangeDiscountsDTO exchangeDiscountsDto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }

            var mainDiscount =
                await _context.Discounts.FirstOrDefaultAsync(d => d.Id == exchangeDiscountsDto.DiscountId);
            if (mainDiscount == null || mainDiscount.isActive == false || mainDiscount.isPrimary == true)
            {
                return BadRequest("Скидка не найдена или не активна или первична");
            }
            var discountOne =
                await _context.Discounts.FirstOrDefaultAsync(d => d.Id == exchangeDiscountsDto.DiscountExchangeOneId);
            var discountTwo =
                await _context.Discounts.FirstOrDefaultAsync(d => d.Id == exchangeDiscountsDto.DiscountExchangeTwoId);
            if (discountOne == null || discountOne.isActive == false || discountTwo == null ||
                discountTwo.isActive == false)
            {
                return BadRequest("Привязываемые скидки не найдены или они неактивны");
            }
            var discount = await _context.ExchangeDiscounts.FirstOrDefaultAsync(ed => ed.Id == exchangeDiscountsDto.Id);
            if (discount == null)
            {
                return BadRequest("Не удалось найти объединение");
            }
            discount.DiscountId = exchangeDiscountsDto.DiscountId;
            discount.DiscountExchangeOneId = exchangeDiscountsDto.DiscountExchangeOneId;
            discount.DiscountExchangeTwoId = exchangeDiscountsDto.DiscountExchangeTwoId;
            await _context.SaveChangesAsync();
            return Ok(discount);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось изменить объединение");
            return BadRequest("Не удалось изменить объединение");
            throw;
        }
    }

    [HttpGet("getExchangeDiscounts")]
    [Authorize]
    public async Task<ActionResult<GetExchangeDiscountsDTO>> GetExchangeDiscounts()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }
            
            var exchangeDiscounts = await _context.ExchangeDiscounts
                .Join(
                    _context.Discounts, 
                    ed => ed.DiscountId, // Соединяем по DiscountId
                    d => d.Id, 
                    (ed, d0) => new { ed, MainDiscountName = d0.Name } // Название основной скидки
                )
                .Join(
                    _context.Discounts, 
                    ed => ed.ed.DiscountExchangeOneId, // Соединяем по DiscountExchangeOneId
                    d => d.Id, 
                    (ed, d1) => new { ed, DiscountExchangeOneName = d1.Name } // Название первого обмена
                )
                .Join(
                    _context.Discounts, 
                    ed => ed.ed.ed.DiscountExchangeTwoId, // Соединяем по DiscountExchangeTwoId
                    d => d.Id, 
                    (ed, d2) => new GetExchangeDiscountsDTO
                    {
                        Id = ed.ed.ed.Id,
                        DiscountId = ed.ed.ed.DiscountId,
                        Name = ed.ed.MainDiscountName,
                        DiscountExchangeOneId = ed.ed.ed.DiscountExchangeOneId,
                        DiscountExchangeOneName = ed.DiscountExchangeOneName,
                        DiscountExchangeTwoId = ed.ed.ed.DiscountExchangeTwoId,
                        DiscountExchangeTwoName = d2.Name // Название второго обмена (актуальное)
                    }
                )
                .ToListAsync();

            return Ok(exchangeDiscounts);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    [HttpGet("getExchangeDiscount")]
    [Authorize]
    public async Task<ActionResult<GetExchangeDiscountsDTO>> GetExchangeDiscount(
        [FromQuery] int Id)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }
            
            var exchangeDiscounts = await _context.ExchangeDiscounts
                .Join(
                    _context.Discounts, 
                    ed => ed.DiscountId, // Соединяем по DiscountId
                    d => d.Id, 
                    (ed, d0) => new { ed, MainDiscountName = d0.Name } // Название основной скидки
                )
                .Join(
                    _context.Discounts, 
                    ed => ed.ed.DiscountExchangeOneId, // Соединяем по DiscountExchangeOneId
                    d => d.Id, 
                    (ed, d1) => new { ed, DiscountExchangeOneName = d1.Name } // Название первого обмена
                )
                .Join(
                    _context.Discounts, 
                    ed => ed.ed.ed.DiscountExchangeTwoId, // Соединяем по DiscountExchangeTwoId
                    d => d.Id, 
                    (ed, d2) => new GetExchangeDiscountsDTO
                    {
                        Id = ed.ed.ed.Id,
                        DiscountId = ed.ed.ed.DiscountId,
                        Name = ed.ed.MainDiscountName,
                        DiscountExchangeOneId = ed.ed.ed.DiscountExchangeOneId,
                        DiscountExchangeOneName = ed.DiscountExchangeOneName,
                        DiscountExchangeTwoId = ed.ed.ed.DiscountExchangeTwoId,
                        DiscountExchangeTwoName = d2.Name // Название второго обмена (актуальное)
                    }
                )
                .Where(ed => ed.Id == Id)
                .ToListAsync();

            return Ok(exchangeDiscounts);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось получить объединение");
            return BadRequest("Не получилось получить объединение");
            throw;
        }
    }

    [HttpGet("getDiscountsNoPrimary")]
    [Authorize]
    public async Task<ActionResult<Discounts>> GetDiscountsNoPrimary()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Токен недействителен или не содержит необходимую информацию.");
            }

            var roleUser = await _context.AccountRole.FirstOrDefaultAsync(r => r.AccountId == int.Parse(userId));
            if (roleUser.RoleId != 2 || roleUser.RoleId == null)
            {
                return Unauthorized("Пользователь не является администратором!");
            }
            
            var discounts = await _context.Discounts
                .Where(d => d.isPrimary == false)
                .ToListAsync();
            return Ok(discounts);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не получилось получить скидки");
            return BadRequest("Не получилось получить скидки");
            throw;
        }
    }
    
    
    
    
}