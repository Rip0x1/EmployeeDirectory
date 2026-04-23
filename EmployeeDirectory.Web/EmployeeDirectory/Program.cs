using Microsoft.EntityFrameworkCore;
using EmployeeDirectory.Data;
using EmployeeDirectory.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using EmployeeDirectory.Models;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;

namespace EmployeeDirectory
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages();
            builder.Services.AddControllers();
            builder.Services.AddMvc();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
            });

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
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+\\абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
                options.Lockout.AllowedForNewUsers = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.Zero;
                options.Lockout.MaxFailedAccessAttempts = int.MaxValue;
                options.ClaimsIdentity.UserIdClaimType = "sub";
                options.ClaimsIdentity.UserNameClaimType = "name";
                options.ClaimsIdentity.RoleClaimType = "role";
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
                .Configure<ITicketStore>((options, ticketStore) =>
                {
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
                        ? CookieSecurePolicy.SameAsRequest 
                        : CookieSecurePolicy.Always;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.MaxAge = TimeSpan.FromHours(8);
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                    options.SessionStore = ticketStore;
                });

            var authBuilder = builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = NegotiateDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Login";
                options.SlidingExpiration = true;
                options.Cookie.MaxAge = TimeSpan.FromHours(8);
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
                options.Cookie.HttpOnly = true;
            })
            .AddNegotiate();
            
            builder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
                .Configure<ITicketStore>((options, ticketStore) =>
                {
                    options.SessionStore = ticketStore;
                });

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            builder.Services.AddScoped<IEmployeeService, EmployeeService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();
            builder.Services.AddScoped<IPositionService, PositionService>();
            builder.Services.AddScoped<IDepartmentEditorService, DepartmentEditorService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<ITicketStore, SessionTicketStore>();
            builder.Services.AddScoped<ILogService, LogService>();
            builder.Services.AddScoped<ILoginLogService, LoginLogService>();
            builder.Services.AddScoped<UserInitializationService>();
            builder.Services.AddScoped<DataSeederService>();
            builder.Services.AddScoped<QuestPdfService>();
            builder.Services.AddScoped<IExportService, ExportService>();
            builder.Services.AddScoped<ILdapDirectory, LdapDirectoryStub>();
            builder.Services.AddScoped<DatabaseInitializationService>();

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
                var context = services.GetRequiredService<ApplicationDbContext>();
                var logger = services.GetRequiredService<ILogger<Program>>();

                var databaseInitializer = services.GetRequiredService<DatabaseInitializationService>();
                var userInit = services.GetRequiredService<UserInitializationService>();
                var dataSeeder = services.GetRequiredService<DataSeederService>();

                int retries = 5;
                while (retries > 0)
                {
                    try
                    {
                        await databaseInitializer.EnsureDatabaseCreatedAsync();
                        await context.Database.EnsureCreatedAsync();
                        await userInit.InitializeAsync();

                        if (!await context.Roles.AnyAsync())
                        {
                            await dataSeeder.SeedDataAsync();
                        }

                        break;
                    }
                    catch (Exception ex)
                    {
                        retries--;
                        logger.LogWarning($"База данных еще не готова или произошла ошибка. Ожидание... (Осталось попыток: {retries}). Ошибка: {ex.Message}");

                        if (retries == 0)
                        {
                            logger.LogCritical(ex, "Не удалось инициализировать базу данных после 5 попыток.");
                            throw;
                        }

                        await Task.Delay(5000);
                    }
                }
            }

            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            
            app.Use(async (context, next) =>
            {
                if (!context.User.Identity?.IsAuthenticated == true)
                {
                    var authService = context.RequestServices.GetRequiredService<Microsoft.AspNetCore.Authentication.IAuthenticationService>();
                    var authResult = await authService.AuthenticateAsync(context, NegotiateDefaults.AuthenticationScheme);
                    if (authResult?.Succeeded == true && authResult.Principal != null)
                    {
                        context.User = authResult.Principal;
                    }
                }
                await next();
            });
            
            app.UseMiddleware<EmployeeDirectory.Middleware.AutoUserCreationMiddleware>();
            
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
