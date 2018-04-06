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

using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
            MinGoldScoreTextBox.Text = config.MinGoldScore.ToString();
        }

        private async void GoButtonClicked(object sender, RoutedEventArgs e)
        {
            GoButton.IsEnabled = false;

            var config = _mainWindowController.GetCurrentConfig();

            var tsmApiKey = TsmKeyTextBox.Text;
            var realm = RealmTextBox.Text;
            var minGoldScore = MinGoldScoreTextBox.Text;

            if (config.Region.Equals("EU") && !config.Region.Equals("US"))
            {
                _mainWindowController.SaveCurrentConfig(tsmApiKey, "EU", realm, minGoldScore);
            }
            else if (!config.Region.Equals("EU") && config.Region.Equals("US"))
            {
                _mainWindowController.SaveCurrentConfig(tsmApiKey, "US", realm, minGoldScore);
            }
            else
            {
                MessageBox.Show("Something went wrong.", "Config Save Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            var downloadResult = await _mainWindowController.DownloadItemData();

            if (downloadResult)
            {
                var result = _mainWindowController.CreateImportList();
                if (result)
                {
                    StatusLabel.Content = "Successful: Created Imports.txt";
                    StatusLabel.Foreground = Brushes.Green;
                }
                else
                {
                    StatusLabel.Content =
                        "Something went wrong. Check your input for TSM API Key and Realm and try again.";
                    StatusLabel.Foreground = Brushes.Red;
                }
            }
            else
            {
                StatusLabel.Content = "Something went wrong. Check your input for TSM API Key and Realm and try again.";
                StatusLabel.Foreground = Brushes.Red;
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
    }
}