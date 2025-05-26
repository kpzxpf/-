using System.Windows;
using MedCentre.models;
using MedCentre.service;

namespace MedCentre;

public partial class RegisterWindow : Window
{
    UserService userService;
    public RegisterWindow()
    {
        InitializeComponent();
        userService = new UserService();
    }

    private async void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        if (password.Password != passwordRepet.Password)
        {
            error.Text = "Пароли не совпадают";
            error.Visibility = Visibility.Visible;
            
            return;
        }
        
        User newUser = new User(
            name.Text, login.Text, password.Password, email.Text);
        
        var (success, user, message) = 
            await userService.RegisterAsync(newUser);

        if (success)
        {
            MessageBox.Show(message, "Регистрация", MessageBoxButton.OK, MessageBoxImage.Information);
            UserSession.Instance.SignIn(user);
            new MainWindow().Show();
            this.Close();
        } else
        {
            error.Text = "Пароли не совпадают";
            error.Visibility = Visibility.Visible;
        }
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        var loginWindow = new LoginWindow();
        loginWindow.Show();
        this.Close();
    }
}