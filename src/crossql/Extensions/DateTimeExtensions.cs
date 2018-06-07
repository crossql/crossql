using System;
using crossql.Helpers;

namespace crossql.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime GetDbSafeDate(this DateTime dateTime) => dateTime < DateTimeHelper.MinSqlValue ? 
            DateTimeHelper.MinSqlValue : 
            dateTime;
    }
}