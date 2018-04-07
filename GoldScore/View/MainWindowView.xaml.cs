/*
 * MIT License
 * 
 * Copyright (c) 2018 Stanislaw Schlosser (https://github.com/0x2aff)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using GoldScore.Controller;

namespace GoldScore.View
{
    /// <inheritdoc cref="MainWindowView" />
    /// <summary>
    ///     Interaction logic for <see cref="MainWindowView" />.
    /// </summary>
    public partial class MainWindowView : IMainWindowView
    {
        private readonly MainWindowController _mainWindowController;

        /// <inheritdoc />
        /// <summary>
        ///     Instantiates a new instance of <see cref="MainWindowView" />.
        /// </summary>
        public MainWindowView()
        {
            InitializeComponent();

            _mainWindowController = new MainWindowController(this);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var config = _mainWindowController.GetCurrentConfig();

            TsmKeyTextBox.Text = config.TsmApiKey;

            if (config.Region.Equals("EU") && !config.Region.Equals("US"))
            {
                EuRadioButton.IsChecked = true;
                UsRadioButton.IsChecked = false;
            }
            else if (!config.Region.Equals("EU") && config.Region.Equals("US"))
            {
                EuRadioButton.IsChecked = false;
                UsRadioButton.IsChecked = true;
            }
            else
            {
                MessageBox.Show("Region field in Config.json has a wrong value. Should be 'EU' or 'US'.",
                    "Config Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            RealmTextBox.Text = config.Realm;
            PriceSourceComboBox.Text = config.PriceSource;
            MinGoldScoreTextBox.Text = config.MinGoldScore.ToString();
        }

        private async void GoButtonClicked(object sender, RoutedEventArgs e)
        {
            GoButton.IsEnabled = false;

            ImportRichTextBox.Document.Blocks.Clear();

            SetInfoMessage("Preparing ...");

            var tsmApiKey = TsmKeyTextBox.Text;
            var realm = RealmTextBox.Text;
            var minGoldScore = MinGoldScoreTextBox.Text;

            if (PriceSourceComboBox.Text == null)
            {
                SetErrorMessage("Please choose a price source.");
                GoButton.IsEnabled = true;
                return;
            }

            var priceSource = PriceSourceComboBox.Text;

            if (EuRadioButton.IsChecked == true)
            {
                _mainWindowController.SaveCurrentConfig(tsmApiKey, "EU", realm, priceSource, minGoldScore);
            }
            else if (UsRadioButton.IsChecked == true)
            {
                _mainWindowController.SaveCurrentConfig(tsmApiKey, "US", realm, priceSource, minGoldScore);
            }
            else
            {
                MessageBox.Show("Something went wrong.", "Config Save Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            SetInfoMessage("Downloading ...");

            var downloadResult = await _mainWindowController.DownloadItemData();

            if (downloadResult)
            {
                SetInfoMessage("Creating import list ...");
                _mainWindowController.CreateImportList();
            }

            GoButton.IsEnabled = true;
        }


        private void InputChanged(object sender, TextChangedEventArgs e)
        {
            if (TsmKeyTextBox.Text.Length > 0 && RealmTextBox.Text.Length > 0 && MinGoldScoreTextBox.Text.Length > 0)
                GoButton.IsEnabled = true;
            else
                GoButton.IsEnabled = false;
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void OnRedditLinkClicked(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        public void SetErrorMessage(string message)
        {
            StatusLabel.Content = message;
            StatusLabel.Foreground = Brushes.Red;
        }

        public void SetSuccessfulMessage(string message)
        {
            StatusLabel.Content = message;
            StatusLabel.Foreground = Brushes.Green;
        }

        public void SetInfoMessage(string message)
        {
            StatusLabel.Content = message;
            StatusLabel.Foreground = Brushes.Black;
        }

        public void SetImportListBox(string importList)
        {
            var flowDoc = new FlowDocument();
            flowDoc.Blocks.Add(new Paragraph(new Run(importList)));

            ImportRichTextBox.Document = flowDoc;
        }
    }
}