using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Converter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeConnection();

            fromCurrency.SelectedIndex = 0;
            toCurrency.SelectedIndex = 1;

            convertButton.IsEnabled = false;

            toAmount.IsReadOnly = true;
            messageBox.IsReadOnly = true;
        }

        private Dictionary<string, double> Currencies { get; set; }

        private void WaitingConnection()
        {
            while (!NetworkInterface.GetIsNetworkAvailable())
            {
            }

            Currencies = new Dictionary<string, double>
            {
                {"USD", GetCurrRate(145)},
                {"EUR", GetCurrRate(292)},
                {"RUB", GetCurrRate(298)},
                {"BYN", 1}
            };

            fromAmount.Dispatcher.Invoke(TextBoxReady);
        }

        private void TextBoxReady()
        {
            fromAmount.IsReadOnly = false;
            messageBox.Text = $"EUR: {Currencies["EUR"]}\r\nUSD: {Currencies["USD"]}\r\nRUB: {Currencies["RUB"]}\n";
        }

        private void InitializeConnection()
        {
            messageBox.Text = @"Подключение к интернету...";
            fromAmount.IsReadOnly = true;

            var waitingConnection = new Thread(WaitingConnection);

            waitingConnection.Start();
        }

        private double GetCurrRate(int id)
        {
            var rawResponse = DownloadPage($"http://www.nbrb.by/API/ExRates/Rates/{id}");

            var response = JObject.Parse(rawResponse);

            return (double)response.SelectToken("Cur_OfficialRate") / (double)response.SelectToken("Cur_Scale");
        }

        private static string DownloadPage(string url)
        {
            string response;

            using (var wc = new WebClient())
            {
                response = wc.DownloadString(url);
            }

            return response;
        }

        private void UpdateRatesButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeConnection();
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            var amount = Convert.ToDouble(fromAmount.Text);

            var converter = new ConverterCore();

            var convertedAmount = converter.Convert(
                amount,
                Currencies[fromCurrency.Text],
                Currencies[toCurrency.Text]
            );

            double roundedAmount = Math.Round(convertedAmount, 2);

            toAmount.Text = roundedAmount.ToString();
        }

        private void FromAmount_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                Convert.ToDouble(fromAmount.Text);
            }
            catch (FormatException)
            {
                convertButton.IsEnabled = false;

                return;
            }

            convertButton.IsEnabled = true;
        }
    }
}
