using System;

namespace IQFeed.CSharpApiClient.Lookup.Historical.Interfaces 
{
    public interface IHistoricalMessage 
    {
        DateTime Timestamp { get; }
    }
}