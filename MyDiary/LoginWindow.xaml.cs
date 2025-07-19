using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using MyDiary.Data;

namespace MyDiary
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly DiaryRepository _diaryRepository;

        // name:        abbas
        // password:    england

        public LoginWindow(DiaryRepository diaryRepository)
        {
            InitializeComponent();

            _diaryRepository = diaryRepository;
        }

        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            string? storedPasswordHash = await _diaryRepository.GetUserPasswordHash(usernameTextBox.Text);

            if(storedPasswordHash == null || !PasswordHasher.VerifyPassword(passwordBox.Password, storedPasswordHash))
            {
                MessageBox.Show("Invalid Details", "Unlock Failed!", MessageBoxButton.OK);
            }
            else
            {
                var mainWindow = App.ServiceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();

                this.Close();
            }
        }
    }
}
