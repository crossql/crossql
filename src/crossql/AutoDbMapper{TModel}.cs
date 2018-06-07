using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using crossql.Attributes;
using crossql.Extensions;

namespace crossql
{
    public class AutoDbMapper<TModel> : IDbMapper<TModel> where TModel : class, new()
    {
        private List<string> _fieldNames;
        private IList<PropertyInfo> _manyToOneProperties;
        private IList<PropertyInfo> _properties;

        private IEnumerable<PropertyInfo> ManyToOneProperties => _manyToOneProperties ?? (_manyToOneProperties = typeof(TModel)
                                                                     .GetRuntimeProperties()
                                                                     .Where(property => property.GetCustomAttributes(true).Any(a => a.GetType().Name == nameof(ManyToOneAttribute)))
                                                                     .ToList());

        private IEnumerable<PropertyInfo> Properties => _properties ?? (_properties = (from property in typeof(TModel).GetRuntimeProperties().OrderBy(p => p.Name)
                                                            let ignore = property.GetCustomAttributes(true).Any(
                                                                a =>
                                                                {
                                                                    // we use "Name" here because we want to allow the user to use their own attributes or attributes from other systems (EF) if necessary.
                                                                    var name = a.GetType().Name;
                                                                    return name == nameof(OneToManyAttribute) ||
                                                                           name == nameof(ManyToOneAttribute) ||
                                                                           name == nameof(ManyToManyAttribute) ||
                                                                           name == nameof(IgnoreAttribute) ||
                                                                           name == nameof(AutoIncrement);
                                                                })
                                                            where !ignore
                                                            select property).ToList());

        public virtual IDictionary<string, object> BuildDbParametersFrom(TModel model)
        {
            var dictionary = new Dictionary<string, object>();

            Properties.OrderBy(p => p.Name).ForEach(property =>
            {
                var value = property.GetValue(model);

                if (property.PropertyType == typeof(DateTime))
                    value = ((DateTime) value).GetDbSafeDate();
                else if (property.PropertyType == typeof(Guid))
                    if ((Guid) value == Guid.Empty)
                        value = null;
                    else
                        value = (Guid) value;

                dictionary.Add(property.Name, value);
            });

            AddManyToOneRecords(model, dictionary);

            return dictionary;
        }

        public virtual TModel BuildFrom(IDataReader reader)
        {
            if (!reader.Read()) return null;

            var model = new TModel();

            Properties.ForEach(property =>
            {
                var ordinal = reader.GetOrdinal(property.Name);
                if (reader.IsDBNull(ordinal)) return;

                SetPropertyValue(ref property, model, reader, ordinal);
            });

            ManyToOneProperties.ForEach(property =>
            {
                var propertyName = property.Name + "Id";
                var ordinal = reader.GetOrdinal(propertyName);

                if (!reader.IsDBNull(ordinal))
                {
                    var manyToOneObject = Activator.CreateInstance(property.PropertyType);
                    var idProperty = property.PropertyType.GetRuntimeProperty("Id");
                    SetPropertyValue(ref idProperty, manyToOneObject, reader, ordinal);

                    property.SetValue(model, manyToOneObject, null);
                }
            });

            return model;
        }

        public virtual IEnumerable<TModel> BuildListFrom(IDataReader reader)
        {
            var list = new List<TModel>();
            var model = BuildFrom(reader);

            while (model != null)
            {
                list.Add(model);
                model = BuildFrom(reader);
            }

            return list;
        }

        public virtual IList<string> FieldNames
        {
            get
            {
                if (_fieldNames == null)
                {
                    _fieldNames = Properties.Select(prop => prop.Name).ToList();
                    _fieldNames.AddRange(ManyToOneProperties.Select(prop => prop.Name + "Id"));
                }

                return _fieldNames;
            }
        }

        public virtual IEnumerable<TModel> BuildQueueFrom(IDataReader reader)
        {
            var queue = new Queue<TModel>();
            var model = BuildFrom(reader);

            while (model != null)
            {
                queue.Enqueue(model);
                model = BuildFrom(reader);
            }

            return queue;
        }

        private void AddManyToOneRecords(TModel model, IDictionary<string, object> dictionary) => ManyToOneProperties.ForEach(propertyInfo =>
        {
            var dbColumnName = propertyInfo.Name + "Id";
            var manyToOneObject = propertyInfo.GetValue(model);
            if (manyToOneObject == null)
            {
                if (!dictionary.ContainsKey(dbColumnName)) dictionary.Add(dbColumnName, null);
                return;
            }

            var manyToOneObjectType = manyToOneObject.GetType();
            var idPropertyInfo = manyToOneObjectType.GetRuntimeProperty("Id");
            var idValue = idPropertyInfo.GetValue(manyToOneObject, null);

            if (idPropertyInfo.PropertyType == typeof(Guid))
            {
                if ((Guid) idValue == Guid.Empty)
                    idValue = null;
                else
                    idValue = (Guid) idValue;
            }
            else if (idPropertyInfo.PropertyType == typeof(int))
            {
                if ((int) idValue == 0) idValue = null;
            }
            else if (idPropertyInfo.PropertyType == typeof(long))
            {
                if ((long) idValue == 0) idValue = null;
            }

            if (!dictionary.ContainsKey(dbColumnName))
                dictionary.Add(dbColumnName, idValue);
        });

        private static void SetPropertyValue(ref PropertyInfo property, object model, IDataReader reader, int ordinal)
        {
            if (property.PropertyType == typeof(Guid))
            {
                var result = reader.GetGuid(ordinal);
                property.SetValue(model, result, null);
            }
            else if (property.PropertyType == typeof(DateTime))
            {
                var result = reader.GetDateTime(ordinal);
                property.SetValue(model, result, null);
            }
            else if (property.PropertyType == typeof(bool))
            {
                var result = reader.GetBoolean(ordinal);
                property.SetValue(model, result, null);
            }
            else if (property.PropertyType == typeof(int))
            {
                var result = reader.GetInt32(ordinal);
                property.SetValue(model, result, null);
            }
            else if (property.PropertyType == typeof(short))
            {
                var result = reader.GetInt16(ordinal);
                property.SetValue(model, result, null);
            }
            else if (property.PropertyType == typeof(long))
            {
                var result = reader.GetInt64(ordinal);
                property.SetValue(model, result, null);
            }
            else
            {
                var result = reader[property.Name];
                property.SetValue(model, result, null);
            }
        }
    }
}