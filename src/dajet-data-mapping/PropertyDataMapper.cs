using DaJet.Metadata.Model;
using System.Data;
using System.Text;

namespace DaJet.Data.Mapping
{
    internal readonly struct PropertyDataMapper
    {
        private readonly int _uuid = -1;   // binary(16)
        private readonly int _binary = -1; // BASE64
        private readonly int _discriminator = -1; // _TYPE
        private readonly int _boolean = -1;       // _L
        private readonly int _numeric = -1;       // _N
        private readonly int _date_time = -1;     // _T
        private readonly int _string = -1;        // _S
        private readonly int _type_code = -1;     // _RTRef : for single type value = actual value; for multiple = ordinal !
        private readonly int _object = -1;        // _RRRef
        private readonly bool _invert = false;    // _Folder
        private readonly bool _single = false; // single or multiple type value flag

        // Исключения из правил:
        // - _KeyField (табличная часть) binary(4) -> int CanBeNumeric
        // - _Folder (иерархические ссылочные типы) binary(1) -> bool инвертировать !!!
        // - _Version (ссылочные типы) timestamp binary(8) -> IsBinary
        // - _Type (тип значений характеристики) varbinary(max) -> IsBinary nullable
        // - _RecordKind (вид движения накопления) numeric(1) CanBeNumeric Приход = 0, Расход = 1
        // - _DimHash numeric(10) ?

        internal PropertyDataMapper(in MetadataProperty property, ref int ordinal)
        {
            DataTypeSet type = property.PropertyType;
            
            _single = !type.IsMultipleType;

            for (int i = 0; i < property.Fields.Count; i++)
            {
                FieldPurpose purpose = property.Fields[i].Purpose;

                if (purpose == FieldPurpose.Value) // single type value
                {
                    if (type.IsUuid) { _uuid = ordinal; } // binary(16)
                    else if (type.IsValueStorage) { _binary = ordinal; } // varbinary(max)
                    else if (type.CanBeBoolean)
                    {
                        _boolean = ordinal; // binary(1)
                        _invert = (property.Fields[i].Name == "_Folder"); // ЭтоГруппа (инвертировать)
                    } 
                    else if (type.CanBeNumeric) { _numeric = ordinal; } // numeric | binary(x)
                    else if (type.CanBeDateTime) { _date_time = ordinal; } // datetime2
                    else if (type.CanBeString) { _string = ordinal; } // nvarchar(max) | nvarchar(x) | nchar(x)
                    else if (type.CanBeReference)
                    {
                        _object = ordinal; // binary(16)
                        _type_code = type.TypeCode; // binary(4)
                    }
                    else if (type.IsBinary) { _binary = ordinal; } // Характеристика.ТипЗначения -> varbinary(max)
                    else
                    {
                        continue; // this should not happen - ignore field
                    }
                }
                else if (purpose == FieldPurpose.Version) // single type value
                {
                    _binary = ordinal; // Ссылка.ВерсияДанных : timestamp | rowversion -> binary(8)
                }
                else if (purpose == FieldPurpose.Discriminator) // multiple type value
                {
                    _discriminator = ordinal; // binary(1) -> byte
                }
                else if (purpose == FieldPurpose.Boolean) // multiple type value
                {
                    _boolean = ordinal; // binary(1) -> 0x00 | 0x01 -> bool
                }
                else if (purpose == FieldPurpose.Numeric) // multiple type value
                {
                    _numeric = ordinal; // numeric -> decimal | int | long
                }
                else if (purpose == FieldPurpose.DateTime) // multiple type value
                {
                    _date_time = ordinal; // datetime2 -> DateTime
                }
                else if (purpose == FieldPurpose.String) // multiple type value
                {
                    _string = ordinal; // nvarchar | nchar -> string
                }
                else if (purpose == FieldPurpose.TypeCode) // multiple type value
                {
                    _type_code = ordinal; // binary(4) -> int
                }
                else if (purpose == FieldPurpose.Object) // multiple type value
                {
                    _object = ordinal; // binary(16) -> Guid
                }
                else
                {
                    continue; // this should not happen - ignore field
                }

                ordinal++;
            }
        }
        
        internal string BuildSelectScript(in MetadataProperty property, in string tableAlias = null!)
        {
            StringBuilder script = new(string.Empty);

            for (int i = 0; i < property.Fields.Count; i++)
            {
                DatabaseField field = property.Fields[i];

                if (script.Length > 0)
                {
                    script.Append($", ");
                }

                if (field.Name == "_KeyField") // Табличная часть "КлючСтроки"
                {
                    script.Append("CAST(CAST(_KeyField AS int) AS numeric(5,0))");
                    continue;
                }

                bool cast_to_int =
                    field.Purpose == FieldPurpose.TypeCode ||
                    field.Purpose == FieldPurpose.Discriminator;

                if (cast_to_int)
                {
                    script.Append("CAST(");
                }

                if (string.IsNullOrEmpty(tableAlias))
                {
                    script.Append(field.Name);
                }
                else
                {
                    script.Append($"{tableAlias}.{field.Name}");
                }

                if (cast_to_int)
                {
                    script.Append(" AS int)");
                }
            }

            return script.ToString();
        }
        
        public object? GetValue(in IDataReader reader)
        {
            if (_single)
            {
                return GetSingleValue(in reader);
            }
            else
            {
                return GetMultipleValue(in reader);
            }
        }
        private object? GetSingleValue(in IDataReader reader)
        {
            if (_uuid > -1) { return GetUuid(in reader); } // УникальныйИдентификатор
            else if (_binary > -1) { return GetBinary(in reader); } // ХранилищеЗначения, ВерсияДанных, КлючСтроки
            else if (_boolean > -1) { return GetBoolean(in reader); }
            else if (_numeric > -1) { return GetNumeric(in reader); }
            else if (_date_time > -1) { return GetDateTime(in reader); }
            else if (_string > -1) { return GetString(in reader); }
            else if (_object > -1) { return GetEntityRef(in reader); }

            return null;
        }
        private object? GetMultipleValue(in IDataReader reader)
        {
            if (_discriminator > -1)
            {
                if (reader.IsDBNull(_discriminator))
                {
                    // Такое может быть, например, для реквизитов не групповых элементов
                    // справочников или харакетристик, которые используются только для групп
                    return null;
                }

                int discriminator = reader.GetInt32(_discriminator); // MS SQLServer

                // TODO: discriminator = ((byte[])reader.GetValue(DiscriminatorOrdinal))[0]; PostgreSQL

                if (discriminator == 1) { return null; } // Неопределено
                else if (discriminator == 2) { return GetBoolean(in reader); } // Булево
                else if (discriminator == 3) { return GetNumeric(in reader); } // Число
                else if (discriminator == 4) { return GetDateTime(in reader); } // Дата
                else if (discriminator == 5) { return GetString(in reader); } // Строка
                else if (discriminator == 8) { return GetEntityRef(in reader); } // Ссылка

                return null; // unknown discriminator - this should not happen
            }

            if (_type_code > -1) // multiple reference type value
            {
                return GetEntityRef(in reader);
            }

            return null;
        }
        private object? GetUuid(in IDataReader reader)
        {
            if (reader.IsDBNull(_uuid))
            {
                return null;
            }

            return new Guid(SQLHelper.Get1CUuid((byte[])reader.GetValue(_uuid)));
        }
        private object? GetBinary(in IDataReader reader)
        {
            if (reader.IsDBNull(_binary))
            {
                return null;
            }

            return ((byte[])reader.GetValue(_binary));
        }
        private object? GetBoolean(in IDataReader reader)
        {
            if (reader.IsDBNull(_boolean))
            {
                return null;
            }

            if (_invert)
            {
                return (((byte[])reader.GetValue(_boolean))[0] == 0); // _Folder ЭтоГруппа

                // TODO: return !reader.GetBoolean(_boolean); // PostgreSQL
            }
            else
            {
                return (((byte[])reader.GetValue(_boolean))[0] != 0); // true

                // TODO: return reader.GetBoolean(_boolean); // PostgreSQL
            }
        }
        private object? GetNumeric(in IDataReader reader)
        {
            if (reader.IsDBNull(_numeric))
            {
                return null;
            }

            return reader.GetDecimal(_numeric);
        }
        private object? GetDateTime(in IDataReader reader)
        {
            if (reader.IsDBNull(_date_time))
            {
                return null;
            }

            return reader.GetDateTime(_date_time);
        }
        private object? GetString(in IDataReader reader)
        {
            if (reader.IsDBNull(_string))
            {
                return null;
            }

            return reader.GetString(_string);
        }
        private object? GetEntityRef(in IDataReader reader)
        {
            if (reader.IsDBNull(_object))
            {
                return null;
            }

            Guid uuid = new(SQLHelper.Get1CUuid((byte[])reader.GetValue(_object)));

            if (_single) // single reference type value - RRef
            {
                return new EntityRef(_type_code, uuid);
            }

            if (_type_code > -1 && !reader.IsDBNull(_type_code)) // multiple reference type value - TRef + RRef
            {
                int typeCode = reader.GetInt32(_type_code);
                
                // TODO: typeCode = DbUtilities.GetInt32((byte[])reader.GetValue(TypeCodeOrdinal)); // PostgreSQL

                return new EntityRef(typeCode, uuid);
            }
            
            return null;
        }
    }
}