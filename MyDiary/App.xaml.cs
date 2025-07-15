using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyDiary.Data;

namespace MyDiary
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // Define SQLite path
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = System.IO.Path.Combine(Environment.GetFolderPath(folder), "diary.db");

            services.AddDbContext<DiaryContext>(options =>
            options.UseSqlite($"Data Source={path}"));

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
