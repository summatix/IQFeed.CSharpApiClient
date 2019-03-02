using System;
using System.Collections.Generic;
using System.IO;

namespace IQFeed.CSharpApiClient.Lookup.Historical.Messages
{
    public abstract class HistoricalMessage
    {
        public static IEnumerable<T> ParseFromFile<T>(string path, Func<string, T> parserFunc)
        {
            using (var file = new StreamReader(path))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line) || line[0] == '!')
                        continue;

                    yield return parserFunc(line);
                }
            }
        }
    }
}