using DaJet.Metadata.Model;

namespace DaJet.Data.Mapping
{
    internal readonly struct PropertyDataMapper
    {
        private readonly int _value = -1;
        private readonly int _number = -1;
        private readonly int _string = -1;
        private readonly int _object = -1;
        private readonly int _boolean = -1;
        private readonly int _date_time = -1;
        private readonly int _type_code = -1;
        private readonly int _discriminator = -1;
        internal PropertyDataMapper(MetadataProperty property, ref int ordinal)
        {
            if (InfoBase.ReferenceTypeUuids.TryGetValue(Property.PropertyType.ReferenceTypeUuid, out ApplicationObject metaObject))
            {
                Enumeration = metaObject as Enumeration;
            }

            for (int i = 0; i < property.Fields.Count; i++)
            {
                ordinal++;

                FieldPurpose purpose = property.Fields[i].Purpose;

                if (purpose == FieldPurpose.Value)
                {
                    _value = ordinal;
                }
                else if (purpose == FieldPurpose.Version)
                {
                    _value = ordinal; // timestamp | rowversion -> binary(8)
                }
                else if (purpose == FieldPurpose.Discriminator)
                {
                    _discriminator = ordinal; // binary(1) -> byte
                    // 0x01 - Неопределено -> null     -> null
                    // 0x02 - Булево       -> bool     -> jxs:boolean  + true | false
                    // 0x03 - Число        -> decimal  -> jxs:decimal  + numeric
                    // 0x04 - Дата         -> DateTime -> jxs:dateTime + string (ISO 8601)
                    // 0x05 - Строка       -> string   -> jxs:string   + string
                    // 0x08 - Ссылка       -> Guid     -> jcfg:EnumRef     + Name
                    //                                  | jcfg:CatalogRef  + UUID
                    // EntityRef { TypeCode, Identity } | jcfg:DocumentRef + UUID
                }
                else if (purpose == FieldPurpose.Boolean)
                {
                    _boolean = ordinal; // binary(1) -> 0x00 | 0x01 -> bool
                }
                else if (purpose == FieldPurpose.Numeric)
                {
                    _number = ordinal; // numeric -> decimal | int | long
                }
                else if (purpose == FieldPurpose.DateTime)
                {
                    _date_time = ordinal; // datetime2 -> DateTime
                }
                else if (purpose == FieldPurpose.String)
                {
                    _string = ordinal; // nvarchar | nchar -> string
                }
                else if (purpose == FieldPurpose.TypeCode)
                {
                    _type_code = ordinal; // binary(4) -> int
                }
                else if (purpose == FieldPurpose.Object)
                {
                    _object = ordinal; // binary(16) -> Guid
                }
                else
                {
                    // this should not happen =)
                }
            }
        }
        internal void BuildSelectCommand(StringBuilder script, string tableAlias)
        {
            for (int i = 0; i < Property.Fields.Count; i++)
            {
                if (Property.Fields[i].Purpose == FieldPurpose.TypeCode ||
                    Property.Fields[i].Purpose == FieldPurpose.Discriminator)
                {
                    script.Append("CAST(");
                }

                if (string.IsNullOrEmpty(tableAlias))
                {
                    script.Append(Property.Fields[i].Name);
                }
                else
                {
                    script.Append($"{tableAlias}.{Property.Fields[i].Name}");
                }

                if (Property.Fields[i].Purpose == FieldPurpose.TypeCode ||
                    Property.Fields[i].Purpose == FieldPurpose.Discriminator)
                {
                    script.Append(" AS int)");
                }

                script.Append($" AS [{Property.Name}], ");
            }
        }
        public object GetValue(IDataReader reader)
        {
            if (DiscriminatorOrdinal > -1)
            {
                return GetMultipleValue(reader);
            }
            else if (TypeCodeOrdinal > -1)
            {
                return GetObjectValue(reader);
            }
            return GetSingleValue(reader);
        }
        private string GetEnumValue(Enumeration enumeration, Guid value)
        {
            for (int i = 0; i < enumeration.Values.Count; i++)
            {
                if (enumeration.Values[i].Uuid == value)
                {
                    return enumeration.Values[i].Name;
                }
            }
            return string.Empty;
        }
        private object GetSingleValue(IDataReader reader)
        {
            if (reader.IsDBNull(ValueOrdinal))
            {
                return null;
            }

            if (Property.PropertyType.IsUuid) // УникальныйИдентификатор
            {
                return new Guid(SQLHelper.Get1CUuid((byte[])reader.GetValue(ValueOrdinal)));
            }
            else if (Property.PropertyType.IsValueStorage) // ХранилищеЗначения
            {
                return ((byte[])reader.GetValue(ValueOrdinal));
            }
            else if (Property.PropertyType.CanBeString)
            {
                return reader.GetString(ValueOrdinal);
            }
            else if (Property.PropertyType.CanBeBoolean)
            {
                if (Property.Purpose == PropertyPurpose.System && Property.Name == "ЭтоГруппа")
                {
                    if (Provider == DatabaseProvider.SQLServer)
                    {
                        return (((byte[])reader.GetValue(ValueOrdinal))[0] == 0); // Unique 1C case =) Уникальный случай для 1С (булево значение инвертируется) !!!
                    }
                    else
                    {
                        return !reader.GetBoolean(ValueOrdinal); // Unique 1C case =) Уникальный случай для 1С (булево значение инвертируется) !!!
                    }
                }
                if (Provider == DatabaseProvider.SQLServer)
                {
                    return (((byte[])reader.GetValue(ValueOrdinal))[0] != 0); // All other cases - во всех остальных случаях булево значение не инвертируется
                }
                else
                {
                    return reader.GetBoolean(ValueOrdinal); // All other cases - во всех остальных случаях булево значение не инвертируется
                }
            }
            else if (Property.PropertyType.CanBeNumeric)
            {
                return reader.GetDecimal(ValueOrdinal);
            }
            else if (Property.PropertyType.CanBeDateTime)
            {
                DateTime dateTime = reader.GetDateTime(ValueOrdinal);
                if (InfoBase.YearOffset > 0)
                {
                    dateTime = dateTime.AddYears(-InfoBase.YearOffset);
                }
                return dateTime;
            }
            else if (Property.PropertyType.CanBeReference)
            {
                Guid uuid = new Guid(SQLHelper.Get1CUuid((byte[])reader.GetValue(ValueOrdinal)));

                if (Enumeration != null)
                {
                    return new EntityRef(Property.PropertyType.ReferenceTypeCode, uuid,
                        CONST_TYPE_ENUM + "." + Enumeration.Name, GetEnumValue(Enumeration, uuid));
                }
                return GetEntityRef(Property, uuid);
            }

            return null; // this should not happen
        }
        private object GetMultipleValue(IDataReader reader)
        {
            if (reader.IsDBNull(DiscriminatorOrdinal))
            {
                // Такое может быть, например, для реквизитов групп элементов справочников,
                // которые используются только для групп
                return null;
            }

            int discriminator;
            if (Provider == DatabaseProvider.SQLServer)
            {
                discriminator = reader.GetInt32(DiscriminatorOrdinal);
            }
            else
            {
                discriminator = ((byte[])reader.GetValue(DiscriminatorOrdinal))[0];
            }

            if (discriminator == 1) // Неопределено
            {
                return null;
            }
            else if (discriminator == 2) // Булево
            {
                if (Provider == DatabaseProvider.SQLServer)
                {
                    return (((byte[])reader.GetValue(BooleanOrdinal))[0] != 0);
                }
                else
                {
                    return reader.GetBoolean(BooleanOrdinal);
                }
            }
            else if (discriminator == 3) // Число
            {
                return reader.GetDecimal(NumberOrdinal);
            }
            else if (discriminator == 4) // Дата
            {
                DateTime dateTime = reader.GetDateTime(DateTimeOrdinal);
                if (InfoBase.YearOffset > 0)
                {
                    dateTime = dateTime.AddYears(-InfoBase.YearOffset);
                }
                return dateTime;
            }
            else if (discriminator == 5) // Строка
            {
                return reader.GetString(StringOrdinal);
            }
            else if (discriminator == 8) // Ссылка
            {
                return GetObjectValue(reader);
            }

            return null; // unknown discriminator - this should not happen
        }
        private object GetObjectValue(IDataReader reader)
        {
            // we are here from GetMultipleValue

            if (reader.IsDBNull(ObjectOrdinal))
            {
                // Такое может быть, например, для реквизитов групп элементов справочников,
                // которые используются только для групп
                return null;
            }

            Guid uuid = new Guid(SQLHelper.Get1CUuid((byte[])reader.GetValue(ObjectOrdinal)));

            if (TypeCodeOrdinal > -1) // multiple reference value - TRef + RRef
            {
                if (reader.IsDBNull(TypeCodeOrdinal))
                {
                    // Такое может быть, например, для реквизитов групп элементов справочников,
                    // которые используются только для групп
                    return null;
                }

                int typeCode;
                if (Provider == DatabaseProvider.SQLServer)
                {
                    typeCode = reader.GetInt32(TypeCodeOrdinal);
                }
                else
                {
                    typeCode = DbUtilities.GetInt32((byte[])reader.GetValue(TypeCodeOrdinal));
                }

                return GetEntityRef(typeCode, uuid);
            }
            else // single reference value - RRef only
            {
                return GetEntityRef(Property, uuid);
            }
        }
        private EntityRef GetEntityRef(int typeCode, Guid uuid)
        {
            if (InfoBase.ReferenceTypeCodes.TryGetValue(typeCode, out ApplicationObject metaObject))
            {
                if (metaObject is Enumeration enumeration)
                {
                    return new EntityRef(typeCode, uuid, CONST_TYPE_ENUM + "." + enumeration.Name, GetEnumValue(enumeration, uuid));
                }
                else if (metaObject is Catalog)
                {
                    return new EntityRef(typeCode, uuid, CONST_TYPE_CATALOG + "." + metaObject.Name);
                }
                else if (metaObject is Document)
                {
                    return new EntityRef(typeCode, uuid, CONST_TYPE_DOCUMENT + "." + metaObject.Name);
                }
                else if (metaObject is Publication)
                {
                    return new EntityRef(typeCode, uuid, CONST_TYPE_EXCHANGE_PLAN + "." + metaObject.Name);
                }
                else if (metaObject is Characteristic)
                {
                    return new EntityRef(typeCode, uuid, CONST_TYPE_CHARACTERISTIC + "." + metaObject.Name);
                }
            }

            return null; // unknown type code - this should not happen
        }
        private EntityRef GetEntityRef(MetadataProperty property, Guid uuid)
        {
            if (!property.PropertyType.CanBeReference || property.PropertyType.ReferenceTypeUuid == Guid.Empty)
            {
                return null;
            }

            if (property.PropertyType.ReferenceTypeCode != 0)
            {
                return GetEntityRef(property.PropertyType.ReferenceTypeCode, uuid);
            }

            // TODO: ReferenceTypeCode == 0 this should be fixed in DaJet.Metadata library

            if (InfoBase.ReferenceTypeUuids.TryGetValue(property.PropertyType.ReferenceTypeUuid, out ApplicationObject propertyType))
            {
                property.PropertyType.ReferenceTypeCode = propertyType.TypeCode; // patch metadata

                if (propertyType is Enumeration enumeration)
                {
                    return new EntityRef(propertyType.TypeCode, uuid, CONST_TYPE_ENUM + "." + enumeration.Name, GetEnumValue(enumeration, uuid));
                }
                else if (propertyType is Catalog)
                {
                    return new EntityRef(propertyType.TypeCode, uuid, CONST_TYPE_CATALOG + "." + propertyType.Name);
                }
                else if (propertyType is Document)
                {
                    return new EntityRef(propertyType.TypeCode, uuid, CONST_TYPE_DOCUMENT + "." + propertyType.Name);
                }
                else if (propertyType is Publication)
                {
                    return new EntityRef(propertyType.TypeCode, uuid, CONST_TYPE_EXCHANGE_PLAN + "." + propertyType.Name);
                }
                else if (propertyType is Characteristic)
                {
                    return new EntityRef(propertyType.TypeCode, uuid, CONST_TYPE_CHARACTERISTIC + "." + propertyType.Name);
                }
            }

            if (property.Name == "Владелец")
            {
                if (MetaObject is Catalog || MetaObject is Characteristic)
                {
                    // TODO: this issue should be fixed in DaJet.Metadata library
                    // NOTE: file names lookup - Property.PropertyType.ReferenceTypeUuid for Owner property is a FileName, not metadata object Uuid !!!
                    if (InfoBase.Catalogs.TryGetValue(property.PropertyType.ReferenceTypeUuid, out ApplicationObject catalog))
                    {
                        property.PropertyType.ReferenceTypeCode = catalog.TypeCode; // patch metadata
                        return GetEntityRef(property.PropertyType.ReferenceTypeCode, uuid);
                    }
                    else if (InfoBase.Characteristics.TryGetValue(property.PropertyType.ReferenceTypeUuid, out ApplicationObject characteristic))
                    {
                        property.PropertyType.ReferenceTypeCode = characteristic.TypeCode; // patch metadata
                        return GetEntityRef(property.PropertyType.ReferenceTypeCode, uuid);
                    }
                }
            }

            return new EntityRef(property.PropertyType.ReferenceTypeCode, uuid);
        }
    }
}