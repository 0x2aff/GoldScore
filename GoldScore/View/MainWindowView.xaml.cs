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
using System.Windows.Input;
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

        private static string _colorGrey = "#FF818181";
        private static string _colorError = "#FFF01E1E";
        private static string _colorSuccess = "#FF176C36";

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

        private void GoButtonClicked(object sender, RoutedEventArgs e)
        {
            _mainWindowController.DownloadItemData();
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}