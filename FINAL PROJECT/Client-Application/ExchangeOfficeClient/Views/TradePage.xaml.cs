using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ExchangeOfficeClient.Services;

namespace ExchangeOfficeClient.Views;

public partial class TradePage : Page
{
    // Cached rates to avoid hitting API on every keystroke
    private decimal _buyRate = 0;
    private decimal _sellRate = 0;
    private DateTime _buyRateFetched = DateTime.MinValue;
    private DateTime _sellRateFetched = DateTime.MinValue;

    public TradePage()
    {
        InitializeComponent();
        RefreshPlnBalance();
    }

    private void RefreshPlnBalance()
    {
        try
        {
            var balances = ServiceClientFactory.AccountService().GetBalances(Session.UserId);
            var pln = balances.FirstOrDefault(b => b.CurrencyCode == "PLN");
            TxtPlnBalance.Text = pln is not null ? $"{pln.Amount:F2} PLN" : "0.00 PLN";
        }
        catch { TxtPlnBalance.Text = "unavailable"; }
    }

    private void BtnRefreshBalance_Click(object sender, RoutedEventArgs e) => RefreshPlnBalance();

    // ── Rate preview on input change ────────────────────────────────────────

    private void TxtBuyCurrency_TextChanged(object sender, TextChangedEventArgs e) => UpdateBuyPreview();
    private void TxtBuyAmount_TextChanged(object sender, TextChangedEventArgs e) => UpdateBuyPreview();
    private void TxtSellCurrency_TextChanged(object sender, TextChangedEventArgs e) => UpdateSellPreview();
    private void TxtSellAmount_TextChanged(object sender, TextChangedEventArgs e) => UpdateSellPreview();

    private void UpdateBuyPreview()
    {
        var code = TxtBuyCurrency.Text.Trim().ToUpper();
        if (code.Length < 3) { BuyPreview.Visibility = Visibility.Collapsed; return; }
        if (!decimal.TryParse(TxtBuyAmount.Text.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        { BuyPreview.Visibility = Visibility.Collapsed; return; }

        try
        {
            // Re-fetch rate only if code changed or cache is older than 60s
            if (_buyRate == 0 || (DateTime.Now - _buyRateFetched).TotalSeconds > 60)
            {
                var rate = ServiceClientFactory.ExchangeRateService().GetCurrentRate(code);
                if (rate.Error is not null) { BuyPreview.Visibility = Visibility.Collapsed; return; }
                _buyRate = rate.Mid;
                _buyRateFetched = DateTime.Now;
            }
            var cost = Math.Round(amount * _buyRate, 2);
            TxtBuyPreview.Text = $"Rate: 1 {code} = {_buyRate:F4} PLN  →  Cost: {cost:F2} PLN";
            BuyPreview.Visibility = Visibility.Visible;
        }
        catch { BuyPreview.Visibility = Visibility.Collapsed; }
    }

    private void UpdateSellPreview()
    {
        var code = TxtSellCurrency.Text.Trim().ToUpper();
        if (code.Length < 3) { SellPreview.Visibility = Visibility.Collapsed; return; }
        if (!decimal.TryParse(TxtSellAmount.Text.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        { SellPreview.Visibility = Visibility.Collapsed; return; }

        try
        {
            if (_sellRate == 0 || (DateTime.Now - _sellRateFetched).TotalSeconds > 60)
            {
                var rate = ServiceClientFactory.ExchangeRateService().GetCurrentRate(code);
                if (rate.Error is not null) { SellPreview.Visibility = Visibility.Collapsed; return; }
                _sellRate = rate.Mid;
                _sellRateFetched = DateTime.Now;
            }
            var gain = Math.Round(amount * _sellRate, 2);
            TxtSellPreview.Text = $"Rate: 1 {code} = {_sellRate:F4} PLN  →  You receive: {gain:F2} PLN";
            SellPreview.Visibility = Visibility.Visible;
        }
        catch { SellPreview.Visibility = Visibility.Collapsed; }
    }

    // ── Buy ──────────────────────────────────────────────────────────────────

    private void BtnBuy_Click(object sender, RoutedEventArgs e)
    {
        TxtBuyMsg.Visibility = Visibility.Collapsed;
        var code = TxtBuyCurrency.Text.Trim().ToUpper();

        if (!decimal.TryParse(TxtBuyAmount.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount <= 0 || string.IsNullOrEmpty(code))
        {
            TxtBuyMsg.Text = "Enter a valid currency code and amount.";
            TxtBuyMsg.Foreground = Brushes.Red;
            TxtBuyMsg.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            var tx = ServiceClientFactory.TransactionService().BuyCurrency(Session.UserId, code, amount);
            if (tx.Error is not null)
            {
                TxtBuyMsg.Text = tx.Error;
                TxtBuyMsg.Foreground = Brushes.Red;
            }
            else
            {
                TxtBuyMsg.Text = $"✔ Bought {tx.ToAmount:F4} {tx.ToCurrency} for {tx.FromAmount:F2} PLN @ rate {tx.Rate:F4}.";
                TxtBuyMsg.Foreground = Brushes.Green;
                _buyRate = 0; // invalidate cached rate
                RefreshPlnBalance();
            }
            TxtBuyMsg.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            TxtBuyMsg.Text = $"Error: {ex.Message}";
            TxtBuyMsg.Foreground = Brushes.Red;
            TxtBuyMsg.Visibility = Visibility.Visible;
        }
    }

    // ── Sell ─────────────────────────────────────────────────────────────────

    private void BtnSell_Click(object sender, RoutedEventArgs e)
    {
        TxtSellMsg.Visibility = Visibility.Collapsed;
        var code = TxtSellCurrency.Text.Trim().ToUpper();

        if (!decimal.TryParse(TxtSellAmount.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount <= 0 || string.IsNullOrEmpty(code))
        {
            TxtSellMsg.Text = "Enter a valid currency code and amount.";
            TxtSellMsg.Foreground = Brushes.Red;
            TxtSellMsg.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            var tx = ServiceClientFactory.TransactionService().SellCurrency(Session.UserId, code, amount);
            if (tx.Error is not null)
            {
                TxtSellMsg.Text = tx.Error;
                TxtSellMsg.Foreground = Brushes.Red;
            }
            else
            {
                TxtSellMsg.Text = $"✔ Sold {tx.FromAmount:F4} {tx.FromCurrency} for {tx.ToAmount:F2} PLN @ rate {tx.Rate:F4}.";
                TxtSellMsg.Foreground = Brushes.Green;
                _sellRate = 0; // invalidate cached rate
                RefreshPlnBalance();
            }
            TxtSellMsg.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            TxtSellMsg.Text = $"Error: {ex.Message}";
            TxtSellMsg.Foreground = Brushes.Red;
            TxtSellMsg.Visibility = Visibility.Visible;
        }
    }
}
