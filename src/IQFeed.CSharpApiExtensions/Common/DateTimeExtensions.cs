﻿using System;

namespace IQFeed.CSharpApiExtensions.Common
{
    public static class DateTimeExtensions
    {
        public static DateTime Trim(this DateTime date, long roundTicks)
        {
            return new DateTime(date.Ticks - date.Ticks % roundTicks, date.Kind);
        }
    }
}