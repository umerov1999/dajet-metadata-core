using System.Data;

namespace DaJet.Data.Mapping
{
    public sealed class DataMapper
    {
        public List<object> Map(IDataReader reader, EntityMap map)
        {
            List<object> result = new();

            map = new EntityMap();
            
            map.MapProperty<EntityRef>("Register")
                .ToColumns(new()
                {
                    new("_Fld123TRef", ColumnType.TypeCode),
                    new("_Fld123RRef", ColumnType.Object)
                });
            
            map.MapProperty<DateTime>("DateTime")
                .ToColumn("_Period", ColumnType.DateTime);

            while (reader.Read())
            {
                object entity = map.Apply(reader);

                if (entity != null)
                {
                    result.Add(entity);
                }
            }

            return result;
        }
    }
}