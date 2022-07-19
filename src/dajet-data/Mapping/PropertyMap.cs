using System.Data;

namespace DaJet.Data.Mapping
{
    public sealed class PropertyMap
    {
        public string Name { get; set; } = string.Empty;
        public Type Type { get; set; } = typeof(string);
        public EntityMap Value { get; set; } = new();
        public List<Column> Columns { get; } = new();
        public void ToColumn(string name, ColumnType type)
        {
            Columns.Add(new()
            {
                Name = name,
                Type = type
            });
        }
        public void ToColumns(List<Column> columns)
        {
            Columns.AddRange(columns);
        }
        public EntityMap ToEntity() // ???
        {
            Type = typeof(EntityMap);

            Value = new EntityMap();

            return Value;
        }
        public object? GetValue(IDataReader reader)
        {
            return null;
        }
    }
}