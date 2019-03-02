using System;
using System.Collections.Generic;
using System.Linq;
using IQFeed.CSharpApiClient.Lookup.Historical.Interfaces;
using IQFeed.CSharpApiExtensions.Common;

namespace IQFeed.CSharpApiExtensions.Lookup.Historical
{
    public static class IntervalMessageExtensions
    {
        public static IEnumerable<HistoricalInterval> ToHistoricalIntervals(
            this IEnumerable<IIntervalMessage> intervalMessages, 
            TimeSpan interval, 
            MessageDirection direction = MessageDirection.Descending)
        {
            return direction == MessageDirection.Descending
                ? intervalMessages.ToHistoricalIntervalsDescending(interval)
                : intervalMessages.ToHistoricalIntervalsAscending(interval);
        }

        private static IEnumerable<HistoricalInterval> ToHistoricalIntervalsAscending(
            this IEnumerable<IIntervalMessage> intervalMessages, 
            TimeSpan interval)
        {
            var intervalTicks = interval.Ticks;
            DateTime? nextTimestamp = null;
            HistoricalInterval currentIntraday = null;

            var totalTrade = 0;

            foreach (var intervalMsg in intervalMessages)
            {
                if (intervalMsg.Timestamp < nextTimestamp)
                {
                    totalTrade += intervalMsg.NumberOfTrades;

                    if (intervalMsg.Low < currentIntraday.Low)
                        currentIntraday.Low = intervalMsg.Low;

                    if (intervalMsg.High > currentIntraday.High)
                        currentIntraday.High = intervalMsg.High;

                    currentIntraday.Close = intervalMsg.Close;

                    currentIntraday.PeriodVolume += intervalMsg.PeriodVolume;
                    currentIntraday.PeriodTrade += intervalMsg.NumberOfTrades;

                    currentIntraday.TotalVolume = intervalMsg.TotalVolume;
                    currentIntraday.TotalTrade = totalTrade;

                    currentIntraday.Wap += (intervalMsg.High + intervalMsg.Low + intervalMsg.Close) / 3 * intervalMsg.PeriodVolume; // VWAP estimation

                    continue;
                }

                if (currentIntraday != null)
                {
                    if (intervalMsg.Timestamp.Date != currentIntraday.Timestamp.Date)     // reset the counts if dates differ
                    {
                        totalTrade = 0;
                    }

                    currentIntraday.Wap = currentIntraday.Wap / currentIntraday.PeriodVolume;
                    yield return currentIntraday;
                }

                totalTrade += intervalMsg.NumberOfTrades;

                var currentTimestamp = intervalMsg.Timestamp.Trim(intervalTicks);
                nextTimestamp = currentTimestamp.AddTicks(intervalTicks);

                currentIntraday = new HistoricalInterval()
                {
                    Timestamp = currentTimestamp,
                    Open = intervalMsg.Open,
                    High = intervalMsg.High,
                    Low = intervalMsg.Low,
                    Close = intervalMsg.Close,

                    PeriodVolume = intervalMsg.PeriodVolume,
                    PeriodTrade = intervalMsg.NumberOfTrades,

                    TotalVolume = intervalMsg.TotalVolume,
                    TotalTrade = totalTrade,

                    Wap = (intervalMsg.High + intervalMsg.Low + intervalMsg.Close) / 3 * intervalMsg.PeriodVolume   // VWAP estimation
                };
            }
        }

        private static IEnumerable<HistoricalInterval> ToHistoricalIntervalsDescending(
            this IEnumerable<IIntervalMessage> intervals, TimeSpan interval)
        {
            // TODO: implement algo
            return intervals.Reverse().ToHistoricalIntervalsAscending(interval);
        }

        /// <summary>
        /// Resample IntervalMessage in specific time window and filled missing values
        /// </summary>
        /// <param name="intervalMessages"></param>
        /// <param name="interval"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static IEnumerable<HistoricalInterval> ToHistoricalIntervals(
            this IEnumerable<IIntervalMessage> intervalMessages,
            TimeSpan interval,
            TimeSpan startTime,
            TimeSpan endTime,
            MessageDirection direction = MessageDirection.Descending)
        {
            return direction == MessageDirection.Descending ?
                intervalMessages.ToHistoricalIntervalsDescending(interval, startTime, endTime)
                : intervalMessages.ToHistoricalIntervalsAscending(interval, startTime, endTime);
        }

        private static IEnumerable<HistoricalInterval> ToHistoricalIntervalsAscending(
            this IEnumerable<IIntervalMessage> intervalMessages,
            TimeSpan interval,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            var enumerator = intervalMessages.GetEnumerator();
            if (!enumerator.MoveNext())
                goto Exit;

            var currentInterval = enumerator.Current;

            var currentTimestamp = currentInterval.Timestamp.Date.Add(startTime);
            var endTimestamp = currentInterval.Timestamp.Date.Add(endTime);
            var nextTime = currentTimestamp.Add(interval);

            HistoricalInterval previousHistorical = null;
            HistoricalInterval currentHistorical = new HistoricalInterval() { Timestamp = currentTimestamp };

            var processing = true;

            while (processing)
            {
                // day is over
                if (currentTimestamp >= endTimestamp)
                {
                    if (currentInterval == null)
                    {
                        processing = false;
                        continue;
                    }

                    // right outbound
                    if (currentInterval.Timestamp >= currentTimestamp &&
                        currentInterval.Timestamp.Date == currentTimestamp.Date)
                    {
                        previousHistorical = currentHistorical;
                        currentInterval = enumerator.MoveNext() ? enumerator.Current : null;
                        continue;
                    }

                    // reset to the next day
                    currentTimestamp = currentInterval.Timestamp.Date.Add(startTime);
                    endTimestamp = currentInterval.Timestamp.Date.Add(endTime);
                    nextTime = currentTimestamp.Add(interval);

                    currentHistorical = new HistoricalInterval() { Timestamp = currentTimestamp };
                    continue;
                }

                if (currentInterval == null)
                {
                    yield return currentHistorical;
                    goto Increment;
                }

                // in range
                if (currentInterval.Timestamp >= currentTimestamp && currentInterval.Timestamp < nextTime)
                {
                    if (currentHistorical.Open == 0)
                        currentHistorical.Open = currentInterval.Open;

                    if (currentInterval.High > currentHistorical.High || currentHistorical.High == 0)
                        currentHistorical.High = currentInterval.High;

                    if (currentInterval.Low < currentHistorical.Low || currentHistorical.Low == 0)
                        currentHistorical.Low = currentInterval.Low;

                    currentHistorical.Close = currentInterval.Close;

                    currentHistorical.PeriodVolume += currentInterval.PeriodVolume;
                    currentHistorical.TotalVolume = currentInterval.TotalVolume;

                    currentHistorical.Wap += (currentInterval.High + currentInterval.Low + currentInterval.Close) / 3 * currentInterval.PeriodVolume; // VWAP estimation

                    currentInterval = enumerator.MoveNext() ? enumerator.Current : null;

                    previousHistorical = currentHistorical;

                    continue;
                }

                // if currentInterval.Timestamp > currentTimestamp, advance and set to
                if (currentInterval.Timestamp > currentTimestamp)
                {
                    if (previousHistorical != null)
                    {
                        if (previousHistorical == currentHistorical)
                            currentHistorical.Wap = currentHistorical.Wap / currentHistorical.PeriodVolume;
                        else
                        {
                            currentHistorical.Open = previousHistorical.Close;
                            currentHistorical.High = previousHistorical.Close;
                            currentHistorical.Low = previousHistorical.Close;
                            currentHistorical.Close = previousHistorical.Close;
                            currentHistorical.Wap = previousHistorical.Close;
                            currentHistorical.TotalVolume = previousHistorical.TotalVolume;
                        }

                        yield return currentHistorical;
                    }
                }

                // left outbound
                else if (currentInterval.Timestamp < currentTimestamp)
                {
                    previousHistorical = currentHistorical;
                    currentInterval = enumerator.MoveNext() ? enumerator.Current : null;
                    continue;
                }

                Increment:
                currentTimestamp = nextTime;
                nextTime = currentTimestamp.Add(interval);

                currentHistorical = new HistoricalInterval()
                {
                    Timestamp = currentTimestamp
                };
            }

            Exit:
            enumerator.Dispose();
        }

        private static IEnumerable<HistoricalInterval> ToHistoricalIntervalsDescending(
            this IEnumerable<IIntervalMessage> intervalMessages,
            TimeSpan interval,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            // TODO: implement the algorithm for descending time
            return intervalMessages.Reverse().ToHistoricalIntervalsAscending(interval, startTime, endTime);
        } 
    }
}