using System.Configuration;
using System.Data;
using System.Windows;
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

            // Register services
            services.AddSingleton<DiaryContext>();
            services.AddSingleton<DiaryRepository>();
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginWindow>();



            //ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            // Register services
            

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
