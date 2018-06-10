using System.Text.RegularExpressions;
using crossql.Helpers;

namespace crossql.Extensions
{
    public static class StringExtensions
    {
        public static string BuildTableName(this string value)
        {
            var name = value.Replace("Model", string.Empty).Replace("Entity",string.Empty);
            name = Regex.Replace(name, @"\`\d", string.Empty);
            return name.Pluralize();
        }
    }
}
