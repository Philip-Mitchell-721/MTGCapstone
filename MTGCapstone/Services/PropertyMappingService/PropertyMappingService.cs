using MTGCapstone.API.Data.Models;

namespace MTGCapstone.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly Dictionary<string, PropertyMappingValue> _cardPropertyMapping
            = new(StringComparer.OrdinalIgnoreCase)
            {
                { "Name", new(new[] { "Name" }) }
            };

        private readonly IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
           // _propertyMappings.Add(new PropertyMapping<CardDto, Card>(_cardPropertyMapping));
        }


        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            // get matching mapping
            var matchingMapping = _propertyMappings
                .OfType<PropertyMapping<TSource, TDestination>>();

            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.First().MappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance " +
                $"for <{typeof(TSource)}, {typeof(TDestination)}");
        }
    }
}
