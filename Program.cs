using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Diplom.Data;
using Diplom.Syncing;
using Hangfire;
using Hangfire.PostgreSql;


var builder = WebApplication.CreateBuilder(args);

// Подключение к базе данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Настройка HttpClient
builder.Services.AddHttpClient();

// Настройка Hangfire с PostgreSQL хранилищем
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("HangfirePg"))
);

// Регистрация нужных сервисов
builder.Services.AddTransient<SynchronizationOrders>();
builder.Services.AddTransient<SynchronizationUsers>();
builder.Services.AddTransient<SynchronizationProductsAndCategories>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Проверяет издателя токена
            ValidateAudience = true, // Проверяет целевую аудиторию токена
            ValidateLifetime = true, // Проверяет срок действия токена
            ValidateIssuerSigningKey = true, // Проверяет подпись токена

            ValidIssuer = builder.Configuration["JwtSettings:Issuer"], // Указываем издателя
            ValidAudience = builder.Configuration["JwtSettings:Audience"], // Указываем аудиторию
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"])) // Подключаем секретный ключ
        };
    });

builder.Services.AddCors(options =>
   {
       options.AddDefaultPolicy(builder =>
       {
           builder.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
       });
   });

   


// Добавление Hangfire сервера для обработки задач
builder.Services.AddHangfireServer();

builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

// Настройка Hangfire до выполнения заданий
using (var scope = app.Services.CreateScope())
{
    var synchronizationOrders = scope.ServiceProvider.GetRequiredService<SynchronizationOrders>();
    synchronizationOrders.ConfigureJobs();
    var synchronizationUsers = scope.ServiceProvider.GetRequiredService<SynchronizationUsers>();
    synchronizationUsers.ConfigureJobs();
    var synchronizationProductsAndCategories = scope.ServiceProvider.GetRequiredService<SynchronizationProductsAndCategories>();
    synchronizationProductsAndCategories.ConfigureJobs();
}

// Если не в режиме разработки, настраиваем обработку ошибок
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

// Подключение панели управления Hangfire
app.UseHangfireDashboard("/hangfire");

app.Run();