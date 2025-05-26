using System.Windows;
using System.Windows.Controls;
using MedCentre.models;
using MedCentre.security;
using MedCentre.service;

namespace MedCentre;

public partial class LoginWindow : Window
{
    private UserService userService;
    private CaptchaGenerator captchaGenerator;
    
    public LoginWindow()
    {
        InitializeComponent();
        userService = new UserService(); 
        captchaGenerator = new CaptchaGenerator();
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        if (captchaPanel.Visibility == Visibility.Visible)
        {
            if (string.IsNullOrEmpty(captcha.Text) || !captchaGenerator.ValidateCaptcha(captcha.Text))
            {
                error.Text = "Неверный код капчи. Попробуйте снова.";
                error.Visibility = Visibility.Visible;
                blockUser(10);
                RefreshCaptcha();
                return;
            }
        }

        var (success, user, message) = 
            await userService.AuthenticateAsync(login.Text, password.Password);

        if (success)
        {
            MessageBox.Show(message, "Авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
            UserSession.Instance.SignIn(user);
            new MainWindow().Show();
            this.Close();
        }
        else
        {
            error.Text = message;
            error.Visibility = Visibility.Visible;
            ShowCaptcha();
        }
    }
    
    private void ShowCaptcha()
    {
        RefreshCaptcha();
        captchaPanel.Visibility = Visibility.Visible;
    }
    
    private void RefreshCaptcha()
    {
        string captchaText = captchaGenerator.GenerateCaptchaText();
        Image captchaImage = captchaGenerator.GenerateCaptchaImage(captchaText);
        
        captchaImageContainer.Child = captchaImage;
        
        captcha.Text = string.Empty;
    }

    private async void blockUser(int seconds)
    {
        btnLogin.IsEnabled = false;
        btnGuest.IsEnabled = false;
        btnRegister.IsEnabled = false;
        password.IsEnabled = false;
        login.IsEnabled = false;
        
        await Task.Delay(seconds * 1000);
        
        btnLogin.IsEnabled = true;
        btnGuest.IsEnabled = true;
        btnRegister.IsEnabled = true;
        password.IsEnabled = true;
        login.IsEnabled = true;
    }

    private void BtnRegister_Click(object sender, RoutedEventArgs e)
    { 
        new RegisterWindow().Show();
        this.Close();
    }

    private void BtnGuest_Click(object sender, RoutedEventArgs e)
    {
        new MainWindow().Show();
        this.Close();
    }
}