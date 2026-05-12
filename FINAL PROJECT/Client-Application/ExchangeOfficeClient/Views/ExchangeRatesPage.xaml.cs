using System.Windows;
using System.Windows.Controls;
using ExchangeOfficeClient.Services;

namespace ExchangeOfficeClient.Views;

public partial class ExchangeRatesPage : Page
{
    private List<ExchangeRateDto> _allRates = [];

    public ExchangeRatesPage()
    {
        InitializeComponent();
    }

    private void LoadAllRates()
    {
        TxtStatus.Text = "Loading...";
        try
        {
            var svc = ServiceClientFactory.ExchangeRateService();
            _allRates = svc.GetAllCurrentRates();
            RatesGrid.ItemsSource = _allRates;
            TxtStatus.Text = $"{_allRates.Count} currencies loaded.";
        }
        catch (Exception ex)
        {
            TxtStatus.Text = $"Error: {ex.Message}";
        }
    }

    private void BtnLoadAll_Click(object sender, RoutedEventArgs e) => LoadAllRates();

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        var filter = TxtSearch.Text.Trim().ToUpper();
        RatesGrid.ItemsSource = string.IsNullOrEmpty(filter)
            ? _allRates
            : _allRates.Where(r => r.Code.Contains(filter) || r.Currency.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
