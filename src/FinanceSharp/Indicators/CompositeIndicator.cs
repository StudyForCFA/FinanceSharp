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


namespace FinanceSharp.Indicators {
    /// <summary>
    /// 	 This indicator is capable of wiring up two separate indicators into a single indicator
    /// 	 such that the output of each will be sent to a user specified function.
    /// </summary>
    /// <remarks>
    /// 	 This type is initialized such that there is no need to call the Update function. This indicator
    /// 	 will have its values automatically updated each time a new piece of data is received from both
    /// 	 the left and right indicators.
    /// </remarks>
    /// <typeparam name="T">The type of data input into this indicator</typeparam>
    public class CompositeIndicator : IndicatorBase {
        /// <summary>
        /// 	 Delegate type used to compose the output of two indicators into a new value.
        /// </summary>
        /// <remarks>
        /// 	 A simple example would be to compute the difference between the two indicators (such as with MACD)
        /// (left, right) => left - right
        /// </remarks>
        /// <param name="left">The left indicator</param>
        /// <param name="right">The right indicator</param>
        /// <returns>And indicator result representing the composition of the two indicators</returns>
        public delegate IndicatorResult IndicatorComposer(IndicatorBase left, IndicatorBase right);

        /// <summary>function used to compose the individual indicators</summary>
        private readonly IndicatorComposer _composer;

        /// <summary>
        /// 	 Gets the 'left' indicator for the delegate
        /// </summary>
        public IndicatorBase Left { get; private set; }

        /// <summary>
        /// 	 Gets the 'right' indicator for the delegate
        /// </summary>
        public IndicatorBase Right { get; private set; }

        /// <summary>
        /// 	 Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady {
            get { return Left.IsReady && Right.IsReady; }
        }

        /// <summary>
        /// 	 Resets this indicator to its initial state
        /// </summary>
        public override void Reset() {
            Left.Reset();
            Right.Reset();
            base.Reset();
        }

        /// <summary>
        /// 	 Creates a new CompositeIndicator capable of taking the output from the left and right indicators
        /// 	 and producing a new value via the composer delegate specified
        /// </summary>
        /// <param name="name">The name of this indicator</param>
        /// <param name="left">The left indicator for the 'composer'</param>
        /// <param name="right">The right indidcator for the 'composoer'</param>
        /// <param name="composer">Function used to compose the left and right indicators</param>
        public CompositeIndicator(string name, IndicatorBase left, IndicatorBase right, IndicatorComposer composer)
            : base(name) {
            _composer = composer;
            Left = left;
            Right = right;
            ConfigureEventHandlers();
        }

        /// <summary>
        /// 	 Creates a new CompositeIndicator capable of taking the output from the left and right indicators
        /// 	 and producing a new value via the composer delegate specified
        /// </summary>
        /// <param name="left">The left indicator for the 'composer'</param>
        /// <param name="right">The right indidcator for the 'composoer'</param>
        /// <param name="composer">Function used to compose the left and right indicators</param>
        public CompositeIndicator(IndicatorBase left, IndicatorBase right, IndicatorComposer composer)
            : this($"COMPOSE({left.Name},{right.Name})", left, right, composer) { }

        /// <summary>
        /// 	 Updates the state of this indicator with the given value and returns true
        /// 	 if this indicator is ready, false otherwise
        /// </summary>
        /// <param name="time"></param>
        /// <param name="input">The value to use to update this indicator</param>
        /// <returns>True if this indicator is ready, false otherwise</returns>
        public override bool Update(long time, DoubleArray input) {
            // compute a new value and update our previous time
            Samples++;

            var result = _composer.Invoke(Left, Right);
            if (result.Status != IndicatorStatus.Success)
                return IsReady;

            Current = result.Value;
            CurrentTime = time;

            // let others know we've produced a new data point
            OnUpdated(time, Current);

            return IsReady;
        }


        /// <summary>
        /// 	 Computes the next value of this indicator from the given state
        /// </summary>
        /// <remarks>
        /// 	 Since this class overrides <see cref="ValidateAndForward"/>, this method is a no-op
        /// </remarks>
        /// <param name="time"></param>
        /// <param name="input">The input given to the indicator</param>
        /// <returns>A new value for this indicator</returns>
        protected override DoubleArray Forward(long time, DoubleArray input) {
            // this should never actually be invoked
            return _composer.Invoke(Left, Right).Value;
        }

        /// <summary>
        /// 	 Configures the event handlers for Left.Updated and Right.Updated to update this instance when
        /// 	 they both have new data.
        /// </summary>
        private void ConfigureEventHandlers() {
            // if either of these are constants then there's no reason
            bool leftIsConstant = Left.GetType().IsSubclassOfGeneric(typeof(ConstantIndicator));
            bool rightIsConstant = Right.GetType().IsSubclassOfGeneric(typeof(ConstantIndicator));

            // wire up the Updated events such that when we get a new piece of data from both left and right
            // we'll call update on this indicator. It's important to note that the CompositeIndicator only uses
            // the timestamp that gets passed into the Update function, his compuation is soley a function
            // of the left and right indicator via '_composer'

            DoubleArray newLeftData = null;
            DoubleArray newRightData = null;
            Left.Updated += (time, updated) => {
                newLeftData = updated;

                // if we have left and right data (or if right is a constant) then we need to update
                if (newRightData != null || rightIsConstant) {
                    Update(MaxTime(time, updated), updated);
                    // reset these to null after each update
                    newLeftData = null;
                    newRightData = null;
                }
            };

            Right.Updated += (time, updated) => {
                newRightData = updated;

                // if we have left and right data (or if left is a constant) then we need to update
                if (newLeftData != null || leftIsConstant) {
                    Update(time, updated);
                    // reset these to null after each update
                    newLeftData = null;
                    newRightData = null;
                }
            };
        }

        private long MaxTime(long time, DoubleArray updated) {
            return Math.Max(time, Math.Max(Right.CurrentTime, Left.CurrentTime));
        }
    }
}