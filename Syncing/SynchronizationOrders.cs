using Diplom.Data;
using Diplom.Models;
using Diplom.Models.DTO;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace Diplom.Syncing;

public class SynchronizationOrders
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<SynchronizationOrders> _logger;
    private readonly IRecurringJobManager _recurringJobManager;

    public SynchronizationOrders(ApplicationDbContext context, IHttpClientFactory httpClientFactory,
        IConfiguration config, ILogger<SynchronizationOrders> logger, IRecurringJobManager recurringJobManager)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
        _recurringJobManager = recurringJobManager;
    }

    public void ConfigureJobs()
    {
        // Настройка cron для синхронизации заказов каждую ночь в 2:00
        _recurringJobManager.AddOrUpdate(
                "sync-orders-job", // Идентификатор задания
                () => SyncOrders(), // Метод для синхронизации заказов
                _config["SyncOrders:cron"], // Cron выражение: каждую ночь в 2:00 UTC
                TimeZoneInfo.Local); // Локальная временная зона
        // Настройка cron для расчёта EXP каждую ночь в 3:00
        _recurringJobManager.AddOrUpdate(
            "calculate-exp-job", // Идентификатор задания
            () => CalculateExp(), // Метод для вычисления EXP
            _config["SyncOrders:cron"], // Cron выражение: каждую ночь в 3:00 UTC
            TimeZoneInfo.Local); // Локальная временная зона
    }


    public async Task SyncOrders()
    {
        try
        {
            _logger.LogInformation("Запущена синхронизация товаров в {Time}", DateTime.UtcNow);
            var lastDate = await _context.Config.FirstOrDefaultAsync(c => c.Name == "LastDateOrder");
            
            var lastDateOrder = lastDate?.ValueDate ?? new DateTime(2000, 4, 7);
            
            // Получаем базовый адрес из конфигурации
            var baseAddress = _config["HttpClient:magazin_api"];

            // Создаем клиента и задаем BaseAddress
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseAddress);
            
            var response = await client.GetAsync($"/sync_orders?last_sync_date={lastDateOrder.ToString("yyyy-MM-dd HH:mm:ss")}");

            var ordersResponse = await response.Content.ReadFromJsonAsync<OrdersResponseDTO>();
            
            var orders = ordersResponse?.order;  
            Console.WriteLine(orders);

            _logger.LogInformation("Received {Count} orders", orders?.Count);
            if (orders is null || !orders.Any()) return;

            foreach (var order in orders)
            {
                // Добавление заказа в БД
                _context.Orders.Add(new Orders
                {
                    AccountId = order.account_id,
                    Amounts = order.amount,
                    DateLastOrder = order.order_date.ToUniversalTime()
                    
                });
            }
            
            await _context.SaveChangesAsync();
            var newLastDate = await _context.Orders.MaxAsync(o => o.DateLastOrder);
            var updateConfig = await _context.Config.SingleOrDefaultAsync(a => a.Name == "LastDateOrder");
            if (updateConfig != null) updateConfig.ValueDate = newLastDate;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Order sync completed and saved to DB");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing orders");
            throw;
        }
    }

    public async Task CalculateExp()
    {
        try
        {
            _logger.LogInformation("Starting EXP calculation at {Time}", DateTime.UtcNow);
            var curToExp = await _context.Config.FirstOrDefaultAsync(c => c.Name == "rublesToExp");
            var currencyToExp = curToExp?.ValueFloat ?? 1f;

            var syncedOrders = await _context.Orders.ToListAsync();
            foreach (var order in syncedOrders)
            {
                var exp = (int)Math.Round(order.Amounts * currencyToExp);
                var wallet = await _context.ExpUsersWallets.FirstOrDefaultAsync(a => a.AccountId == order.AccountId);
                var balance = 0;
                if (wallet != null)
                {
                    balance = wallet.ExpValue + exp;
                    wallet.ExpValue += exp;
                }


                _context.ExpChanges.Add(new ExpChanges
                {
                    AccountId = order.AccountId,
                    ExpUserId = wallet.Id,
                    Value = exp,
                    CurrentBalance = balance,
                    CreatedAt = order.DateLastOrder,
                    Discription = "Начисление Exp за покупки в магазине"
                });
            }

            var deletOrders = _context.Orders;
            _context.RemoveRange(deletOrders);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Exp calculation completed and saved to DB");
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating exp");
            throw;
        }
    }
}