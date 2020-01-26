using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace Api.Helpers
{
    public static class ObjectExtensions
    {
        public static ExpandoObject ShapeData<TSource>(this TSource source, string fields)
        {
            if (source == null)
            {
                throw new ArgumentNullException($"Source");
            }

            var dataShappedObject = new ExpandoObject();


            if (string.IsNullOrWhiteSpace(fields))
            {
                var propertyInfos = typeof(TSource).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                foreach (var propertyInfo in propertyInfos)
                {
                    var propertyValue = propertyInfo.GetValue(source);
                    ((IDictionary<string, object>)dataShappedObject).Add(propertyInfo.Name, propertyValue);
                }

                return dataShappedObject;
            }

            var fieldsAfterSplit = fields.Split(',');

            foreach(var field in fieldsAfterSplit)
            {
                var propertyName = field.Trim();
                var propertyInfo = typeof(TSource).GetProperty(
                    propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null)
                {
                    throw new ArgumentNullException($"Property {propertyName} wan't found on {typeof(TSource)}");
                }

                var propertyValue = propertyInfo.GetValue(source);
                ((IDictionary<string, object>)dataShappedObject).Add(propertyInfo.Name, propertyValue);
            }

            return dataShappedObject;
        }
    }
}
