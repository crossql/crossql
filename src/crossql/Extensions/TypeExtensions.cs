using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using crossql.Attributes;
using crossql.Helpers;

namespace crossql.Extensions
{
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type, string> _databaseTableNames = new Dictionary<Type, string>();
        private const string _defaultPrimaryKeyName = "Id";

        // todo: convert this to a PrimaryKeyFactory
        internal static readonly Dictionary<Type, string> PrimaryKeys = new Dictionary<Type, string>();

        public static void AddOrUpdatePrimaryKey(this Type type, string key)
        {
            if (PrimaryKeys.ContainsKey(type))
                PrimaryKeys[type] = key;
            else
                PrimaryKeys.Add(type, key);
        }

        public static string BuildTableName(this Type type)
        {
            if (_databaseTableNames.TryGetValue(type, out string tableName)) return tableName;

            var defined = type.GetCustomAttribute<TableNameAttribute>();
            if (defined != null)
            {
                _databaseTableNames[type] = defined.Name;
                return defined.Name;
            }

            var name = type.GetTypeInfo().Name;
            var clean = name.Replace("Model", string.Empty).Replace("Entity", string.Empty);
            var regexed = Regex.Replace(clean, @"\`\d", string.Empty);
            var result = regexed.Pluralize();

            _databaseTableNames[type] = result;
            return result;
        }

        public static string GetPrimaryKeyName(this Type type)
        {
            string identifierName;
            if (PrimaryKeys.ContainsKey(type))
            {
                identifierName = PrimaryKeys[type];
            }
            else
            {
                identifierName = type.GetRuntimeProperties()
                                     .FirstOrDefault(property => property.GetCustomAttributes(true).Any(a => a.GetType().Name == nameof(PrimaryKeyAttribute)))
                                     ?.Name ?? _defaultPrimaryKeyName;
                PrimaryKeys[type] = identifierName;
            }

            return identifierName;
        }
    }
}