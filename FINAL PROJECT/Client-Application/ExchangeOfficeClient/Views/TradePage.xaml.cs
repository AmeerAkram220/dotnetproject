using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ExchangeOfficeClient.Services;

namespace ExchangeOfficeClient.Views;

public partial class TradePage : Page
{
    public TradePage() => InitializeComponent();

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
            var svc = ServiceClientFactory.TransactionService();
            var tx = svc.BuyCurrency(Session.UserId, code, amount);

            if (tx.Error is not null)
            {
                TxtBuyMsg.Text = tx.Error;
                TxtBuyMsg.Foreground = Brushes.Red;
            }
            else
            {
                TxtBuyMsg.Text = $"Bought {tx.ToAmount:F4} {tx.ToCurrency} for {tx.FromAmount:F2} PLN @ rate {tx.Rate:F4}.";
                TxtBuyMsg.Foreground = Brushes.Green;
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
            var svc = ServiceClientFactory.TransactionService();
            var tx = svc.SellCurrency(Session.UserId, code, amount);

            if (tx.Error is not null)
            {
                TxtSellMsg.Text = tx.Error;
                TxtSellMsg.Foreground = Brushes.Red;
            }
            else
            {
                TxtSellMsg.Text = $"Sold {tx.FromAmount:F4} {tx.FromCurrency} for {tx.ToAmount:F2} PLN @ rate {tx.Rate:F4}.";
                TxtSellMsg.Foreground = Brushes.Green;
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
