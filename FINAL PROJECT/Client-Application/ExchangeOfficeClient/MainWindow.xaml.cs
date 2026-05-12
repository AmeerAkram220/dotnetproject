using System.Windows;
using ExchangeOfficeClient.Services;
using ExchangeOfficeClient.Views;

namespace ExchangeOfficeClient;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        NavigateTo(new LoginPage(this));
    }

    public void NavigateTo(System.Windows.Controls.Page page) =>
        MainFrame.Navigate(page);

    public void OnLoggedIn()
    {
        BtnLogout.Visibility = Visibility.Visible;
        NavigateTo(new AccountPage());
    }

    private void NavRates_Click(object sender, RoutedEventArgs e)
    {
        if (!Session.IsLoggedIn) return;
        NavigateTo(new ExchangeRatesPage());
    }

    private void NavAccount_Click(object sender, RoutedEventArgs e)
    {
        if (!Session.IsLoggedIn) return;
        NavigateTo(new AccountPage());
    }

    private void NavTrade_Click(object sender, RoutedEventArgs e)
    {
        if (!Session.IsLoggedIn) return;
        NavigateTo(new TradePage());
    }

    private void NavHistory_Click(object sender, RoutedEventArgs e)
    {
        if (!Session.IsLoggedIn) return;
        NavigateTo(new HistoryPage());
    }

    private void NavLogout_Click(object sender, RoutedEventArgs e)
    {
        Session.Clear();
        BtnLogout.Visibility = Visibility.Collapsed;
        NavigateTo(new LoginPage(this));
    }
}
