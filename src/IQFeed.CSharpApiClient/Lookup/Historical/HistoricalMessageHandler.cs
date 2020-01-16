using IQFeed.CSharpApiClient.Common;
using IQFeed.CSharpApiClient.Lookup.Common;
using IQFeed.CSharpApiClient.Lookup.Historical.Messages;

namespace IQFeed.CSharpApiClient.Lookup.Historical
{
    public class HistoricalMessageHandler<T> : BaseLookupMessageHandler
    {
        public MessageContainer<TickMessage<T>> GetTickMessages(byte[] message, int count)
        {
            return ProcessMessages(TickMessage<T>.Parse, ParseErrorMessage, message, count);
        }

        public MessageContainer<TickMessage<T>> GetTickMessagesWithRequestId(byte[] message, int count)
        {
            return ProcessMessages(TickMessage<T>.ParseWithRequestId, ParseErrorMessageWithRequestId, message, count);
        }

        public MessageContainer<IntervalMessage> GetIntervalMessages(byte[] message, int count)
        {
            return ProcessMessages(IntervalMessage.Parse, ParseErrorMessage, message, count);
        }

        public MessageContainer<IntervalMessage> GetIntervalMessagesWithRequestId(byte[] message, int count)
        {
            return ProcessMessages(IntervalMessage.ParseWithRequestId, ParseErrorMessageWithRequestId, message, count);
        }

        public MessageContainer<DailyWeeklyMonthlyMessage> GetDailyWeeklyMonthlyMessages(byte[] message, int count)
        {
            return ProcessMessages(DailyWeeklyMonthlyMessage.Parse, ParseErrorMessage, message, count);
        }
        public MessageContainer<DailyWeeklyMonthlyMessage> GetDailyWeeklyMonthlyMessagesWithRequestId(byte[] message, int count)
        {
            return ProcessMessages(DailyWeeklyMonthlyMessage.ParseWithRequestId, ParseErrorMessageWithRequestId, message, count);
        }
    }
}