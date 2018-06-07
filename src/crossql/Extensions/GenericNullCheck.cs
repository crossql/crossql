using System.Collections.Generic;

namespace crossql.Extensions
{
    public static class GenericNullValueComparer
    {
        public static bool IsNull<T>(this T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default(T));
        }
    }
}