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
        TxtTitle.Text = $"My Account — {Session.Username}";
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
}
