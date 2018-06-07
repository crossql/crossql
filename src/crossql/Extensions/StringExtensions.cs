using System.Text.RegularExpressions;
using crossql.Helpers;

namespace crossql.Extensions
{
    public static class StringExtensions
    {
        public static string BuildTableName(this string value)
        {
            var name = value.Replace("Model", "");
            name = Regex.Replace(name, @"\`\d", "");
            return name.Pluralize();
        }
    }
}
