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
        DpTo.SelectedDate = DateTime.Today;
        DpFrom.SelectedDate = DateTime.Today.AddDays(-30);
    }

    // ── Current rates ────────────────────────────────────────────────────────

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
            : _allRates.Where(r => r.Code.Contains(filter) ||
                r.Currency.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
    }


    private void BtnLoadHistory_Click(object sender, RoutedEventArgs e)
    {
        HistSummary.Visibility = Visibility.Collapsed;
        HistGrid.ItemsSource = null;
        TxtHistStatus.Foreground = System.Windows.Media.Brushes.Gray;

        var code = TxtHistCode.Text.Trim().ToUpper();
        if (string.IsNullOrEmpty(code))
        {
            TxtHistStatus.Text = "Enter a currency code.";
            return;
        }

        if (DpFrom.SelectedDate is null || DpTo.SelectedDate is null)
        {
            TxtHistStatus.Text = "Select both dates.";
            return;
        }

        var from = DpFrom.SelectedDate.Value;
        var to = DpTo.SelectedDate.Value;

        if (from > to)
        {
            TxtHistStatus.Text = "'From' must be before 'To'.";
            return;
        }

        if ((to - from).TotalDays > 93)
        {
            TxtHistStatus.Text = "Max range is 93 days (NBP limit).";
            TxtHistStatus.Foreground = System.Windows.Media.Brushes.OrangeRed;
            return;
        }

        TxtHistStatus.Text = "Loading...";
        try
        {
            var rates = ServiceClientFactory.ExchangeRateService()
                .GetHistoricalRates(code, from, to);

            if (rates.Count == 0)
            {
                TxtHistStatus.Text = $"No data for '{code}' in this range.";
                return;
            }

            HistGrid.ItemsSource = rates;
            TxtHistStatus.Text = $"{rates.Count} entries.";

            // Summary statistics
            var mids = rates.Select(r => r.Mid).ToList();
            TxtHistMin.Text = $"{mids.Min():F4}";
            TxtHistMax.Text = $"{mids.Max():F4}";
            TxtHistAvg.Text = $"{mids.Average():F4}";
            HistSummary.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            TxtHistStatus.Text = $"Error: {ex.Message}";
            TxtHistStatus.Foreground = System.Windows.Media.Brushes.Red;
        }
    }
}
