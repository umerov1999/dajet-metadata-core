using System.Data;

namespace DaJet.Data.Mapping
{
    public sealed class EntityMap
    {
        public List<PropertyMap> Properties { get; } = new();
        public PropertyMap MapProperty<T>(string name)
        {
            PropertyMap property = new()
            {
                Name = name,
                Type = typeof(T)
            };

            Properties.Add(property);

            return property;
        }
        public PropertyMap MapProperty(string name)
        {
            PropertyMap property = new()
            {
                Name = name
            };

            Properties.Add(property);

            return property;
        }
        public object Apply(IDataReader reader)
        {
            return new object();
        }
    }
}