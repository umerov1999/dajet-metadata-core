using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System.Data;
using System.Text;

namespace DaJet.Data.Mapping
{
    public sealed class EntityDataMapper : IDaJetDataMapper
    {
        private readonly int _yearOffset = 0;
        private readonly MetadataService _metadataService;
        private readonly ApplicationObject _entity;
        private readonly Dictionary<MetadataProperty, PropertyDataMapper> _propertyMappers = new();
        public EntityDataMapper(MetadataService metadata, int yearOffset, ApplicationObject entity)
        {
            _entity = entity;
            _yearOffset = yearOffset;
            _metadataService = metadata;

            ConfigurePropertyDataMappers();
        }
        private void ConfigurePropertyDataMappers()
        {
            _propertyMappers.Clear();

            int ordinal = 0;

            for (int i = 0; i < _entity.Properties.Count; i++)
            {
                MetadataProperty property = _entity.Properties[i];

                //if (Options.IgnoreProperties.Contains(property.Name))
                //{
                //    continue;
                //}

                _propertyMappers.Add(property, new PropertyDataMapper(in property, ref ordinal));
            }
        }
        public Dictionary<string, object> Select(Guid uuid_sql)
        {
            IQueryExecutor executor = _metadataService.CreateQueryExecutor();

            string script = GetSelectEntityByUuidScript();

            Dictionary<string, object> parameters = new()
            {
                { "identity", uuid_sql.ToByteArray() }
            };

            Dictionary<string, object> entity = new();

            foreach (IDataReader reader in executor.ExecuteReader(script, 10, parameters))
            {
                for (int f = 0; f < reader.FieldCount; f++)
                {
                    entity.Add(reader.GetName(f), reader[f]);
                }
            }

            return entity;
        }

        #region "SELECT ENTITY BY REFERENCE (UUID)"

        private string SELECT_ENTITY_BY_UUID_SCRIPT;
        private string GetSelectEntityByUuidScript()
        {
            if (SELECT_ENTITY_BY_UUID_SCRIPT == null)
            {
                SELECT_ENTITY_BY_UUID_SCRIPT = BuildSelectEntityScript() + " WHERE _IDRRef = @identity;";
            }
            return SELECT_ENTITY_BY_UUID_SCRIPT;
        }
        private string BuildSelectEntityScript(string tableAlias = null!)
        {
            StringBuilder script = new("SELECT ");

            bool first = true;

            foreach (var item in _propertyMappers)
            {
                string propertyScript = item.Value.BuildSelectScript(item.Key, in tableAlias);

                if (string.IsNullOrWhiteSpace(propertyScript))
                {
                    continue;
                }

                if (!first) { script.Append(", "); }

                script.Append(propertyScript);

                first = false;
            }

            script.Append($" FROM {_entity.TableName}");

            if (!string.IsNullOrEmpty(tableAlias))
            {
                script.Append($" AS {tableAlias}");
            }

            return script.ToString();
        }

        #endregion

        //private void ConfigureTablePartDataMappers()
        //{
        //    Options.TablePartMappers.Clear();

        //    foreach (TablePart table in Options.MetaObject.TableParts)
        //    {
        //        if (Options.IgnoreProperties.Contains(table.Name))
        //        {
        //            continue;
        //        }

        //        EntityDataMapper mapper = new EntityDataMapper();
        //        mapper.Configure(new DataMapperOptions()
        //        {
        //            InfoBase = Options.InfoBase,
        //            MetaObject = table,
        //            Provider = Options.Provider,
        //            ConnectionString = Options.ConnectionString,
        //            IgnoreProperties = new List<string>()
        //            {
        //                "Ссылка",
        //                "КлючСтроки",
        //                "НомерСтроки"
        //            }
        //        });

        //        Options.TablePartMappers.Add(mapper);
        //    }
        //}

        #region "OLD CODE"

        //private string SELECT_ENTITY_COUNT_SCRIPT = string.Empty;
        //private string SELECT_ENTITY_PAGING_SCRIPT = string.Empty;
        //private string SELECT_ENTITY_TABLE_PART_SCRIPT = string.Empty;

        //public DataMapperOptions Options { get; private set; }
        //private struct PropertyOrdinal
        //{
        //    internal int Ordinal;
        //    internal MetadataProperty Property;
        //}
        //private readonly Dictionary<string, int> CatalogPropertyOrder = new Dictionary<string, int>()
        //{
        //    { "ЭтоГруппа",        0 }, // IsFolder           - bool (invert)
        //    { "Ссылка",           1 }, // Ref                - uuid
        //    { "ПометкаУдаления",  2 }, // DeletionMark       - bool
        //    { "Владелец",         3 }, // Owner              - { #type + #value }
        //    { "Родитель",         4 }, // Parent             - uuid
        //    { "Код",              5 }, // Code               - string | number
        //    { "Наименование",     6 }, // Description        - string
        //    { "Предопределённый", 7 }  // PredefinedDataName - string
        //};
        //private readonly Dictionary<string, int> DocumentPropertyOrder = new Dictionary<string, int>()
        //{
        //    { "Ссылка",           0 }, // Ref                - uuid
        //    { "ПометкаУдаления",  1 }, // DeletionMark       - bool
        //    { "Дата",             2 }, // Date               - DateTime
        //    { "Номер",            3 }, // Number             - string | number
        //    { "Проведён",         4 }  // Posted             - bool
        //};

        //public void Configure(DataMapperOptions options)
        //{
        //    Options = options;
        //    ConfigureDataMapper();
        //}
        //public void Reconfigure()
        //{
        //    SELECT_ENTITY_COUNT_SCRIPT = string.Empty;
        //    SELECT_ENTITY_PAGING_SCRIPT = string.Empty;
        //    // SELECT_ENTITY_TABLE_PART_SCRIPT is not changing !
        //}


        //private void GetValue()
        //{
        //    if (_yearOffset > 0)
        //    {
        //        DateTime dateTime = dateTime.AddYears(-_yearOffset);
        //    }

        //    if (Enumeration != null)
        //    {
        //        return new EntityRef(Property.PropertyType.ReferenceTypeCode, uuid,
        //            CONST_TYPE_ENUM + "." + Enumeration.Name, GetEnumValue(Enumeration, uuid));
        //    }
        //}
        //private EntityRef GetEntityRef(int typeCode, Guid uuid)
        //{
        //    if (InfoBase.ReferenceTypeCodes.TryGetValue(typeCode, out ApplicationObject metaObject))
        //    {
        //        if (metaObject is Enumeration enumeration)
        //        {
        //            return new EntityRef(typeCode, uuid, CONST_TYPE_ENUM + "." + enumeration.Name, GetEnumValue(enumeration, uuid));
        //        }
        //        else if (metaObject is Catalog)
        //        {
        //            return new EntityRef(typeCode, uuid, CONST_TYPE_CATALOG + "." + metaObject.Name);
        //        }
        //        else if (metaObject is Document)
        //        {
        //            return new EntityRef(typeCode, uuid, CONST_TYPE_DOCUMENT + "." + metaObject.Name);
        //        }
        //        else if (metaObject is Publication)
        //        {
        //            return new EntityRef(typeCode, uuid, CONST_TYPE_EXCHANGE_PLAN + "." + metaObject.Name);
        //        }
        //        else if (metaObject is Characteristic)
        //        {
        //            return new EntityRef(typeCode, uuid, CONST_TYPE_CHARACTERISTIC + "." + metaObject.Name);
        //        }
        //    }

        //    return null; // unknown type code - this should not happen
        //}
        //private EntityRef GetEntityRef(MetadataProperty property, Guid uuid)
        //{
        //    if (!property.PropertyType.CanBeReference || property.PropertyType.ReferenceTypeUuid == Guid.Empty)
        //    {
        //        return null;
        //    }

        //    if (property.PropertyType.ReferenceTypeCode != 0)
        //    {
        //        return GetEntityRef(property.PropertyType.ReferenceTypeCode, uuid);
        //    }

        //    // TODO: ReferenceTypeCode == 0 this should be fixed in DaJet.Metadata library

        //    if (InfoBase.ReferenceTypeUuids.TryGetValue(property.PropertyType.ReferenceTypeUuid, out ApplicationObject propertyType))
        //    {
        //        property.PropertyType.ReferenceTypeCode = propertyType.TypeCode; // patch metadata

        //        if (propertyType is Enumeration enumeration)
        //        {
        //            return new EntityRef(propertyType.TypeCode, uuid, CONST_TYPE_ENUM + "." + enumeration.Name, GetEnumValue(enumeration, uuid));
        //        }
        //        else if (propertyType is Catalog)
        //        {
        //            return new EntityRef(propertyType.TypeCode, uuid, CONST_TYPE_CATALOG + "." + propertyType.Name);
        //        }
        //        else if (propertyType is Document)
        //        {
        //            return new EntityRef(propertyType.TypeCode, uuid, CONST_TYPE_DOCUMENT + "." + propertyType.Name);
        //        }
        //        else if (propertyType is Publication)
        //        {
        //            return new EntityRef(propertyType.TypeCode, uuid, CONST_TYPE_EXCHANGE_PLAN + "." + propertyType.Name);
        //        }
        //        else if (propertyType is Characteristic)
        //        {
        //            return new EntityRef(propertyType.TypeCode, uuid, CONST_TYPE_CHARACTERISTIC + "." + propertyType.Name);
        //        }
        //    }

        //    if (property.Name == "Владелец")
        //    {
        //        if (MetaObject is Catalog || MetaObject is Characteristic)
        //        {
        //            // TODO: this issue should be fixed in DaJet.Metadata library
        //            // NOTE: file names lookup - Property.PropertyType.ReferenceTypeUuid for Owner property is a FileName, not metadata object Uuid !!!
        //            if (InfoBase.Catalogs.TryGetValue(property.PropertyType.ReferenceTypeUuid, out ApplicationObject catalog))
        //            {
        //                property.PropertyType.ReferenceTypeCode = catalog.TypeCode; // patch metadata
        //                return GetEntityRef(property.PropertyType.ReferenceTypeCode, uuid);
        //            }
        //            else if (InfoBase.Characteristics.TryGetValue(property.PropertyType.ReferenceTypeUuid, out ApplicationObject characteristic))
        //            {
        //                property.PropertyType.ReferenceTypeCode = characteristic.TypeCode; // patch metadata
        //                return GetEntityRef(property.PropertyType.ReferenceTypeCode, uuid);
        //            }
        //        }
        //    }

        //    return new EntityRef(property.PropertyType.ReferenceTypeCode, uuid);
        //}
        //private string GetEnumValue(Enumeration enumeration, Guid value)
        //{
        //    for (int i = 0; i < enumeration.Values.Count; i++)
        //    {
        //        if (enumeration.Values[i].Uuid == value)
        //        {
        //            return enumeration.Values[i].Name;
        //        }
        //    }
        //    return string.Empty;
        //}

        //private void ConfigureDataMapper()
        //{
        //    if (Options.MetaObject == null)
        //    {
        //        Options.MetaObject = Options.InfoBase.GetApplicationObjectByName(Options.MetadataName);
        //    }

        //    if (Options.MetaObject is Catalog)
        //    {
        //        Options.IgnoreProperties = new List<string>()
        //        {
        //            "ВерсияДанных"
        //        };
        //        if (Options.InfoBase.PlatformRequiredVersion < 80300)
        //        {
        //            Options.IgnoreProperties.Add("Предопределённый");
        //        }
        //    }
        //    else if (Options.MetaObject is Document)
        //    {
        //        Options.IgnoreProperties = new List<string>()
        //        {
        //            "ВерсияДанных",
        //            "ПериодНомера"
        //        };
        //    }

        //    if (Options.MetaObject is Catalog catalog)
        //    {
        //        OrderCatalogSystemProperties(catalog);
        //    }
        //    else if (Options.MetaObject is Document document)
        //    {
        //        OrderDocumentSystemProperties(document);
        //    }

        //    ConfigurePropertyDataMappers();
        //    ConfigureTablePartDataMappers();
        //}
        //private void OrderCatalogSystemProperties(Catalog catalog)
        //{
        //    List<PropertyOrdinal> ordinals = new List<PropertyOrdinal>();

        //    int i = 0;
        //    while (i < catalog.Properties.Count)
        //    {
        //        if (catalog.Properties[i].Purpose == PropertyPurpose.System)
        //        {
        //            if (CatalogPropertyOrder.TryGetValue(catalog.Properties[i].Name, out int ordinal))
        //            {
        //                ordinals.Add(new PropertyOrdinal()
        //                {
        //                    Ordinal = ordinal,
        //                    Property = catalog.Properties[i]
        //                });
        //                catalog.Properties.RemoveAt(i);
        //            }
        //            else
        //            {
        //                i++;
        //            }
        //        }
        //        else
        //        {
        //            i++;
        //        }
        //    }

        //    IEnumerable<MetadataProperty> ordered = ordinals
        //        .OrderBy(item => item.Ordinal)
        //        .Select(item=>item.Property);

        //    catalog.Properties.InsertRange(0, ordered);
        //}
        //private void OrderDocumentSystemProperties(Document document)
        //{
        //    List<PropertyOrdinal> ordinals = new List<PropertyOrdinal>();

        //    int i = 0;
        //    while (i < document.Properties.Count)
        //    {
        //        if (document.Properties[i].Purpose == PropertyPurpose.System)
        //        {
        //            if (DocumentPropertyOrder.TryGetValue(document.Properties[i].Name, out int ordinal))
        //            {
        //                ordinals.Add(new PropertyOrdinal()
        //                {
        //                    Ordinal = ordinal,
        //                    Property = document.Properties[i]
        //                });
        //                document.Properties.RemoveAt(i);
        //            }
        //            else
        //            {
        //                i++;
        //            }
        //        }
        //        else
        //        {
        //            i++;
        //        }
        //    }

        //    IEnumerable<MetadataProperty> ordered = ordinals
        //        .OrderBy(item => item.Ordinal)
        //        .Select(item => item.Property);

        //    document.Properties.InsertRange(0, ordered);
        //}

        //public int GetTotalRowCount()
        //{
        //    int rowCount = 0;

        //    using (SqlConnection connection = new SqlConnection(Options.ConnectionString))
        //    {
        //        connection.Open();

        //        using (SqlCommand command = connection.CreateCommand())
        //        {
        //            command.CommandType = CommandType.Text;
        //            command.CommandText = GetTotalRowCountScript();
        //            command.CommandTimeout = Options.CommandTimeout; // seconds

        //            ConfigureQueryParameters(command, Options.Filter);

        //            rowCount = (int)command.ExecuteScalar();
        //        }
        //    }

        //    return rowCount;
        //}
        //public EntityRef GetEntityRef(IDataReader reader)
        //{
        //    for (int i = 0; i < PropertyMappers.Count; i++)
        //    {
        //        if (PropertyMappers[i].Property.Name == "Ссылка")
        //        {
        //            return new EntityRef(Options.MetaObject.TypeCode, (Guid)PropertyMappers[i].GetValue(reader));
        //        }
        //    }
        //    return null;
        //}
        //public bool GetIsFolder(IDataReader reader)
        //{
        //    for (int i = 0; i < PropertyMappers.Count; i++)
        //    {
        //        if (PropertyMappers[i].Property.Name == "ЭтоГруппа")
        //        {
        //            return (bool)PropertyMappers[i].GetValue(reader);
        //        }
        //    }
        //    return false;
        //}
        //public string GetPredefinedDataName(IDataReader reader, Guid uuid)
        //{
        //    if (Options.MetaObject is IPredefinedValues predefined)
        //    {
        //        foreach (PredefinedValue value in predefined.PredefinedValues)
        //        {
        //            if (value.Uuid == uuid)
        //            {
        //                return value.Name;
        //            }
        //        }
        //    }
        //    return null;
        //}
        //public long TestGetPageDataRows(int size, int page)
        //{
        //    Stopwatch watcher = new Stopwatch();

        //    watcher.Start();

        //    using (SqlConnection connection = new SqlConnection(Options.ConnectionString))
        //    {
        //        connection.Open();

        //        using (SqlCommand command = connection.CreateCommand())
        //        {
        //            command.CommandType = CommandType.Text;
        //            command.CommandText = GetSelectEntityPagingScript();
        //            command.CommandTimeout = Options.CommandTimeout; // seconds
        //            command.Parameters.AddWithValue("PageSize", size);
        //            command.Parameters.AddWithValue("PageNumber", page);

        //            ConfigureQueryParameters(command, Options.Filter);

        //            using (SqlDataReader reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    // do nothing ¯\_(ツ)_/¯
        //                }
        //                reader.Close();
        //            }
        //        }
        //    }

        //    watcher.Stop();

        //    return watcher.ElapsedMilliseconds;
        //}
        //public IEnumerable<IDataReader> GetPageDataRows(int size, int page)
        //{
        //    using (SqlConnection connection = new SqlConnection(Options.ConnectionString))
        //    {
        //        connection.Open();

        //        using (SqlCommand command = connection.CreateCommand())
        //        {
        //            command.CommandType = CommandType.Text;
        //            command.CommandText = GetSelectEntityPagingScript();
        //            command.CommandTimeout = Options.CommandTimeout; // seconds
        //            command.Parameters.AddWithValue("PageSize", size);
        //            command.Parameters.AddWithValue("PageNumber", page);

        //            ConfigureQueryParameters(command, Options.Filter);

        //            using (SqlDataReader reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    yield return reader;
        //                }
        //                reader.Close();
        //            }
        //        }
        //    }
        //}
        //public IEnumerable<IDataReader> GetTablePartDataRows(EntityRef entity)
        //{
        //    using (DbConnection connection = GetDbConnection())
        //    {
        //        connection.Open();

        //        using (DbCommand command = connection.CreateCommand())
        //        {
        //            command.CommandType = CommandType.Text;
        //            command.CommandText = GetSelectTablePartScript();
        //            command.CommandTimeout = Options.CommandTimeout; // seconds

        //            if (command is SqlCommand ms_command)
        //            {
        //                ms_command.Parameters.AddWithValue("entity", SQLHelper.GetSqlUuid(entity.Identity));
        //            }
        //            else if (command is NpgsqlCommand pg_command)
        //            {
        //                pg_command.Parameters.AddWithValue("entity", SQLHelper.GetSqlUuid(entity.Identity));
        //            }

        //            using (DbDataReader reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    yield return reader;
        //                }
        //                reader.Close();
        //            }
        //        }
        //    }
        //}
        /////<summary>The method is correlated with this one <see cref="BuildWhereClause"/></summary>
        //private void ConfigureQueryParameters(SqlCommand command, List<FilterParameter> filter)
        //{
        //    if (Options.Filter == null || Options.Filter.Count == 0)
        //    {
        //        return;
        //    }

        //    for (int p = 0; p < Options.Filter.Count; p++)
        //    {
        //        FilterParameter parameter = Options.Filter[p];

        //        object value = parameter.Value;

        //        if (value is DateTime dateTime)
        //        {
        //            value = dateTime.AddYears(Options.InfoBase.YearOffset);
        //        }

        //        command.Parameters.AddWithValue($"p{p}", value);
        //    }
        //}



        //public string GetTotalRowCountScript()
        //{
        //    if (string.IsNullOrEmpty(SELECT_ENTITY_COUNT_SCRIPT))
        //    {
        //        StringBuilder script = new StringBuilder();

        //        script.Append($"SELECT COUNT(*) FROM {Options.MetaObject.TableName} WITH(NOLOCK)");

        //        if (Options.Filter != null && Options.Filter.Count > 0)
        //        {
        //            script.Append($" WHERE {BuildWhereClause(Options.Filter)}");
        //        }

        //        script.Append(";");

        //        SELECT_ENTITY_COUNT_SCRIPT = script.ToString();
        //    }

        //    return SELECT_ENTITY_COUNT_SCRIPT;
        //}

        //public string GetSelectEntityPagingScript()
        //{
        //    if (string.IsNullOrEmpty(SELECT_ENTITY_PAGING_SCRIPT))
        //    {
        //        if (Options.Index == null)
        //        {
        //            // default - use of clustered index
        //            SELECT_ENTITY_PAGING_SCRIPT = BuildSelectEntityPagingScript();
        //        }
        //        else
        //        {
        //            // custom - use of selected by user index
        //            SELECT_ENTITY_PAGING_SCRIPT = BuildSelectEntityPagingScript(Options.Index, Options.Filter);
        //        }
        //    }

        //    return SELECT_ENTITY_PAGING_SCRIPT;
        //}

        //public string BuildSelectEntityPagingScript()
        //{
        //    StringBuilder script = new StringBuilder();

        //    script.Append("WITH cte AS ");
        //    script.Append($"(SELECT _IDRRef FROM {Options.MetaObject.TableName} ORDER BY _IDRRef ASC ");
        //    script.Append("OFFSET @PageSize * (@PageNumber - 1) ROWS ");
        //    script.Append("FETCH NEXT @PageSize ROWS ONLY) ");
        //    script.Append(BuildSelectEntityScript("t"));
        //    script.Append(" INNER JOIN cte ON t._IDRRef = cte._IDRRef;");

        //    return script.ToString();
        //}
        //public string BuildSelectEntityPagingScript(IndexInfo index, List<FilterParameter> filter = null)
        //{
        //    StringBuilder script = new StringBuilder();

        //    script.Append("WITH cte AS ");
        //    script.Append($"(SELECT _IDRRef FROM {Options.MetaObject.TableName} ");
        //    if (filter != null && filter.Count > 0)
        //    {
        //        script.Append($"WHERE {BuildWhereClause(filter)} ");
        //    }
        //    script.Append($"ORDER BY {BuildOrderByClause(index)} ");
        //    script.Append("OFFSET @PageSize * (@PageNumber - 1) ROWS ");
        //    script.Append("FETCH NEXT @PageSize ROWS ONLY) ");
        //    script.Append(BuildSelectEntityScript("t"));
        //    script.Append(" INNER JOIN cte ON ");
        //    script.Append(BuildJoinOnClause(index));
        //    script.Append(";");

        //    return script.ToString();
        //}
        //private string BuildOrderByClause(IndexInfo index)
        //{
        //    StringBuilder clause = new StringBuilder();

        //    foreach (IndexColumnInfo column in index.Columns)
        //    {
        //        if (clause.Length > 0)
        //        {
        //            clause.Append(", ");
        //        }
        //        clause.Append($"{column.Name} {(column.IsDescending ? "DESC" : "ASC")}");
        //    }

        //    return clause.ToString();
        //}
        //private string BuildJoinOnClause(IndexInfo index)
        //{
        //    return "t._IDRRef = cte._IDRRef"; // TODO: use clustered index info
        //}
        //private string BuildWhereClause(List<FilterParameter> filter)
        //{
        //    if (filter != null && filter.Count == 0)
        //    {
        //        return string.Empty;
        //    }

        //    StringBuilder clause = new StringBuilder();

        //    for (int p = 0; p < filter.Count; p++)
        //    {
        //        FilterParameter parameter = filter[p];

        //        string fieldName = GetDatabaseFieldByPath(parameter.Path);
        //        string _operator = GetComparisonOperatorSymbol(parameter.Operator);

        //        if (clause.Length > 0)
        //        {
        //            clause.Append(" AND ");
        //        }

        //        clause.Append($"{fieldName} {_operator} @p{p}");
        //    }

        //    return clause.ToString();
        //}
        //private string GetDatabaseFieldByPath(string path)
        //{
        //    // TODO: multi-part path (example: Регистратор.Дата)
        //    foreach (MetadataProperty property in Options.MetaObject.Properties)
        //    {
        //        if (property.Name == path)
        //        {
        //            if (property.Fields.Count > 0)
        //            {
        //                return property.Fields[0].Name;
        //            }
        //        }
        //    }
        //    return string.Empty;
        //}
        //private string GetComparisonOperatorSymbol(ComparisonOperator comparisonOperator)
        //{
        //    if (comparisonOperator == ComparisonOperator.Equal) return "=";
        //    else if (comparisonOperator == ComparisonOperator.NotEqual) return "<>";
        //    else if (comparisonOperator == ComparisonOperator.Less) return "<";
        //    else if (comparisonOperator == ComparisonOperator.LessOrEqual) return "<=";
        //    else if (comparisonOperator == ComparisonOperator.Greater) return ">";
        //    else if (comparisonOperator == ComparisonOperator.GreaterOrEqual) return ">=";
        //    else if (comparisonOperator == ComparisonOperator.Contains) return "IN";
        //    else if (comparisonOperator == ComparisonOperator.Between) return "BETWEEN";

        //    throw new ArgumentOutOfRangeException(nameof(comparisonOperator));
        //}

        //public string GetSelectTablePartScript()
        //{
        //    if (string.IsNullOrEmpty(SELECT_ENTITY_TABLE_PART_SCRIPT))
        //    {
        //        StringBuilder script = new StringBuilder();

        //        MetadataProperty property = Options.MetaObject.Properties.Where(p => p.Name == "Ссылка").FirstOrDefault();
        //        DatabaseField field = property.Fields[0];

        //        script.Append(BuildSelectEntityScript(null));
        //        script.Append($" WHERE {field.Name} = @entity ");

        //        if (Options.Provider == DatabaseProvider.SQLServer)
        //        {
        //            script.Append($"ORDER BY {field.Name} ASC, _KeyField ASC;");
        //        }
        //        else
        //        {
        //            script.Append($"ORDER BY {field.Name} ASC, _keyfield ASC;");
        //        }

        //        SELECT_ENTITY_TABLE_PART_SCRIPT = script.ToString();
        //    }

        //    return SELECT_ENTITY_TABLE_PART_SCRIPT;
        //}

        #endregion
    }
}