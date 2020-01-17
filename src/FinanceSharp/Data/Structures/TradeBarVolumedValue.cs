﻿using System.Runtime.InteropServices;

namespace FinanceSharp.Data {
    /// <summary>
    ///     Represents a CHLOV candle struct.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TradeBarVolumedValue : DataStruct {
        public double Close;
        public double High;
        public double Low;
        public double Open;
        public double Volume;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
        public TradeBarVolumedValue(double close, double high, double low, double open, double volume) {
            Close = close;
            High = high;
            Low = low;
            Open = open;
            Volume = volume;
        }

        int DataStruct.Properties => Properties;
        public const int Properties = 5;

        /// <summary>Creates a new object that is a copy of the current instance.</summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone() {
            return new TradeBarVolumedValue(Close, High, Low, Open, Volume);
        }

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>The fully qualified type name.</returns>
        public override string ToString() {
            return $"{nameof(Close)}: {Close}, {nameof(High)}: {High}, {nameof(Low)}: {Low}, {nameof(Open)}: {Open}, {nameof(Volume)}: {Volume}, {nameof(Properties)}: {Properties}";
        }
    }
}