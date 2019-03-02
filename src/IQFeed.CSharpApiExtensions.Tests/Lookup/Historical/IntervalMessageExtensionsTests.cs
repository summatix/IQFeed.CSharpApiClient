using System;
using System.Linq;
using IQFeed.CSharpApiClient.Lookup.Historical.Interfaces;
using IQFeed.CSharpApiExtensions.Lookup.Historical;
using NUnit.Framework;

namespace IQFeed.CSharpApiExtensions.Tests.Lookup.Historical
{
    public class IntervalMessageExtensionsTests
    {
        [Test]
        public void Should_Return_Empty_When_No_Interval()
        {
            // Arrange
            var emptyIntervalMessages = Enumerable.Empty<IIntervalMessage>();
            var interval = TimeSpan.FromMinutes(1);
            var startTime = new TimeSpan(9, 30, 0);
            var endTime = new TimeSpan(16, 0, 0);

            // Act
            var historicalIntervals = emptyIntervalMessages.ToHistoricalIntervals(interval, startTime, endTime);

            // Assert
            Assert.IsEmpty(historicalIntervals);
        }
    }
}