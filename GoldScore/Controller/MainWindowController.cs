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

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GoldScore.Model;
using GoldScore.View;
using Newtonsoft.Json;

namespace GoldScore.Controller
{
    /// <summary>
    ///     Controller for <see cref="MainWindowView" />.
    /// </summary>
    public class MainWindowController
    {
        private readonly IMainWindowView _mainWindowView;

        public List<ItemModel> ItemList = new List<ItemModel>();

        /// <summary>
        ///     Instantiates a new instance of <see cref="MainWindowController" />
        /// </summary>
        /// <param name="mainWindowView"></param>
        public MainWindowController(IMainWindowView mainWindowView)
        {
            _mainWindowView = mainWindowView;
        }

        /// <summary>
        ///     Returns the current configuration object.
        /// </summary>
        /// <returns>
        ///     <see cref="Config" />
        /// </returns>
        public Config GetCurrentConfig()
        {
            return ConfigManager.Instance.Config;
        }

        /// <summary>
        ///     Saves the current configuration input into the object and configuration file.
        /// </summary>
        /// <param name="tsmApiKey">TradeSkillMaster API Key.</param>
        /// <param name="region">Region: EU or US.</param>
        /// <param name="realm">Realm.</param>
        /// <param name="priceSource">Price Source.</param>
        /// <param name="minGoldScore">Minimum Gold Score.</param>
        public void SaveCurrentConfig(string tsmApiKey, string region, string realm, string priceSource,
            string minGoldScore)
        {
            ConfigManager.Instance.Config.TsmApiKey = tsmApiKey;
            ConfigManager.Instance.Config.Region = region;
            ConfigManager.Instance.Config.Realm = realm;
            ConfigManager.Instance.Config.PriceSource = priceSource;
            ConfigManager.Instance.Config.MinGoldScore = int.Parse(minGoldScore);
            ConfigManager.Instance.SaveConfig();
        }

        /// <summary>
        ///     Downloads the currently available TSM API data.
        /// </summary>
        /// <returns>TSM API Status Code: 200 = true, [400:500] false.</returns>
        public async Task<bool> DownloadItemData()
        {
            using (var httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            }))
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept",
                    "text/html,application/xhtml+xml,application/xml");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

                httpClient.BaseAddress = new Uri("http://api.tradeskillmaster.com/v1/item/");

                var tsmDownloadUrl =
                    $"{GetCurrentConfig().Region}/{GetCurrentConfig().Realm}?format=json&fields=Id%2CMarketValue%2CMinBuyout%2CHistoricalPrice%2CRegionMarketAvg" +
                    $"%2CRegionMinBuyoutAvg%2CRegionHistoricalPrice%2CRegionSaleAvg%2CRegionAvgDailySold%2CRegionSaleRate&apiKey={GetCurrentConfig().TsmApiKey}";

                var response = await httpClient.GetAsync(tsmDownloadUrl);

                if (!response.IsSuccessStatusCode)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            _mainWindowView.SetErrorMessage(
                                "Download error: Check your input for TSM API Key and Realm and try again.");
                            break;
                        case HttpStatusCode.InternalServerError:
                            _mainWindowView.SetErrorMessage(
                                "Download error: TSM API currently has some problems. Please try again later.");
                            break;
                        default:
                            _mainWindowView.SetErrorMessage(
                                $"Download error: Something went wrong. Please provide author with following error code: {response.StatusCode.ToString()}");
                            break;
                    }

                    return false;
                }

                var tsmResponseContent = await response.Content.ReadAsStringAsync();
                File.WriteAllText("TSMData.json", tsmResponseContent);

                ItemList = JsonConvert.DeserializeObject<List<ItemModel>>(tsmResponseContent);
                return true;
            }
        }

        /// <summary>
        ///     Creates the item import list based on current configuration.
        /// </summary>
        public void CreateImportList()
        {
            var importList = string.Empty;

            foreach (var item in ItemList)
            {
                var priceSource = GetCurrentConfig().PriceSource;
                long price;

                switch (priceSource)
                {
                    case "MarketValue":
                        price = item.MarketValue;
                        break;
                    case "MinBuyout":
                        price = item.MinBuyout;
                        break;
                    case "HistoricalPrice":
                        price = item.HistoricalPrice;
                        break;
                    case "RegionMarketAvg":
                        price = item.RegionMarketAvg;
                        break;
                    case "RegionMinBuyoutAvg":
                        price = item.RegionMinBuyoutAvg;
                        break;
                    case "RegionHistoricalPrice":
                        price = item.RegionHistoricalPrice;
                        break;
                    case "RegionSaleAvg":
                        price = item.RegionSaleAvg;
                        break;
                    default:
                        _mainWindowView.SetErrorMessage(
                            "Error: Did you choose a price source?");
                        return;
                }

                var goldScore = (float) price / 10000 * item.RegionSaleRate * item.RegionAvgDailySold;
                if (goldScore >= GetCurrentConfig().MinGoldScore)
                    importList = importList + "i:" + item.Id + ",";
            }

            importList = importList.TrimEnd(',');

            if (importList.Length <= 0)
                _mainWindowView.SetErrorMessage(
                    "Import list error: Could not find any item that that an appropiate gold score.");

            // File.WriteAllText("Imports.txt", importList);

            _mainWindowView.SetImportListBox(importList);

            _mainWindowView.SetSuccessfulMessage("Successful: Created Imports.txt");
        }
    }
}