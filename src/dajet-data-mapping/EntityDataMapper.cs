using DaJet.Metadata.Model;
using System.Data;
using System.Text;

namespace DaJet.Data.Mapping
{
    public sealed class EntityDataMapper : IDaJetDataMapper
    {
        private readonly IQueryExecutor _executor;
        private readonly DataMapperOptions _options;
        private readonly Dictionary<string, object> _parameters = new();
        private readonly Dictionary<MetadataProperty, PropertyDataMapper> _mappers = new();
        private string SELECT_SCRIPT = string.Empty;
        public EntityDataMapper(DataMapperOptions options, IQueryExecutor executor)
        {
            _options = options;
            _executor = executor;

            Configure();
        }
        public DataMapperOptions Options { get { return _options; } }
        public void Configure()
        {
            ConfigurePropertyDataMappers();
            
            ConfigureSelectQueryScript();

            ConfigureSelectQueryParameters();
        }
        private void ConfigurePropertyDataMappers()
        {
            _mappers.Clear();

            int ordinal = 0;

            for (int i = 0; i < _options.Entity.Properties.Count; i++)
            {
                MetadataProperty property = _options.Entity.Properties[i];

                if (_options.IgnoreProperties.Contains(property.Name))
                {
                    continue;
                }

                _mappers.Add(property, new PropertyDataMapper(in property, ref ordinal));
            }
        }
        private void ConfigureSelectQueryScript()
        {
            SELECT_SCRIPT = BuildSelectScript();

            if (_options.Filter.Count > 0)
            {
                SELECT_SCRIPT += (" " + BuildWhereClause());
            }

            SELECT_SCRIPT += ";";
        }
        private void ConfigureSelectQueryParameters()
        {
            _parameters.Clear();

            for (int p = 0; p < _options.Filter.Count; p++)
            {
                FilterParameter parameter = _options.Filter[p];

                object value = parameter.Value;

                if (value == null)
                {
                    value = DBNull.Value;
                }
                if (value is DateTime dateTime)
                {
                    value = dateTime.AddYears(_options.InfoBase.YearOffset);
                }
                else if (value is Guid uuid)
                {
                    value = SQLHelper.GetSqlUuid(uuid.ToByteArray());
                }

                _parameters.Add($"p{p}", value);
            }
        }

        public List<Dictionary<string, object>> Select()
        {
            List<Dictionary<string, object>> result = new();

            foreach (IDataReader reader in _executor.ExecuteReader(SELECT_SCRIPT, 10, _parameters))
            {
                Dictionary<string, object> entity = new();

                for (int i = 0; i < _options.Entity.Properties.Count; i++)
                {
                    MetadataProperty property = _options.Entity.Properties[i];

                    if (!_mappers.TryGetValue(property, out PropertyDataMapper mapper))
                    {
                        continue;
                    }

                    object? value = mapper.GetValue(in reader);

                    if (value is DateTime dateTime)
                    {
                        dateTime = dateTime.AddYears(-_options.InfoBase.YearOffset);
                        
                        entity.Add(property.Name, dateTime);
                    }
                    else
                    {
                        entity.Add(property.Name, value!);
                    }
                }

                result.Add(entity);
            }

            return result;
        }

        #region "BUILD DATABASE QUERY SCRIPTS"

        private string BuildSelectScript()
        {
            StringBuilder script = new("SELECT ");

            bool first = true;

            foreach (var item in _mappers)
            {
                string propertyScript = item.Value.BuildSelectScript(item.Key);

                if (string.IsNullOrWhiteSpace(propertyScript))
                {
                    continue;
                }

                if (!first) { script.Append(", "); }

                script.Append(propertyScript);

                first = false;
            }

            script.Append($" FROM {_options.Entity.TableName}");

            return script.ToString();
        }
        private string BuildWhereClause()
        {
            if (_options.Filter != null && _options.Filter.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder where = new("WHERE ");

            bool first = true;

            for (int p = 0; p < _options.Filter.Count; p++)
            {
                FilterParameter parameter = _options.Filter[p];

                string fieldName = GetFieldName(parameter.Name);
                string _operator = GetComparisonSymbol(parameter.Operator);

                if (!first) { where.Append(" AND "); }

                if (parameter.Value == null)
                {
                    if (parameter.Operator == ComparisonOperator.NotEqual)
                    {
                        where.Append($"{fieldName} IS NOT NULL");
                    }
                    else
                    {
                        where.Append($"{fieldName} IS NULL");
                    }
                }
                else
                {
                    where.Append($"{fieldName} {_operator} @p{p}");
                }

                first = false;
            }

            return where.ToString();
        }
        private string GetFieldName(string propertyName)
        {
            foreach (MetadataProperty property in _options.Entity.Properties)
            {
                if (property.Name == propertyName)
                {
                    if (property.Fields.Count == 1) // TODO: multiple fields property
                    {
                        return property.Fields[0].Name;
                    }
                }
            }

            return string.Empty;
        }
        private string GetComparisonSymbol(ComparisonOperator comparisonOperator)
        {
            //typeof(ComparisonOperator).GetCustomAttributes()

            if (comparisonOperator == ComparisonOperator.Equal) return "=";
            else if (comparisonOperator == ComparisonOperator.NotEqual) return "<>";
            else if (comparisonOperator == ComparisonOperator.Less) return "<";
            else if (comparisonOperator == ComparisonOperator.LessOrEqual) return "<=";
            else if (comparisonOperator == ComparisonOperator.Greater) return ">";
            else if (comparisonOperator == ComparisonOperator.GreaterOrEqual) return ">=";
            else if (comparisonOperator == ComparisonOperator.Contains) return "IN";
            else if (comparisonOperator == ComparisonOperator.Between) return "BETWEEN";

            throw new ArgumentOutOfRangeException(nameof(comparisonOperator));
        }
        
        #endregion

        #region "SELECT TABLE PART BY NAME AND ENTITY (UUID)"

        private string SELECT_ENTITY_TABLE_PART_SCRIPT = string.Empty;
        //public string GetSelectTablePartScript()
        //{
        //    if (string.IsNullOrEmpty(SELECT_ENTITY_TABLE_PART_SCRIPT))
        //    {
        //        MetadataProperty? property = _entity.Properties
        //            .Where(p => p.Name == "Ссылка")
        //            .FirstOrDefault();

        //        if (property != null)
        //        {
        //            StringBuilder script = new();

        //            DatabaseField field = property.Fields[0];

        //            script.Append(BuildSelectEntityScript());

        //            script.Append($" WHERE {field.Name} = @entity ");

        //            script.Append($"ORDER BY {field.Name} ASC, _KeyField ASC;"); // MS SQLServer

        //            // TODO: script.Append($"ORDER BY {field.Name} ASC, _keyfield ASC;"); PostgreSQL

        //            SELECT_ENTITY_TABLE_PART_SCRIPT = script.ToString();
        //        }
        //    }

        //    return SELECT_ENTITY_TABLE_PART_SCRIPT;
        //}

        #endregion

        #region "JSON SERIALIZER"

        //else if (value is byte[] byteArray)
        //{
        //    entity.Add(property.Name, Convert.ToBase64String(byteArray));
        //}

        private const string CONST_TYPE_ENUM = "jcfg:EnumRef";
        private const string CONST_TYPE_CATALOG = "jcfg:CatalogRef";
        private const string CONST_TYPE_DOCUMENT = "jcfg:DocumentRef";
        private const string CONST_TYPE_EXCHANGE_PLAN = "jcfg:ExchangePlanRef";
        private const string CONST_TYPE_CHARACTERISTIC = "jcfg:ChartOfCharacteristicTypesRef";

        private const string CONST_REF = "Ref";
        private const string CONST_TYPE = "#type";
        private const string CONST_VALUE = "#value";
        private const string CONST_TYPE_STRING = "jxs:string";
        private const string CONST_TYPE_DECIMAL = "jxs:decimal";
        private const string CONST_TYPE_BOOLEAN = "jxs:boolean";
        private const string CONST_TYPE_DATETIME = "jxs:dateTime";
        private const string CONST_TYPE_CATALOG_REF = "jcfg:CatalogRef";
        private const string CONST_TYPE_CATALOG_OBJ = "jcfg:CatalogObject";
        private const string CONST_TYPE_DOCUMENT_REF = "jcfg:DocumentRef";
        private const string CONST_TYPE_DOCUMENT_OBJ = "jcfg:DocumentObject";
        private const string CONST_TYPE_OBJECT_DELETION = "jent:ObjectDeletion";
        private const string CONST_TYPE_INFO_REGISTER_SET = "jcfg:InformationRegisterRecordSet";
        private const string CONST_TYPE_ACCUM_REGISTER_SET = "jcfg:AccumulationRegisterRecordSet";

        //if (register.RegisterKind == RegisterKind.Balance && DataMapper.PropertyMappers[i].Property.Name == "ВидДвижения")
        //{
        //    value = (decimal) value == 0 ? "Receipt" : "Expense";
        //}

        //private string GetEnumValueName(Enumeration enumeration, Guid value)
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

        //private string GetEntityTypeName(int typeCode, Guid uuid)
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

        #endregion

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

        /////<summary>The method is correlated with this one <see cref="BuildWhereClause"/></summary>
        
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
        
        #endregion
    }
}