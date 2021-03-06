﻿/*
 * All Rights reserved to Ebby Technologies LTD @ Eli Belash, 2020.
 * Original code by QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
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

using static FinanceSharp.Constants;


namespace FinanceSharp.Indicators {
    /// <summary>
    /// 	 This indicator computes the Normalized Average True Range (NATR).
    /// 	 The Normalized Average True Range is calculated with the following formula:
    /// 	 NATR = (ATR(period) / Close) * 100
    /// </summary>
    public class NormalizedAverageTrueRange : BarIndicator {
        private readonly int _period;
        private readonly TrueRange _tr;
        private readonly AverageTrueRange _atr;
        private double _lastAtrValue;

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="NormalizedAverageTrueRange"/> class using the specified name and period.
        /// </summary> 
        /// <param name="name">The name of this indicator</param>
        /// <param name="period">The period of the NATR</param>
        public NormalizedAverageTrueRange(string name, int period) :
            base(name) {
            _period = period;
            _tr = new TrueRange(name + "_TR");
            _atr = new AverageTrueRange(name + "_ATR", period, MovingAverageType.Simple);
        }

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="NormalizedAverageTrueRange"/> class using the specified period.
        /// </summary> 
        /// <param name="period">The period of the NATR</param>
        public NormalizedAverageTrueRange(int period)
            : this($"NATR({period})", period) { }

        /// <summary>
        /// 	 Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady => Samples > _period;

        /// <summary>
        /// 	 Required period, in data points, for the indicator to be ready and fully initialized.
        /// </summary>
        public override int WarmUpPeriod => _period + 1;

        /// <summary>
        /// 	 Computes the next value of this indicator from the given state
        /// </summary>
        /// <param name="time"></param>
        /// <param name="input">The input given to the indicator</param>
        /// <returns>A new value for this indicator</returns>
        protected override DoubleArray Forward(long time, DoubleArray input) {
            _tr.Update(time, input);

            if (!IsReady) {
                _atr.Update(time, input);
                return input[CloseIdx] != 0 ? _atr / input[CloseIdx] * 100 : 0d;
            }

            if (Samples == _period + 1) {
                // first output value is SMA of TrueRange
                _atr.Update(time, input);
                _lastAtrValue = _atr;
            } else {
                // next TrueRange values are smoothed using Wilder's approach
                _lastAtrValue = (_lastAtrValue * (_period - 1) + _tr) / _period;
            }

            return input[CloseIdx] != 0 ? _lastAtrValue / input[CloseIdx] * 100 : 0d;
        }

        /// <summary>
        /// 	 Resets this indicator to its initial state
        /// </summary>
        public override void Reset() {
            _tr.Reset();
            _atr.Reset();
            base.Reset();
        }
    }
}