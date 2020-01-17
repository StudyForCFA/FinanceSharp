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
using FinanceSharp.Data;
using static FinanceSharp.Constants;
using FinanceSharp.Data;


namespace FinanceSharp.Indicators {
    /// <summary>
    /// 	 A momentum indicator developed by Edwin “Sedge” Coppock in October 1965.
    /// 	 The goal of this indicator is to identify long-term buying opportunities in the S&amp;P500 and Dow Industrials.
    /// 	 Source: http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:coppock_curve
    /// </summary>
    public class CoppockCurve : IndicatorBase {
        private readonly RateOfChangePercent _longRoc;
        private readonly LinearWeightedMovingAverage _lwma;
        private readonly RateOfChangePercent _shortRoc;

        /// <summary>
        /// 	 Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady => _lwma.IsReady;

        /// <summary>
        /// 	 Required period, in data points, for the indicator to be ready and fully initialized.
        /// </summary>
        public int WarmUpPeriod { get; }

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="CoppockCurve" /> indicator with its default values.
        /// </summary>
        public CoppockCurve()
            : this(11, 14, 10) { }

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="CoppockCurve"/> indicator
        /// </summary>
        /// <param name="shortRocPeriod">The period for the short ROC</param>
        /// <param name="longRocPeriod">The period for the long ROC</param>
        /// <param name="lwmaPeriod">The period for the LWMA</param>
        public CoppockCurve(int shortRocPeriod, int longRocPeriod, int lwmaPeriod)
            : this($"CC({shortRocPeriod},{longRocPeriod},{lwmaPeriod})", shortRocPeriod, longRocPeriod, lwmaPeriod) { }

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="CoppockCurve" /> indicator
        /// </summary>
        /// <param name="name">A name for the indicator</param>
        /// <param name="shortRocPeriod">The period for the short ROC</param>
        /// <param name="longRocPeriod">The period for the long ROC</param>
        /// <param name="lwmaPeriod">The period for the LWMA</param>
        public CoppockCurve(string name, int shortRocPeriod, int longRocPeriod, int lwmaPeriod)
            : base(name) {
            _shortRoc = new RateOfChangePercent(shortRocPeriod);
            _longRoc = new RateOfChangePercent(longRocPeriod);
            _lwma = new LinearWeightedMovingAverage(lwmaPeriod);
            WarmUpPeriod = Math.Max(shortRocPeriod, longRocPeriod) + lwmaPeriod - 1;
        }

        /// <summary>
        /// 	 Resets this indicator to its initial state
        /// </summary>
        public override void Reset() {
            base.Reset();
            _shortRoc.Reset();
            _longRoc.Reset();
            _lwma.Reset();
        }

        /// <summary>
        /// 	 Computes the next value of this indicator from the given state
        /// </summary>
        /// <param name="time"></param>
        /// <param name="input">The input given to the indicator</param>
        /// <returns>A new value for this indicator</returns>
        protected override DoubleArray Forward(long time, DoubleArray input) {
            _shortRoc.Update(time, input);
            _longRoc.Update(time, input);
            if (!_longRoc.IsReady || !_shortRoc.IsReady) {
                return Constants.Zero;
            }

            _lwma.Update(time, _shortRoc + _longRoc);
            return _lwma;
        }
    }
}