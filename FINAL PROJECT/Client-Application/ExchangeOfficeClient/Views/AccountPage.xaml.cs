using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ExchangeOfficeClient.Services;

namespace ExchangeOfficeClient.Views;

public partial class AccountPage : Page
{
    public AccountPage()
    {
        InitializeComponent();
        TxtUsername.Text = Session.Username;
        TxtUserId.Text = $"User ID: {Session.UserId}";
        LoadBalances();
    }

    private void LoadBalances()
    {
        try
        {
            var svc = ServiceClientFactory.AccountService();
            BalancesGrid.ItemsSource = svc.GetBalances(Session.UserId);
        }
        catch (Exception ex)
        {
            TxtTopUpMsg.Text = $"Error loading balances: {ex.Message}";
            TxtTopUpMsg.Foreground = Brushes.Red;
            TxtTopUpMsg.Visibility = Visibility.Visible;
        }
    }

    private void BtnRefresh_Click(object sender, RoutedEventArgs e) => LoadBalances();

    private void BtnTopUp_Click(object sender, RoutedEventArgs e)
    {
        TxtTopUpMsg.Visibility = Visibility.Collapsed;

        if (!decimal.TryParse(TxtTopUp.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            TxtTopUpMsg.Text = "Enter a valid positive amount.";
            TxtTopUpMsg.Foreground = Brushes.Red;
            TxtTopUpMsg.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            var svc = ServiceClientFactory.AccountService();
            var result = svc.TopUpBalance(Session.UserId, amount);

            if (!result.Success)
            {
                TxtTopUpMsg.Text = result.Error;
                TxtTopUpMsg.Foreground = Brushes.Red;
            }
            else
            {
                TxtTopUpMsg.Text = $"Successfully added {amount:F2} PLN.";
                TxtTopUpMsg.Foreground = Brushes.Green;
                TxtTopUp.Clear();
                LoadBalances();
            }
            TxtTopUpMsg.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            TxtTopUpMsg.Text = $"Error: {ex.Message}";
            TxtTopUpMsg.Foreground = Brushes.Red;
            TxtTopUpMsg.Visibility = Visibility.Visible;
        }
    }

    private void BtnChangePwd_Click(object sender, RoutedEventArgs e)
    {
        TxtPwdMsg.Visibility = Visibility.Collapsed;
        var current = TxtCurrentPwd.Password;
        var newPwd = TxtNewPwd.Password;
        var confirm = TxtConfirmPwd.Password;

        if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(newPwd))
        {
            TxtPwdMsg.Text = "Please fill in all password fields.";
            TxtPwdMsg.Foreground = Brushes.Red;
            TxtPwdMsg.Visibility = Visibility.Visible;
            return;
        }
        if (newPwd.Length < 6)
        {
            TxtPwdMsg.Text = "New password must be at least 6 characters.";
            TxtPwdMsg.Foreground = Brushes.Red;
            TxtPwdMsg.Visibility = Visibility.Visible;
            return;
        }
        if (newPwd != confirm)
        {
            TxtPwdMsg.Text = "New passwords do not match.";
            TxtPwdMsg.Foreground = Brushes.Red;
            TxtPwdMsg.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            var svc = ServiceClientFactory.AccountService();
            var result = svc.ChangePassword(Session.UserId, current, newPwd);

            TxtPwdMsg.Text = result.Success ? "Password changed successfully." : result.Error;
            TxtPwdMsg.Foreground = result.Success ? Brushes.Green : Brushes.Red;
            TxtPwdMsg.Visibility = Visibility.Visible;

            if (result.Success)
            {
                TxtCurrentPwd.Clear();
                TxtNewPwd.Clear();
                TxtConfirmPwd.Clear();
            }
        }
        catch (Exception ex)
        {
            TxtPwdMsg.Text = $"Error: {ex.Message}";
            TxtPwdMsg.Foreground = Brushes.Red;
            TxtPwdMsg.Visibility = Visibility.Visible;
        }
    }
}
