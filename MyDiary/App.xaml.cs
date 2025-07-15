using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyDiary.Data;

namespace MyDiary
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static IConfiguration Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            Configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            var services = new ServiceCollection();
            services.Configure<Settings>(Configuration.GetSection("Settings"));
            services.AddSingleton(Configuration);

            // Define SQLite path
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = System.IO.Path.Combine(Environment.GetFolderPath(folder), "diary.db");
            var connectionString = String.IsNullOrWhiteSpace(Configuration.GetConnectionString("DiaryDb")) ? $"Data Source={path}" : Configuration.GetConnectionString("DiaryDb");

            services.AddDbContext<DiaryContext>(options =>
                options.UseSqlite(connectionString));

            // Register services
            //services.AddSingleton<DiaryContext>();
            services.AddSingleton<DiaryRepository>();

            services.AddTransient<MainWindow>();
            services.AddTransient<LoginWindow>();

            ServiceProvider = services.BuildServiceProvider();
            

            //var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            //mainWindow.Show();
            var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();


        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DiaryContext>();
            services.AddScoped<DiaryRepository>();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<LoginWindow>();
        }
    }

}
