using System.Windows;
using System.Windows.Controls;
using ExchangeOfficeClient.Services;

namespace ExchangeOfficeClient.Views;

public partial class RegisterPage : Page
{
    private readonly MainWindow _main;

    public RegisterPage(MainWindow main)
    {
        InitializeComponent();
        _main = main;
    }

    private void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        TxtError.Visibility = Visibility.Collapsed;
        TxtSuccess.Visibility = Visibility.Collapsed;
        var username = TxtUsername.Text.Trim();
        var password = TxtPassword.Password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            TxtError.Text = "Please enter username and password.";
            TxtError.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            var svc = ServiceClientFactory.AccountService();
            var result = svc.Register(username, password);

            if (result.Error is not null)
            {
                TxtError.Text = result.Error;
                TxtError.Visibility = Visibility.Visible;
                return;
            }

            TxtSuccess.Text = "Account created! Redirecting to login...";
            TxtSuccess.Visibility = Visibility.Visible;

            Task.Delay(1500).ContinueWith(_ =>
                Dispatcher.Invoke(() => _main.NavigateTo(new LoginPage(_main))));
        }
        catch (Exception ex)
        {
            TxtError.Text = $"Service unavailable: {ex.Message}";
            TxtError.Visibility = Visibility.Visible;
        }
    }

    private void BtnGoLogin_Click(object sender, RoutedEventArgs e) =>
        _main.NavigateTo(new LoginPage(_main));
}
