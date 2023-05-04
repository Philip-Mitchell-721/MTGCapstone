using MTGCapstone.API.Services;
using System.Linq.Dynamic.Core;

namespace MTGCapstone.API.Extentions
{
    public static class IQueryableExtentions
    {
        public static IQueryable<T> ApplySort<T>(
            this IQueryable<T> source,
            string orderBy,
            Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (mappingDictionary is null)
            {
                throw new ArgumentNullException(nameof(mappingDictionary));
            }
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return source;
            }

            string orderByString = string.Empty;

            // the orderBy string is separated by ",", so we split it.
            string[] orderByAfterSplit = orderBy.Split(',');

            // apply each orderby clause
            foreach (string orderByClause in orderByAfterSplit)
            {
                // trim the orderBy clause, as it might contain leading
                // or trailing spaces.  Can't trim the var in foreach,
                // so use another var.

                string trimmedOrderByClause = orderByClause.Trim();

                // if the sort option ends with " desc", we order
                // descending, otherwise ascending
                bool orderDescending = trimmedOrderByClause.EndsWith(" desc");

                // remove " asc" or " desc" from the orderByClause, so we 
                // get the property name to look for in the mapping dictionary
                int indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
                string propertyName = indexOfFirstSpace == -1 ? trimmedOrderByClause :
                    trimmedOrderByClause.Remove(indexOfFirstSpace);

                // find the matching property
                if (!mappingDictionary.ContainsKey(propertyName))
                {
                    throw new ArgumentNullException($"Key mapping for {propertyName} is missing");
                }

                // get the PrpertyMappingValue
                PropertyMappingValue? propertyMappingValue = mappingDictionary[propertyName];

                if (propertyMappingValue is null)
                {
                    throw new ArgumentNullException(nameof(propertyMappingValue));
                }

                // revert sort order if necessary
                if (propertyMappingValue.Revert)
                {
                    orderDescending = !orderDescending;
                }

                // Run through the property names
                foreach (string destinationPropery in propertyMappingValue.DestinationProperties)
                {
                    orderByString = orderByString
                        + (string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ", ")
                        + destinationPropery
                        + (orderDescending ? " descending" : " ascending");
                }
            }

            return source.OrderBy(orderByString);
        }
    }
}
