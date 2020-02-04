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

using System;


namespace FinanceSharp.Consolidators {
    /// <summary>
    /// 	 A data consolidator that can make bigger bars from any base data
    ///
    /// 	 This type acts as the base for other consolidators that produce bars on a given time step or for a count of data.
    /// </summary>
    /// <typeparam name="T">The input type into the consolidator's Update method</typeparam>
    public abstract class TradeBarConsolidatorBase : PeriodCountConsolidatorBase {
        /// <summary>
        /// 	 Creates a consolidator to produce a new 'TradeBar' representing the period
        /// </summary>
        /// <param name="period">The minimum span of time before emitting a consolidated bar</param>
        protected TradeBarConsolidatorBase(TimeSpan period)
            : base(period) { }

        /// <summary>
        /// 	 Creates a consolidator to produce a new 'TradeBar' representing the last count pieces of data
        /// </summary>
        /// <param name="maxCount">The number of pieces to accept before emiting a consolidated bar</param>
        protected TradeBarConsolidatorBase(int maxCount)
            : base(maxCount) { }

        /// <summary>
        /// 	 Creates a consolidator to produce a new 'TradeBar' representing the last count pieces of data or the period, whichever comes first
        /// </summary>
        /// <param name="maxCount">The number of pieces to accept before emiting a consolidated bar</param>
        /// <param name="period">The minimum span of time before emitting a consolidated bar</param>
        protected TradeBarConsolidatorBase(int maxCount, TimeSpan period)
            : base(maxCount, period) { }

        /// <summary>
        /// 	 Gets a copy of the current 'workingBar'.
        /// </summary>
        public TradeBarValue WorkingBar {
            get {
                unsafe {
                    return WorkingData.Get<TradeBarValue>(0);
                }
            }
        }
    }
}