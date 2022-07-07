using DaJet.Metadata.Model;
using System.ComponentModel;

namespace DaJet.Data.Mapping
{
    public sealed class DataMapperOptions
    {
        public InfoBase InfoBase { get; set; }
        public ApplicationObject Entity { get; set; }
        public List<FilterParameter> Filter { get; set; } = new();
        public List<string> IgnoreProperties { get; set; } = new();
    }
    public sealed class FilterParameter
    {
        public FilterParameter() { }
        public FilterParameter(string name, object? value)
        {
            Name = name;
            Value = value!;
        }
        public FilterParameter(string name, object? value, ComparisonOperator _operator) : this(name, value)
        {
            Operator = _operator;
        }
        public string Name { get; set; } = string.Empty;
        public object Value { get; set; } = null!;
        public ComparisonOperator Operator { get; set; } = ComparisonOperator.Equal;
    }
    public enum ComparisonOperator
    {
        [Description("=")] Equal,
        [Description("<>")] NotEqual,
        [Description("IN")] Contains,
        [Description(">")] Greater,
        [Description(">=")] GreaterOrEqual,
        [Description("<")] Less,
        [Description("<=")] LessOrEqual,
        [Description("BETWEEN")] Between
    }
}