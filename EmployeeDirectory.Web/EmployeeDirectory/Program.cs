using Microsoft.EntityFrameworkCore;
using EmployeeDirectory.Data;
using EmployeeDirectory.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using EmployeeDirectory.Models;
using Microsoft.AspNetCore.Server.IISIntegration;

namespace EmployeeDirectory
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(5050);

                options.ListenLocalhost(5051, listenOptions =>
                {
                    listenOptions.UseHttps();
                });
            });

            builder.Services.AddRazorPages();
            builder.Services.AddControllers();
            builder.Services.AddMvc();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                x => x.MigrationsAssembly("EmployeeDirectory")
                ));

            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = false;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
                options.Lockout.AllowedForNewUsers = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.Zero;
                options.Lockout.MaxFailedAccessAttempts = int.MaxValue;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
                options.Cookie.HttpOnly = true;
            });

            builder.Services.AddAuthentication(IISDefaults.AuthenticationScheme);

            builder.Services.AddAuthorization();

            builder.Services.AddScoped<IEmployeeService, EmployeeService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();
            builder.Services.AddScoped<IPositionService, PositionService>();
            builder.Services.AddScoped<IDepartmentEditorService, DepartmentEditorService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ILogService, LogService>();
            builder.Services.AddScoped<ILoginLogService, LoginLogService>();
            builder.Services.AddScoped<UserInitializationService>();
            builder.Services.AddScoped<DataSeederService>();
            builder.Services.AddScoped<QuestPdfService>();
            builder.Services.AddScoped<IExportService, ExportService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    if (app.Environment.IsDevelopment())
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache");
                    }
                    else
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
                    }
                }
            });

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("Permissions-Policy", "attribution-reporting=()");
                await next();
            });

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userInit = services.GetRequiredService<UserInitializationService>();
                var dataSeeder = services.GetRequiredService<DataSeederService>();

                int maxRetries = 5;
                int delayInMilliseconds = 3000;

                for (int retry = 0; retry < maxRetries; retry++)
                {
                    try
                    {
                        logger.LogInformation($"Попытка подключения к БД и применения миграций (Попытка {retry + 1} из {maxRetries})...");

                        await context.Database.MigrateAsync();

                        await userInit.InitializeAsync();
                        await dataSeeder.SeedDataAsync();

                        logger.LogInformation("База данных успешно создана и инициализирована.");
                        break; 
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning($"База данных еще не готова. Ожидание... (Осталось попыток: {maxRetries - retry - 1})");

                        if (retry == maxRetries - 1)
                        {
                            logger.LogError(ex, "Критическая ошибка: не удалось инициализировать базу данных после нескольких попыток.");
                            throw; 
                        }

                        await Task.Delay(delayInMilliseconds);
                    }
                }
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapControllers();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}