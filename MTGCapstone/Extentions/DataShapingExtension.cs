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
            var expandoObjectList = new List<ExpandoObject>();

            /* create a list with PropertyInfo Objects on TSource.  Reflection is expensive,
             * so rather than doing it for each object in the list, we do it once and reuse the results.
             * After all, part of the reflection is on type of the object (TSource), not on the instance.*/
            var propertyInfoList = new List<PropertyInfo>();
            if (string.IsNullOrWhiteSpace(fields))
            {
                //all public properties should be in the ExpandoObject
                var propertyInfos = typeof(TSource).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                // Fields is a comma separated list, so split it.
                var splitFields = fields.Split(',');
                foreach (var field in splitFields)
                {
                    //trim after the split
                    var propertyName = field.Trim();
                    var propertyInfos = typeof(TSource).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
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
                var dataShapedObject = new ExpandoObject();
                foreach (var propertyInfo in propertyInfoList)
                {
                    // GetValue returns the value of the property on the source object
                    var propertyValue = propertyInfo.GetValue(sourceObject);

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

            var dataShapedObject = new ExpandoObject();

            if (string.IsNullOrWhiteSpace(fields))
            {
                //all properties because no fields
                var propertyInfos = typeof(TSource)
                    .GetProperties(BindingFlags.IgnoreCase | 
                                    BindingFlags.Public | 
                                    BindingFlags.Instance);
                foreach (var propertyInfo in propertyInfos)
                {
                    var propertyValue = propertyInfo.GetValue(source);

                    ((IDictionary<string, object?>)dataShapedObject)
                        .Add(propertyInfo.Name, propertyValue);
                }

                return dataShapedObject;

            }

            var fieldsAfterSplit = fields.Split(',');

            foreach (var field in fieldsAfterSplit)
            {
                var propertyName = field.Trim();

                var propertyInfo = typeof(TSource).GetProperty(propertyName,
                                    BindingFlags.IgnoreCase |
                                    BindingFlags.Public |
                                    BindingFlags.Instance);

                if (propertyInfo is null)
                {
                    throw new Exception($"Property {propertyName} wasn't found on {typeof(TSource)}");
                }
                
                var propertyValue = propertyInfo.GetValue(source);

                ((IDictionary<string, object?>)dataShapedObject)
                    .Add(propertyInfo.Name, propertyValue);
            }

            return dataShapedObject;
        }
    }
}
