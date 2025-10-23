using Microsoft.EntityFrameworkCore;
using EmployeeDirectory.Data;
using EmployeeDirectory.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using EmployeeDirectory.Models;

namespace EmployeeDirectory
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages();
            builder.Services.AddControllers();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddAuthentication()
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });

            builder.Services.AddAuthorization();

            builder.Services.AddScoped<IEmployeeService, EmployeeService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();
            builder.Services.AddScoped<IPositionService, PositionService>();
            builder.Services.AddScoped<IDepartmentEditorService, DepartmentEditorService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ILogService, LogService>();
            builder.Services.AddScoped<UserInitializationService>();

            var app = builder.Build();
 
            app.UseHsts();
            app.UseHttpsRedirection();
            
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


            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userInit = scope.ServiceProvider.GetRequiredService<UserInitializationService>();
                try
                {
                    context.Database.Migrate();
                    await userInit.InitializeAsync();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database");
                }
            }

            app.Run();
        }
    }
}
