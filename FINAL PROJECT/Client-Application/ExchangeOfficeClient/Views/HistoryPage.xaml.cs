using System.Windows;
using System.Windows.Controls;
using ExchangeOfficeClient.Services;

namespace ExchangeOfficeClient.Views;

public partial class HistoryPage : Page
{
    public HistoryPage()
    {
        InitializeComponent();
        LoadHistory();
    }

    private void LoadHistory()
    {
        try
        {
            var svc = ServiceClientFactory.TransactionService();
            HistoryGrid.ItemsSource = svc.GetTransactionHistory(Session.UserId);
        }
        catch (Exception ex)
        {
            TxtError.Text = $"Failed to load history: {ex.Message}";
            TxtError.Visibility = Visibility.Visible;
        }
    }
}
