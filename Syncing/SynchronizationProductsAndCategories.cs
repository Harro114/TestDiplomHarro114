using Diplom.Data;
using Diplom.Models;
using Diplom.Models.DTO;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace Diplom.Syncing;

public class SynchronizationProductsAndCategories
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<SynchronizationOrders> _logger;
    private readonly IRecurringJobManager _recurringJobManager;

    public SynchronizationProductsAndCategories(ApplicationDbContext context, IHttpClientFactory httpClientFactory,
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
        _recurringJobManager.AddOrUpdate<SynchronizationProductsAndCategories>(
            "SyncProducts",
            x => x.SyncProducts(),
            _config["SyncProducts:cron"],
            TimeZoneInfo.Local
        );
        
        _recurringJobManager.AddOrUpdate<SynchronizationProductsAndCategories>(
            "SyncCategories",
            x => x.SyncCategories(),
            _config["SyncCategories:cron"],
            TimeZoneInfo.Local
            );
    }

    public async Task SyncProducts()
    {
        try
        {
            _logger.LogInformation("Запущена синхронизация товаров {Time}", DateTime.UtcNow);

            var baseAddress = _config["HttpClient:magazin_api"];
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseAddress);

            var response = await client.GetAsync("/sync_products");

            var productsResponse = await response.Content.ReadFromJsonAsync<ProductsResponseDTO>();

            var products = productsResponse?.product;

            _logger.LogInformation("Количество товаров {Count}", products?.Count);

            if (products is null || !products.Any()) return;


            var existingProduct = await _context.ProductsStore.AsNoTracking().ToListAsync();

            foreach (var product in products)
            {
                if (existingProduct.Any(u => u.Name == product.name)) continue;
                
                await _context.ProductsStore.AddAsync(new ProductsStore
                {
                    Name = product.name,
                    isActive = product.is_active
                });

            }

            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Возникла ошибка синхронизации товаров");
            throw;
        }
    }
    
    public async Task SyncCategories()
    {
        try
        {
            _logger.LogInformation("Запущена синхронизация категорий {Time}", DateTime.UtcNow);

            var baseAddress = _config["HttpClient:magazin_api"];
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseAddress);

            var response = await client.GetAsync("/sync_categories");

            var categoriesResponse = await response.Content.ReadFromJsonAsync<CategoriesResponseDTO>();

            var categories = categoriesResponse?.categorie;

            _logger.LogInformation("Количество категорий: {Count}", categories?.Count);

            if (categories is null || !categories.Any()) return;


            var existingCategory = await _context.ProductsStore.AsNoTracking().ToListAsync();

            foreach (var categorie in categories)
            {
                if (existingCategory.Any(u => u.Name == categorie.name)) continue;
                
                await _context.CategoriesStore.AddAsync(new CategoriesStore
                {
                    Name = categorie.name,
                    isActive = categorie.is_active
                });

            }

            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Возникла ошибка синхронизации категорий");
            throw;
        }
    }
}