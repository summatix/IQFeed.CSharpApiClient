using System;
using System.Collections.Generic;
using System.Linq;
using IQFeed.CSharpApiClient.Lookup.Historical.Interfaces;
using IQFeed.CSharpApiExtensions.Common;

namespace IQFeed.CSharpApiExtensions.Lookup.Historical
{
    public static class TickMessageExtensions
    {
        /// <summary>
        /// Resample TickMessage
        /// </summary>
        /// <param name="tickMessages"></param>
        /// <param name="interval"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static IEnumerable<HistoricalInterval> ToHistoricalIntervals(
            this IEnumerable<ITickMessage> tickMessages,
            TimeSpan interval,
            MessageDirection direction = MessageDirection.Descending)
        {
            return direction == MessageDirection.Descending ?
                ToHistoricalIntervalsDescending(tickMessages, interval) : 
                ToHistoricalIntervalsAscending(tickMessages, interval);
        }

        private static IEnumerable<HistoricalInterval> ToHistoricalIntervalsAscending(this IEnumerable<ITickMessage> tickMessages, TimeSpan interval)
        {
            var intervalTicks = interval.Ticks;
            DateTime? nextTimestamp = null;
            HistoricalInterval currentIntraday = null;

            var totalVolume = 0;
            var totalTrade = 0;

            foreach (var tick in tickMessages)
            {
                // to effect the price the trade must be C or E
                if (tick.BasisForLast == 'O')    
                    continue;

                // only if a quantity have been traded
                if (tick.LastSize == 0)           
                    continue;

                totalVolume += tick.LastSize;
                totalTrade += 1;

                if (tick.Timestamp < nextTimestamp)
                {
                    if (tick.Last < currentIntraday.Low)
                        currentIntraday.Low = tick.Last;

                    if (tick.Last > currentIntraday.High)
                        currentIntraday.High = tick.Last;

                    currentIntraday.Close = tick.Last;

                    currentIntraday.PeriodVolume += tick.LastSize;
                    currentIntraday.PeriodTrade += 1;

                    currentIntraday.TotalVolume = totalVolume;
                    currentIntraday.TotalTrade = totalTrade;

                    currentIntraday.Wap += tick.Last * tick.LastSize;

                    continue;
                }

                if (currentIntraday != null)
                {
                    // reset the counts if dates differ
                    if (tick.Timestamp.Date != currentIntraday.Timestamp.Date)
                    {
                        totalVolume = tick.LastSize;
                        totalTrade = 1;
                    }

                    currentIntraday.Wap = currentIntraday.Wap / currentIntraday.PeriodVolume;
                    yield return currentIntraday;
                }

                var currentTimestamp = tick.Timestamp.Trim(intervalTicks);
                nextTimestamp = currentTimestamp.AddTicks(intervalTicks);
                currentIntraday = new HistoricalInterval()
                {
                    Timestamp = currentTimestamp,
                    Open = tick.Last,
                    High = tick.Last,
                    Low = tick.Last,
                    Close = tick.Last,
                    PeriodVolume = tick.LastSize,
                    PeriodTrade = 1,
                    TotalVolume = totalVolume,
                    TotalTrade = totalTrade,
                    Wap = tick.Last * tick.LastSize
                };
            }
        }

        private static IEnumerable<HistoricalInterval> ToHistoricalIntervalsDescending(this IEnumerable<ITickMessage> tickMessages, TimeSpan interval)
        {
            // TODO: implement this algo
            return tickMessages.Reverse().ToHistoricalIntervalsAscending(interval);
        }
    }
}