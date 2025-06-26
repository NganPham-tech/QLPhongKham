using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore; // THÊM DÒNG NÀY
using QLPhongKham.Models;
using QLPhongKham.Services;
using QLPhongKham.Repositories;

using QLPhongKham.Data;
namespace QLPhongKham
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // Register existing repositories với interface mới
            builder.Services.AddScoped<IDoctorRepository, EFDoctorRepository>();
            builder.Services.AddScoped<IPatientRepository, EFPatientRepository>();
            builder.Services.AddScoped<IStaffRepository, EFStaffRepository>();            // Register new repositories
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
            builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
              // Register services
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IPatientLinkingService, PatientLinkingService>();
            builder.Services.AddScoped<IDoctorLinkingService, DoctorLinkingService>();
            builder.Services.AddScoped<IInvoiceService, InvoiceService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            // Add HttpContextAccessor for UserService
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Test database connection
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            Console.WriteLine("Testing database connection...");
            await TestDbConnection.TestConnection(connectionString!);

            // Seed roles and admin user
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    await RoleSeeder.SeedRolesAndAdminAsync(services);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the database.");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            
            // Route để phá vỡ vòng lặp chuyển hướng nếu có
            app.MapControllerRoute(
                name: "debug_route",
                pattern: "Admin/Debug/{action=BreakRedirectLoop}",
                defaults: new { area = "Admin", controller = "Debug" });

            // Admin area route - phải đặt trước default route
            app.MapControllerRoute(
                name: "admin_area",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}",
                constraints: new { area = "Admin" });

            // Admin root route
            app.MapControllerRoute(
                name: "admin_root",
                pattern: "Admin",
                defaults: new { area = "Admin", controller = "Home", action = "Index" });

            // Default routes
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");            app.MapRazorPages();

            app.Run();
        }
    }
}