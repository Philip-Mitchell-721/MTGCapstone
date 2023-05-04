using System.Dynamic;
using System.Reflection;

namespace MTGCapstone.API.Extentions
{
    public static class DataShapingExtension
    {
        public static IEnumerable<ExpandoObject> ShapeData<TSource>(this IEnumerable<TSource> source, string? fields)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            //create a list to hold our ExpandoObjects
            List<ExpandoObject> expandoObjectList = new List<ExpandoObject>();

            /* create a list with PropertyInfo Objects on TSource.  Reflection is expensive,
             * so rather than doing it for each object in the list, we do it once and reuse the results.
             * After all, part of the reflection is on type of the object (TSource), not on the instance.*/
            List<PropertyInfo> propertyInfoList = new List<PropertyInfo>();
            if (string.IsNullOrWhiteSpace(fields))
            {
                //all public properties should be in the ExpandoObject
                PropertyInfo[] propertyInfos = typeof(TSource).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                // Fields is a comma separated list, so split it.
                string[] splitFields = fields.Split(',');
                foreach (string field in splitFields)
                {
                    //trim after the split
                    string propertyName = field.Trim();
                    PropertyInfo? propertyInfos = typeof(TSource).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (propertyInfos is null)
                    {
                        throw new Exception($"Property {propertyName} wasn't found on {typeof(TSource)}");
                    }
                    
                    propertyInfoList.Add(propertyInfos);
                    
                }

            }

            // run through the source objects
            foreach (TSource sourceObject in source)
            {
                //for each object of TSource, create an ExpandoObject that will be mapped to
                ExpandoObject dataShapedObject = new ExpandoObject();
                foreach (PropertyInfo propertyInfo in propertyInfoList)
                {
                    // GetValue returns the value of the property on the source object
                    object? propertyValue = propertyInfo.GetValue(sourceObject);

                    // add the field to the ExpandoObject
                    ((IDictionary<string, object?>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
                }

                // add the ExpandoObject to the list
                expandoObjectList.Add(dataShapedObject);

            }
            // return the list
            return expandoObjectList;
        }

        public static ExpandoObject ShapeData<TSource>(this TSource source, string? fields)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            ExpandoObject dataShapedObject = new ExpandoObject();

            if (string.IsNullOrWhiteSpace(fields))
            {
                //all properties because no fields
                PropertyInfo[] propertyInfos = typeof(TSource)
                    .GetProperties(BindingFlags.IgnoreCase | 
                                    BindingFlags.Public | 
                                    BindingFlags.Instance);
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    object? propertyValue = propertyInfo.GetValue(source);

                    ((IDictionary<string, object?>)dataShapedObject)
                        .Add(propertyInfo.Name, propertyValue);
                }

                return dataShapedObject;

            }

            string[] fieldsAfterSplit = fields.Split(',');

            foreach (string field in fieldsAfterSplit)
            {
                string propertyName = field.Trim();

                PropertyInfo? propertyInfo = typeof(TSource).GetProperty(propertyName,
                                    BindingFlags.IgnoreCase |
                                    BindingFlags.Public |
                                    BindingFlags.Instance);

                if (propertyInfo is null)
                {
                    throw new Exception($"Property {propertyName} wasn't found on {typeof(TSource)}");
                }

                object? propertyValue = propertyInfo.GetValue(source);

                ((IDictionary<string, object?>)dataShapedObject)
                    .Add(propertyInfo.Name, propertyValue);
            }

            return dataShapedObject;
        }
    }
}
