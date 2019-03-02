using System;
using System.Globalization;

namespace IQFeed.CSharpApiExtensions.Lookup.Historical
{
    public class HistoricalInterval
    {
        public DateTime Timestamp { get; set; }
        public float Open { get; set; }
        public float High { get; set; }
        public float Low { get; set; }
        public float Close { get; set; }
        public int PeriodVolume { get; set; }
        public int PeriodTrade { get; set; }
        public long TotalVolume { get; set; }
        public int TotalTrade { get; set; }
        public float Wap { get; set; }

        public override string ToString()
        {
            return $"{Timestamp.ToString("yy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture)} O:{Open} H:{High} L:{Low} C:{Close} V:{PeriodVolume} T:{PeriodTrade} TV:{TotalVolume} TT:{TotalTrade} W:{Wap:0.00}";
        }
    }
}