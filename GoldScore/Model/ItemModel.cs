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

namespace GoldScore.Model
{
    /// <summary>
    ///     Represents the item entity.
    /// </summary>
    public class ItemModel
    {
        public long Id { get; set; }

        public long MarketValue { get; set; }
        public long MinBuyout { get; set; }
        public long HistoricalPrice { get; set; }
        public long RegionMarketAvg { get; set; }
        public long RegionMinBuyoutAvg { get; set; }
        public long RegionHistoricalPrice { get; set; }
        public long RegionSaleAvg { get; set; }

        public float RegionAvgDailySold { get; set; }
        public float RegionSaleRate { get; set; }
    }
}