namespace DaJet.Data.Mapping
{
    public sealed class Column
    {
        public Column() { }
        public Column(string name, ColumnType type)
        {
            Name = name;
            Type = type;
        }
        public string Name { get; set; } = string.Empty;
        public ColumnType Type { get; set; } = ColumnType.Pointer;
    }
}