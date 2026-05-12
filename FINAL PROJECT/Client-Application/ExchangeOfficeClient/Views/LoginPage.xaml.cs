using System.Windows;
using System.Windows.Controls;
using ExchangeOfficeClient.Services;

namespace ExchangeOfficeClient.Views;

public partial class LoginPage : Page
{
    private readonly MainWindow _main;

    public LoginPage(MainWindow main)
    {
        InitializeComponent();
        _main = main;
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        TxtError.Visibility = Visibility.Collapsed;
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
            var result = svc.Login(username, password);

            if (result.Error is not null)
            {
                TxtError.Text = result.Error;
                TxtError.Visibility = Visibility.Visible;
                return;
            }

            Session.UserId = result.Id;
            Session.Username = result.Username;
            _main.OnLoggedIn();
        }
        catch (Exception ex)
        {
            TxtError.Text = $"Service unavailable: {ex.Message}";
            TxtError.Visibility = Visibility.Visible;
        }
    }

    private void BtnGoRegister_Click(object sender, RoutedEventArgs e) =>
        _main.NavigateTo(new RegisterPage(_main));
}
