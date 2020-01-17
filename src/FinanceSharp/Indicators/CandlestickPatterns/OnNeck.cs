﻿/*
 * All Rights reserved to Ebby Technologies LTD @ Eli Belash, 2020.
 * Original code by: 
 * 
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using FinanceSharp.Data.Market;
using FinanceSharp.Data.Rolling;
using static FinanceSharp.Constants;
using FinanceSharp.Data;


namespace FinanceSharp.Indicators.CandlestickPatterns {
    /// <summary>
    /// 	 On-Neck candlestick pattern indicator
    /// </summary>
    /// <remarks>
    /// 	 Must have:
    /// - first candle: long black candle
    /// - second candle: white candle with open below previous day low and close equal to previous day low
    /// 	 The meaning of "equal" is specified with SetCandleSettings
    /// 	 The returned value is negative(-1): on-neck is always bearish
    /// 	 The user should consider that on-neck is significant when it appears in a downtrend, while this function
    /// 	 does not consider it
    /// </remarks>
    public class OnNeck : CandlestickPattern {
        private readonly int _equalAveragePeriod;
        private readonly int _bodyLongAveragePeriod;

        private double _equalPeriodTotal;
        private double _bodyLongPeriodTotal;

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="OnNeck"/> class using the specified name.
        /// </summary>
        /// <param name="name">The name of this indicator</param>
        public OnNeck(string name)
            : base(name, Math.Max(CandleSettings.Get(CandleSettingType.Equal).AveragePeriod, CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod) + 1 + 1) {
            _equalAveragePeriod = CandleSettings.Get(CandleSettingType.Equal).AveragePeriod;
            _bodyLongAveragePeriod = CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod;
        }

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="OnNeck"/> class.
        /// </summary>
        public OnNeck()
            : this("ONNECK") { }

        /// <summary>
        /// 	 Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady {
            get { return Samples >= Period; }
        }

        /// <summary>
        /// 	 Computes the next value of this indicator from the given state
        /// </summary>
        /// <param name="timeWindow"></param>
        /// <param name="window">The window of data held in this indicator</param>
        /// <param name="time"></param>
        /// <param name="input"></param>
        /// <returns>A new value for this indicator</returns>
        protected override DoubleArray Forward(IReadOnlyWindow<long> timeWindow, IReadOnlyWindow<DoubleArray> window, long time, DoubleArray input) {
            if (!IsReady) {
                if (Samples >= Period - _equalAveragePeriod) {
                    _equalPeriodTotal += GetCandleRange(CandleSettingType.Equal, window[1]);
                }

                if (Samples >= Period - _bodyLongAveragePeriod) {
                    _bodyLongPeriodTotal += GetCandleRange(CandleSettingType.BodyLong, window[1]);
                }

                return Constants.Zero;
            }

            double value;
            if (
                // 1st: black
                GetCandleColor(window[1]) == CandleColor.Black &&
                //      long
                GetRealBody(window[1]) > GetCandleAverage(CandleSettingType.BodyLong, _bodyLongPeriodTotal, window[1]) &&
                // 2nd: white
                GetCandleColor(input) == CandleColor.White &&
                //      open below prior low
                input.Open < window[1].Low &&
                //      close equal to prior low
                input[CloseIdx] <= window[1].Low + GetCandleAverage(CandleSettingType.Equal, _equalPeriodTotal, window[1]) &&
                input[CloseIdx] >= window[1].Low - GetCandleAverage(CandleSettingType.Equal, _equalPeriodTotal, window[1])
            )
                value = -1d;
            else
                value = Constants.Zero;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            _equalPeriodTotal += GetCandleRange(CandleSettingType.Equal, window[1]) -
                                 GetCandleRange(CandleSettingType.Equal, window[_equalAveragePeriod + 1]);

            _bodyLongPeriodTotal += GetCandleRange(CandleSettingType.BodyLong, window[1]) -
                                    GetCandleRange(CandleSettingType.BodyLong, window[_bodyLongAveragePeriod + 1]);

            return value;
        }

        /// <summary>
        /// 	 Resets this indicator to its initial state
        /// </summary>
        public override void Reset() {
            _equalPeriodTotal = Constants.Zero;
            _bodyLongPeriodTotal = Constants.Zero;
            base.Reset();
        }
    }
}