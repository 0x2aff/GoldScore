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

using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace GoldScore
{
    /// <summary>
    ///     Management class that handles configuration access.
    /// </summary>
    public sealed class ConfigManager
    {
        private static volatile ConfigManager _instance;
        private static readonly object SyncRoot = new object();

        private ConfigManager()
        {
            try
            {
                var configFile = File.ReadAllText("Config.json");
                Config = JsonConvert.DeserializeObject<Config>(configFile);

                if (Config != null && CheckConfig()) return;

                MessageBox.Show("Configuration file corrupt. Create new default configuration file.",
                    "Configuration Corrupt", MessageBoxButton.OK, MessageBoxImage.Warning);
                CreateDefaultConfig();
            }
            catch (FileNotFoundException)
            {
                CreateDefaultConfig();
            }
        }

        public Config Config { get; set; }

        /// <summary>
        ///     Get current instance of the <see cref="ConfigManager" />.
        /// </summary>
        public static ConfigManager Instance
        {
            get
            {
                if (_instance != null) return _instance;

                lock (SyncRoot)
                {
                    if (_instance == null)
                        _instance = new ConfigManager();
                }

                return _instance;
            }
        }

        /// <summary>
        ///     Saves the current instance of the config into a json file.
        /// </summary>
        public void SaveConfig()
        {
            var configFile = JsonConvert.SerializeObject(Config, Formatting.Indented);

            File.WriteAllText("Config.json", configFile);
        }

        private void CreateDefaultConfig()
        {
            Config = new Config
            {
                TsmApiKey = string.Empty,
                Region = "EU",
                Realm = string.Empty,
                MinGoldScore = 1500
            };

            var configFile = JsonConvert.SerializeObject(Config, Formatting.Indented);

            File.WriteAllText("Config.json", configFile);
        }

        private bool CheckConfig()
        {
            return Config.TsmApiKey != null && Config.Region != null && Config.Realm != null && Config.MinGoldScore > 0;
        }
    }

    /// <summary>
    ///     Defines the structure of the config.
    /// </summary>
    public class Config
    {
        public string TsmApiKey { get; set; }
        public string Region { get; set; }
        public string Realm { get; set; }
        public int MinGoldScore { get; set; }
    }
}