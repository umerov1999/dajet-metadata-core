using DaJet.Metadata.Model;
using System;

namespace DaJet.Metadata.Core
{
    public static class Configurator
    {
        #region "Information register system properties"

        // Последовательность сериализации системных свойств регистра в формат 1С JDTO
        // 1. "Регистратор" = Recorder   - uuid
        // 2. "Период"      = Period     - DateTime
        // 3. "ВидДвижения" = RecordType - string { "Receipt", "Expense" }
        // 4. "Активность"  = Active     - bool
        public static void ConfigureInformationRegister(in InformationRegister register, DatabaseProvider provider)
        {
            if (register == null)
            {
                return;
            }

            if (register.UseRecorder)
            {
                // Описание типов свойства "Регистратор" конфигурируется после чтения метаданных документов:
                // именно объект метаданных "Документ" содержит ссылки на свои регистры движения.
                ConfigurePropertyРегистратор(register, provider);
            }

            if (register.Periodicity != RegisterPeriodicity.None)
            {
                ConfigurePropertyПериод(register, provider);
            }

            if (register.UseRecorder)
            {
                ConfigurePropertyАктивность(register, provider);
                ConfigurePropertyНомерЗаписи(register, provider);
            }
        }
        public static void ConfigurePropertyПериод(in ApplicationObject register, DatabaseProvider provider)
        {
            MetadataProperty property = new MetadataProperty()
            {
                Name = "Период",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = (provider == DatabaseProvider.SQLServer ? "_Period" : "_period")
            };
            property.PropertyType.CanBeDateTime = true;

            property.Fields.Add(new DatabaseField()
            {
                Name = property.DbName,
                Length = 6,
                Precision = 19,
                TypeName = "datetime2"
            });

            register.Properties.Add(property);
        }
        public static void ConfigurePropertyНомерЗаписи(in ApplicationObject register, DatabaseProvider provider)
        {
            MetadataProperty property = new MetadataProperty()
            {
                Name = "НомерСтроки",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = (provider == DatabaseProvider.SQLServer ? "_LineNo" : "_lineno")
            };
            property.PropertyType.CanBeNumeric = true;

            property.Fields.Add(new DatabaseField()
            {
                Name = property.DbName,
                Length = 5,
                Precision = 9,
                TypeName = "numeric"
            });
            
            register.Properties.Add(property);
        }
        public static void ConfigurePropertyАктивность(in ApplicationObject register, DatabaseProvider provider)
        {
            MetadataProperty property = new MetadataProperty()
            {
                Name = "Активность",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = (provider == DatabaseProvider.SQLServer ? "_Active" : "_active")
            };

            property.PropertyType.CanBeBoolean = true;

            property.Fields.Add(new DatabaseField()
            {
                Name = property.DbName,
                Length = 1,
                TypeName = "binary"
            });

            register.Properties.Add(property);
        }
        public static void ConfigurePropertyРегистратор(in ApplicationObject register, DatabaseProvider provider)
        {
            MetadataProperty property = new MetadataProperty()
            {
                Uuid = Guid.Empty,
                Name = "Регистратор",
                Purpose = PropertyPurpose.System,
                DbName = (provider == DatabaseProvider.SQLServer ? "_Recorder" : "_recorder")
            };

            property.PropertyType.CanBeReference = true;

            property.Fields.Add(new DatabaseField()
            {
                Name = (provider == DatabaseProvider.SQLServer ? "_RecorderRRef" : "_recorderrref"),
                Length = 16,
                TypeName = "binary",
                IsPrimaryKey = true,
                Purpose = FieldPurpose.Value
            });

            register.Properties.Add(property);

            //MetadataProperty property = register.Properties.Where(p => p.Name == "Регистратор").FirstOrDefault();

            //if (property == null)
            //{
            //    // добавляем новое свойство
            //    property = new MetadataProperty()
            //    {
            //        Name = "Регистратор",
            //        Purpose = PropertyPurpose.System,
            //        Uuid = Guid.Empty,
            //        DbName = (provider == DatabaseProvider.SQLServer ? "_Recorder" : "_recorder")
            //    };
            //    property.PropertyType.CanBeReference = true;
            //    property.PropertyType.ReferenceTypeUuid = document.Uuid; // single type value
            //                                                             //property.PropertyType.ReferenceTypeCode = document.TypeCode; // single type value
            //    property.Fields.Add(new DatabaseField()
            //    {
            //        Name = (provider == DatabaseProvider.SQLServer ? "_RecorderRRef" : "_recorderrref"),
            //        Length = 16,
            //        TypeName = "binary",
            //        Scale = 0,
            //        Precision = 0,
            //        IsNullable = false,
            //        KeyOrdinal = 0,
            //        IsPrimaryKey = true,
            //        Purpose = FieldPurpose.Value
            //    });
            //    register.Properties.Add(property);
            //    return;
            //}

            //// На всякий случай проверям повторное обращение одного и того же документа
            //if (property.PropertyType.ReferenceTypeUuid == document.Uuid) return;

            //// Проверям необходимость добавления поля для хранения кода типа документа
            //if (property.PropertyType.ReferenceTypeUuid == Guid.Empty) return;

            //// Изменяем назначение поля для хранения ссылки на документ, предварительно убеждаясь в его наличии
            //DatabaseField field = property.Fields.Where(f => f.Name.ToLowerInvariant() == "_recorderrref").FirstOrDefault();
            //if (field != null)
            //{
            //    field.Purpose = FieldPurpose.Object;
            //}

            //// Добавляем поле для хранения кода типа документа, предварительно убеждаясь в его отсутствии
            //if (property.Fields.Where(f => f.Name.ToLowerInvariant() == "_recordertref").FirstOrDefault() == null)
            //{
            //    property.Fields.Add(new DatabaseField()
            //    {
            //        Name = (provider == DatabaseProvider.SQLServer ? "_RecorderTRef" : "_recordertref"),
            //        Length = 4,
            //        TypeName = "binary",
            //        Scale = 0,
            //        Precision = 0,
            //        IsNullable = false,
            //        KeyOrdinal = 0,
            //        IsPrimaryKey = true,
            //        Purpose = FieldPurpose.TypeCode
            //    });
            //}

            //// Устанавливаем признак множественного типа значения (составного типа данных)
            ////property.PropertyType.ReferenceTypeCode = 0; // multiple type value
            //property.PropertyType.ReferenceTypeUuid = Guid.Empty; // multiple type value
        }

        #endregion
    }



    //{
    //    internal InfoBase InfoBase;

    //    private readonly Dictionary<string, Type> MetadataTypes = new Dictionary<string, Type>()
    //    {
    //        { MetadataTokens.Acc, typeof(Account) },
    //        { MetadataTokens.AccRg, typeof(AccountingRegister) },
    //        { MetadataTokens.AccumRg, typeof(AccumulationRegister) },
    //        { MetadataTokens.Reference, typeof(Catalog) },
    //        { MetadataTokens.Chrc, typeof(Characteristic) },
    //        { MetadataTokens.Const, typeof(Constant) },
    //        { MetadataTokens.Document, typeof(Document) },
    //        { MetadataTokens.Enum, typeof(Enumeration) },
    //        { MetadataTokens.InfoRg, typeof(InformationRegister) },
    //        { MetadataTokens.Node, typeof(Publication) },
    //        { MetadataTokens.VT, typeof(TablePart) }
    //    };
    //    private readonly Dictionary<Type, Func<ApplicationObject>> Factories = new Dictionary<Type, Func<ApplicationObject>>()
    //    {
    //        { typeof(Account), () => { return new Account(); } },
    //        { typeof(AccountingRegister), () => { return new AccountingRegister(); } },
    //        { typeof(AccumulationRegister), () => { return new AccumulationRegister(); } },
    //        { typeof(Catalog), () => { return new Catalog(); } },
    //        { typeof(Characteristic), () => { return new Characteristic(); } },
    //        { typeof(Constant), () => { return new Constant(); } },
    //        { typeof(Document), () => { return new Document(); } },
    //        { typeof(Enumeration), () => { return new Enumeration(); } },
    //        { typeof(InformationRegister), () => { return new InformationRegister(); } },
    //        { typeof(Publication), () => { return new Publication(); } },
    //        { typeof(TablePart), () => { return new TablePart(); } }
    //    };

    //    private readonly bool IsParallel = false;
    //    private readonly bool LogExceptions = false;

    //    private Configurator()
    //    {

    //    }

    //    #region "DbNames"

    //    public string CreateDbName(string token, int code)
    //    {
    //        return $"_{token}{code}".ToLowerInvariant();
    //    }
    //    public Type GetTypeByToken(string token)
    //    {
    //        if (string.IsNullOrWhiteSpace(token)) return null;

    //        if (MetadataTypes.TryGetValue(token, out Type type))
    //        {
    //            return type;
    //        }
    //        return null;
    //    }
    //    public Func<ApplicationObject> GetFactory(string token)
    //    {
    //        Type type = GetTypeByToken(token);
    //        if (type == null) return null;

    //        if (Factories.TryGetValue(type, out Func<ApplicationObject> factory))
    //        {
    //            return factory;
    //        }
    //        return null;
    //    }
    //    public ApplicationObject CreateObject(Guid uuid, string token, int code)
    //    {
    //        Func<ApplicationObject> factory = GetFactory(token);
    //        if (factory == null) return null;

    //        ApplicationObject metaObject = factory();
    //        // FIXME: metaObject.FileName = uuid;
    //        metaObject.TypeCode = code;
    //        metaObject.TableName = CreateDbName(token, code);

    //        return metaObject;
    //    }
    //    public MetadataProperty CreateProperty(Guid uuid, string token, int code)
    //    {
    //        return new MetadataProperty()
    //        {
    //            Uuid = uuid,
    //            // FIXME FileName = uuid,
    //            DbName = CreateDbName(token, code)
    //        };
    //    }

    //    #endregion

    //    public void ConfigureProperties(ApplicationObject metaObject, ConfigObject properties, PropertyPurpose purpose)
    //    {
    //        int propertiesCount = properties.GetInt32(new int[] { 1 }); // количество реквизитов
    //        if (propertiesCount == 0) return;

    //        int propertyOffset = 2;
    //        for (int p = 0; p < propertiesCount; p++)
    //        {
    //            // P.0.1.1.1.1.2 - property uuid
    //            Guid propertyUuid = properties.GetUuid(new int[] { p + propertyOffset, 0, 1, 1, 1, 1, 2 });
    //            // P.0.1.1.1.2 - property name
    //            string propertyName = properties.GetString(new int[] { p + propertyOffset, 0, 1, 1, 1, 2 });
    //            // P.0.1.1.1.3 - property alias descriptor
    //            string propertyAlias = string.Empty;
    //            ConfigObject aliasDescriptor = properties.GetObject(new int[] { p + propertyOffset, 0, 1, 1, 1, 3 });
    //            if (aliasDescriptor.Values.Count == 3)
    //            {
    //                // P.0.1.1.1.3.2 - property alias
    //                propertyAlias = properties.GetString(new int[] { p + propertyOffset, 0, 1, 1, 1, 3, 2 });
    //            }
    //            // P.0.1.1.2 - property types
    //            ConfigObject propertyTypes = properties.GetObject(new int[] { p + propertyOffset, 0, 1, 1, 2 });
    //            // P.0.1.1.2.0 = "Pattern"

    //            // TODO !!! DataTypeSet typeInfo = (DataTypeInfo)GetConverter<DataTypeInfo>().Convert(propertyTypes);
    //            DataTypeSet typeInfo = new DataTypeSet();

    //            // P.0.3 - property usage for catalogs and characteristics
    //            int propertyUsage = -1;
    //            if (metaObject is Catalog || metaObject is Characteristic)
    //            {
    //                propertyUsage = properties.GetInt32(new int[] { p + propertyOffset, 0, 3 });
    //            }

    //            ConfigureProperty(metaObject, purpose, propertyUuid, propertyName, propertyAlias, typeInfo, propertyUsage);
    //        }
    //    }
    //    public void ConfigureProperty(ApplicationObject metaObject, PropertyPurpose purpose, Guid fileName, string name, string alias, DataTypeSet type, int propertyUsage)
    //    {
    //        if (!InfoBase.Properties.TryGetValue(fileName, out MetadataProperty property)) return;

    //        property.Name = name;
    //        property.Alias = alias;
    //        property.Purpose = purpose;
    //        property.PropertyType = type;
    //        if (propertyUsage != -1)
    //        {
    //            property.PropertyUsage = (PropertyUsage)propertyUsage;
    //        }
    //        metaObject.Properties.Add(property);

    //        ConfigureDatabaseFields(property);
    //    }

    //    public void ConfigureSharedProperties(ApplicationObject metaObject)
    //    {
    //        // FIXME:
    //        //foreach (SharedProperty property in InfoBase.SharedProperties.Values)
    //        //{
    //        //    if (property.UsageSettings.TryGetValue(metaObject.FileName, out SharedPropertyUsage usage))
    //        //    {
    //        //        if (usage == SharedPropertyUsage.Use)
    //        //        {
    //        //            metaObject.Properties.Add(property);
    //        //        }
    //        //    }
    //        //    else // Auto
    //        //    {
    //        //        if (property.AutomaticUsage == AutomaticUsage.Use)
    //        //        {
    //        //            metaObject.Properties.Add(property);
    //        //        }
    //        //    }
    //        //}
    //    }

    //    #region "TableParts"

    //    public void ConfigureTableParts(ApplicationObject owner, ConfigObject tableParts)
    //    {
    //        int tablePartsCount = tableParts.GetInt32(new int[] { 1 }); // количество табличных частей
    //        if (tablePartsCount == 0) return;

    //        int offset = 2;
    //        for (int t = 0; t < tablePartsCount; t++)
    //        {
    //            // T.0.1.5.1.1.2 - uuid табличной части
    //            Guid uuid = tableParts.GetUuid(new int[] { t + offset, 0, 1, 5, 1, 1, 2 });
    //            // T.0.1.5.1.2 - имя табличной части
    //            string name = tableParts.GetString(new int[] { t + offset, 0, 1, 5, 1, 2 });

    //            if (InfoBase.TableParts.TryGetValue(uuid, out ApplicationObject tablePart))
    //            {
    //                if (tablePart is TablePart)
    //                {
    //                    tablePart.Name = name;
    //                    ((TablePart)tablePart).Owner = owner;
    //                    tablePart.TableName = owner.TableName + tablePart.TableName;
    //                    owner.TableParts.Add((TablePart)tablePart);

    //                    // T.2 - коллекция реквизитов табличной части (ConfigObject)
    //                    // T.2.0 = 888744e1-b616-11d4-9436-004095e12fc7 - идентификатор коллекции реквизитов табличной части
    //                    // T.2.1 - количество реквизитов табличной части
    //                    Guid collectionUuid = tableParts.GetUuid(new int[] { t + offset, 2, 0 });
    //                    if (collectionUuid == new Guid("888744e1-b616-11d4-9436-004095e12fc7"))
    //                    {
    //                        ConfigurePropertyСсылка(owner, tablePart);
    //                        ConfigurePropertyКлючСтроки(tablePart);
    //                        ConfigurePropertyНомерСтроки(tablePart);

    //                        ConfigObject properties = tableParts.GetObject(new int[] { t + offset, 2 });
    //                        ConfigureProperties(tablePart, properties, PropertyPurpose.Property);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    private void ConfigurePropertyСсылка(ApplicationObject owner, ApplicationObject tablePart)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "Ссылка",
    //            // FIXME FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System
    //            // FIXME , DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? owner.TableName + "_IDRRef" : owner.TableName + "_idrref")
    //        };
    //        property.PropertyType.IsUuid = true;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 16,
    //            TypeName = "binary",
    //            KeyOrdinal = 1,
    //            IsPrimaryKey = true
    //        });
    //        tablePart.Properties.Add(property);
    //    }
    //    private void ConfigurePropertyКлючСтроки(ApplicationObject tablePart)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "КлючСтроки",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_KeyField" : "_keyfield")
    //        };
    //        property.PropertyType.IsBinary = true;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 4,
    //            TypeName = "binary",
    //            KeyOrdinal = 2,
    //            IsPrimaryKey = true
    //        });
    //        tablePart.Properties.Add(property);
    //    }
    //    private void ConfigurePropertyНомерСтроки(ApplicationObject tablePart)
    //    {
    //        if (!InfoBase.Properties.TryGetValue(tablePart.FileName, out MetadataProperty property)) return;

    //        property.Name = "НомерСтроки";
    //        property.PropertyType.CanBeNumeric = true;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 5,
    //            Precision = 5,
    //            TypeName = "numeric"
    //        });

    //        tablePart.Properties.Add(property);
    //    }

    //    #endregion

    //    public void ConfigureDatabaseFields(MetadataProperty property)
    //    {
    //        if (property.PropertyType.IsMultipleType)
    //        {
    //            ConfigureDatabaseFieldsForMultipleType(property);
    //        }
    //        else
    //        {
    //            ConfigureDatabaseFieldsForSingleType(property);
    //        }
    //    }
    //    private void ConfigureDatabaseFieldsForSingleType(MetadataProperty property)
    //    {
    //        if (property.PropertyType.IsUuid)
    //        {
    //            if (FileReader.DatabaseProvider == DatabaseProvider.SQLServer)
    //            {
    //                property.Fields.Add(new DatabaseField(property.DbName, "binary", 16));
    //            }
    //            else
    //            {
    //                property.Fields.Add(new DatabaseField(property.DbName, "bytea", 16));
    //            }
    //        }
    //        else if (property.PropertyType.IsBinary)
    //        {
    //            // is used only for system properties of system types
    //            // TODO: log if it happens eventually
    //        }
    //        else if (property.PropertyType.IsValueStorage)
    //        {
    //            if (FileReader.DatabaseProvider == DatabaseProvider.SQLServer)
    //            {
    //                property.Fields.Add(new DatabaseField(property.DbName, "varbinary", -1));
    //            }
    //            else
    //            {
    //                property.Fields.Add(new DatabaseField(property.DbName, "bytea", -1));
    //            }
    //        }
    //        else if (property.PropertyType.CanBeString)
    //        {
    //            if (property.PropertyType.StringKind == StringKind.Fixed)
    //            {
    //                if (FileReader.DatabaseProvider == DatabaseProvider.SQLServer)
    //                {
    //                    property.Fields.Add(new DatabaseField(property.DbName, "nchar", property.PropertyType.StringLength));
    //                }
    //                else
    //                {
    //                    property.Fields.Add(new DatabaseField(property.DbName, "mchar", property.PropertyType.StringLength));
    //                }
    //            }
    //            else
    //            {
    //                if (FileReader.DatabaseProvider == DatabaseProvider.SQLServer)
    //                {
    //                    property.Fields.Add(new DatabaseField(property.DbName, "nvarchar", property.PropertyType.StringLength));
    //                }
    //                else
    //                {
    //                    property.Fields.Add(new DatabaseField(property.DbName, "mvarchar", property.PropertyType.StringLength));
    //                }
    //            }
    //        }
    //        else if (property.PropertyType.CanBeNumeric)
    //        {
    //            // length can be updated from database
    //            property.Fields.Add(new DatabaseField(
    //                property.DbName,
    //                "numeric", 9,
    //                property.PropertyType.NumericPrecision,
    //                property.PropertyType.NumericScale));
    //        }
    //        else if (property.PropertyType.CanBeBoolean)
    //        {
    //            if (FileReader.DatabaseProvider == DatabaseProvider.SQLServer)
    //            {
    //                property.Fields.Add(new DatabaseField(property.DbName, "binary", 1));
    //            }
    //            else
    //            {
    //                property.Fields.Add(new DatabaseField(property.DbName, "boolean", 1));
    //            }
    //        }
    //        else if (property.PropertyType.CanBeDateTime)
    //        {
    //            // length, precision and scale can be updated from database
    //            if (FileReader.DatabaseProvider == DatabaseProvider.SQLServer)
    //            {
    //                property.Fields.Add(new DatabaseField(property.DbName, "datetime2", 6, 19, 0));
    //            }
    //            else
    //            {
    //                property.Fields.Add(new DatabaseField(property.DbName, "timestamp without time zone", 6, 19, 0));
    //            }
    //        }
    //        else if (property.PropertyType.CanBeReference)
    //        {
    //            if (FileReader.DatabaseProvider == DatabaseProvider.SQLServer)
    //            {
    //                property.Fields.Add(new DatabaseField(property.DbName + MetadataTokens.RRef, "binary", 16));
    //            }
    //            else
    //            {
    //                property.Fields.Add(new DatabaseField(property.DbName + MetadataTokens.RRef, "bytea", 16));
    //            }
    //        }
    //    }
    //    private void ConfigureDatabaseFieldsForMultipleType(MetadataProperty property)
    //    {
    //        if (FileReader.DatabaseProvider == DatabaseProvider.SQLServer)
    //        {
    //            property.Fields.Add(new DatabaseField(property.DbName + "_" + MetadataTokens.TYPE, "binary", 1)
    //            {
    //                Purpose = FieldPurpose.Discriminator
    //            });
    //        }
    //        else
    //        {
    //            property.Fields.Add(new DatabaseField(property.DbName + "_" + MetadataTokens.TYPE, "bytea", 1)
    //            {
    //                Purpose = FieldPurpose.Discriminator
    //            });
    //        }
    //        if (property.PropertyType.CanBeString)
    //        {
    //            if (property.PropertyType.StringKind == StringKind.Fixed)
    //            {
    //                if (FileReader.DatabaseProvider == DatabaseProvider.SQLServer)
    //                {
    //                    property.Fields.Add(new DatabaseField(
    //                        property.DbName + "_" + MetadataTokens.S,
    //                        "nchar",
    //                        property.PropertyType.StringLength)
    //                    {
    //                        Purpose = FieldPurpose.String
    //                    });
    //                }
    //                else
    //                {
    //                    property.Fields.Add(new DatabaseField(
    //                        property.DbName + "_" + MetadataTokens.S,
    //                        "mchar",
    //                        property.PropertyType.StringLength)
    //                    {
    //                        Purpose = FieldPurpose.String
    //                    });
    //                }
    //            }
    //            else
    //            {
    //                if (FileReader.DatabaseProvider == DatabaseProvider.SQLServer)
    //                {
    //                    property.Fields.Add(new DatabaseField(
    //                        property.DbName + "_" + MetadataTokens.S,
    //                        "nvarchar",
    //                        property.PropertyType.StringLength)
    //                    {
    //                        Purpose = FieldPurpose.String
    //                    });
    //                }
    //                else
    //                {
    //                    property.Fields.Add(new DatabaseField(
    //                        property.DbName + "_" + MetadataTokens.S,
    //                        "mvarchar",
    //                        property.PropertyType.StringLength)
    //                    {
    //                        Purpose = FieldPurpose.String
    //                    });
    //                }
    //            }
    //        }
    //        if (property.PropertyType.CanBeNumeric)
    //        {
    //            // length can be updated from database
    //            property.Fields.Add(new DatabaseField(
    //                property.DbName + "_" + MetadataTokens.N,
    //                "numeric", 9,
    //                property.PropertyType.NumericPrecision,
    //                property.PropertyType.NumericScale)
    //            {
    //                Purpose = FieldPurpose.Numeric
    //            });

    //        }
    //        if (property.PropertyType.CanBeBoolean)
    //        {
    //            if (FileReader.DatabaseProvider == DatabaseProvider.SQLServer)
    //            {
    //                property.Fields.Add(new DatabaseField(property.DbName + "_" + MetadataTokens.L, "binary", 1)
    //                {
    //                    Purpose = FieldPurpose.Boolean
    //                });
    //            }
    //            else
    //            {
    //                property.Fields.Add(new DatabaseField(property.DbName + "_" + MetadataTokens.L, "boolean", 1)
    //                {
    //                    Purpose = FieldPurpose.Boolean
    //                });
    //            }
    //        }
    //        if (property.PropertyType.CanBeDateTime)
    //        {
    //            // length, precision and scale can be updated from database
    //            if (FileReader.DatabaseProvider == DatabaseProvider.SQLServer)
    //            {
    //                property.Fields.Add(new DatabaseField(property.DbName + "_" + MetadataTokens.T, "datetime2", 6, 19, 0)
    //                {
    //                    Purpose = FieldPurpose.DateTime
    //                });
    //            }
    //            else
    //            {
    //                property.Fields.Add(new DatabaseField(property.DbName + "_" + MetadataTokens.T, "timestamp without time zone", 6, 19, 0)
    //                {
    //                    Purpose = FieldPurpose.DateTime
    //                });
    //            }
    //        }
    //        if (property.PropertyType.CanBeReference)
    //        {
    //            if (property.PropertyType.ReferenceTypeUuid == Guid.Empty) // miltiple refrence type
    //            {
    //                if (FileReader.DatabaseProvider == DatabaseProvider.SQLServer)
    //                {
    //                    property.Fields.Add(new DatabaseField(property.DbName + "_" + MetadataTokens.RTRef, "binary", 4)
    //                    {
    //                        Purpose = FieldPurpose.TypeCode
    //                    });
    //                }
    //                else
    //                {
    //                    property.Fields.Add(new DatabaseField(property.DbName + "_" + MetadataTokens.RTRef, "bytea", 4)
    //                    {
    //                        Purpose = FieldPurpose.TypeCode
    //                    });
    //                }
    //            }
    //            if (FileReader.DatabaseProvider == DatabaseProvider.SQLServer)
    //            {
    //                property.Fields.Add(new DatabaseField(property.DbName + "_" + MetadataTokens.RRRef, "binary", 16)
    //                {
    //                    Purpose = FieldPurpose.Object
    //                });
    //            }
    //            else
    //            {
    //                property.Fields.Add(new DatabaseField(property.DbName + "_" + MetadataTokens.RRRef, "bytea", 16)
    //                {
    //                    Purpose = FieldPurpose.Object
    //                });
    //            }
    //        }
    //    }

    //    #region "Reference type system properties"

    //    public void ConfigurePropertyСсылка(ApplicationObject metaObject)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "Ссылка",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_IDRRef" : "_idrref")
    //        };
    //        property.PropertyType.IsUuid = true;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 16,
    //            TypeName = "binary",
    //            KeyOrdinal = 1,
    //            IsPrimaryKey = true
    //        });
    //        metaObject.Properties.Add(property);
    //    }
    //    public void ConfigurePropertyВерсияДанных(ApplicationObject metaObject)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "ВерсияДанных",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_Version" : "_version")
    //        };
    //        property.PropertyType.IsBinary = true;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 8,
    //            TypeName = "timestamp"
    //        });
    //        metaObject.Properties.Add(property);
    //    }
    //    public void ConfigurePropertyПометкаУдаления(ApplicationObject metaObject)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "ПометкаУдаления",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_Marked" : "_marked")
    //        };
    //        property.PropertyType.CanBeBoolean = true;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 1,
    //            TypeName = "binary"
    //        });
    //        metaObject.Properties.Add(property);
    //    }
    //    public void ConfigurePropertyПредопределённый(ApplicationObject metaObject)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "Предопределённый",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_PredefinedID" : "_predefinedid")
    //        };
    //        property.PropertyType.IsUuid = true;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 16,
    //            TypeName = "binary"
    //        });
    //        metaObject.Properties.Add(property);
    //    }
    //    public void ConfigurePropertyКод(ApplicationObject metaObject)
    //    {
    //        if (!(metaObject is IReferenceCode code)) throw new ArgumentOutOfRangeException();

    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "Код",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_Code" : "_code")
    //        };
    //        if (code.CodeType == CodeType.String)
    //        {
    //            property.PropertyType.CanBeString = true;
    //            property.Fields.Add(new DatabaseField()
    //            {
    //                Name = property.DbName,
    //                Length = code.CodeLength,
    //                TypeName = "nvarchar"
    //            });
    //        }
    //        else
    //        {
    //            property.PropertyType.CanBeNumeric = true;
    //            property.Fields.Add(new DatabaseField()
    //            {
    //                Name = property.DbName,
    //                Precision = code.CodeLength,
    //                TypeName = "numeric"
    //            });
    //        }
    //        metaObject.Properties.Add(property);
    //    }
    //    public void ConfigurePropertyНаименование(ApplicationObject metaObject)
    //    {
    //        if (!(metaObject is IDescription description)) throw new ArgumentOutOfRangeException();

    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "Наименование",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_Description" : "_description")
    //        };
    //        property.PropertyType.CanBeString = true;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = description.DescriptionLength,
    //            TypeName = "nvarchar"
    //        });
    //        metaObject.Properties.Add(property);
    //    }
    //    public void ConfigurePropertyРодитель(ApplicationObject metaObject)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "Родитель",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_ParentIDRRef" : "_parentidrref")
    //        };
    //        property.PropertyType.CanBeReference = true;
    //        property.PropertyType.ReferenceTypeUuid = metaObject.Uuid;
    //        //property.PropertyType.ReferenceTypeCode = metaObject.TypeCode;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 16,
    //            TypeName = "binary"
    //        });
    //        metaObject.Properties.Add(property);
    //    }
    //    public void ConfigurePropertyЭтоГруппа(ApplicationObject metaObject)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "ЭтоГруппа",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_Folder" : "_folder")
    //        };
    //        property.PropertyType.CanBeBoolean = true;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 1,
    //            TypeName = "binary"
    //        });
    //        metaObject.Properties.Add(property);
    //    }
    //    public void ConfigurePropertyВладелец(Catalog catalog, Guid owner)
    //    {
    //        MetadataProperty property = new MetadataProperty
    //        {
    //            Name = "Владелец",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_OwnerID" : "_ownerid")
    //        };
    //        property.PropertyType.CanBeReference = true;

    //        if (catalog.Owners == 1) // Single type value
    //        {
    //            property.PropertyType.ReferenceTypeUuid = owner;
    //            property.Fields.Add(new DatabaseField()
    //            {
    //                Name = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_OwnerIDRRef" : "_owneridrref"),
    //                Length = 16,
    //                TypeName = "binary"
    //            });
    //        }
    //        else // Multiple type value
    //        {
    //            property.Fields.Add(new DatabaseField()
    //            {
    //                Name = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_OwnerID_TYPE" : "_ownerid_type"),
    //                Length = 1,
    //                TypeName = "binary",
    //                Purpose = FieldPurpose.Discriminator
    //            });
    //            property.Fields.Add(new DatabaseField()
    //            {
    //                Name = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_OwnerID_RTRef" : "_ownerid_rtref"),
    //                Length = 4,
    //                TypeName = "binary",
    //                Purpose = FieldPurpose.TypeCode
    //            });
    //            property.Fields.Add(new DatabaseField()
    //            {
    //                Name = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_OwnerID_RRRef" : "_ownerid_rrref"),
    //                Length = 16,
    //                TypeName = "binary",
    //                Purpose = FieldPurpose.Object
    //            });
    //        }

    //        catalog.Properties.Add(property);
    //    }

    //    #endregion

    //    #region "Characteristic properties"

    //    public void ConfigurePropertyТипЗначения(Characteristic characteristic)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "ТипЗначения",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_Type" : "_type")
    //        };
    //        property.PropertyType.IsBinary = true;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = -1,
    //            IsNullable = true,
    //            TypeName = "varbinary"
    //        });
    //        characteristic.Properties.Add(property);
    //    }

    //    #endregion

    //    #region "Document system properties"

    //    public void ConfigurePropertyДата(Document document)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "Дата",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_Date_Time" : "_date_time")
    //        };
    //        property.PropertyType.CanBeDateTime = true;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 6,
    //            Precision = 19,
    //            TypeName = "datetime2"
    //        });
    //        document.Properties.Add(property);
    //    }
    //    public void ConfigurePropertyПериодичность(Document document)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "ПериодНомера",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_NumberPrefix" : "_numberprefix")
    //        };
    //        property.PropertyType.CanBeDateTime = true;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 6,
    //            Precision = 19,
    //            TypeName = "datetime2"
    //        });
    //        document.Properties.Add(property);
    //    }
    //    public void ConfigurePropertyПроведён(Document document)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "Проведён",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_Posted" : "_posted")
    //        };
    //        property.PropertyType.CanBeBoolean = true;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 1,
    //            TypeName = "binary"
    //        });
    //        document.Properties.Add(property);
    //    }
    //    public void ConfigurePropertyНомер(Document document)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "Номер",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_Number" : "_number")
    //        };
    //        if (document.NumberType == NumberType.Number)
    //        {
    //            property.PropertyType.CanBeNumeric = true;
    //            property.Fields.Add(new DatabaseField()
    //            {
    //                Name = property.DbName,
    //                Length = 6,
    //                Precision = 19,
    //                TypeName = "numeric"
    //            });
    //        }
    //        else
    //        {
    //            property.PropertyType.CanBeString = true;
    //            property.PropertyType.StringKind = StringKind.Fixed;
    //            property.PropertyType.StringLength = document.NumberLength;
    //            property.Fields.Add(new DatabaseField()
    //            {
    //                Name = property.DbName,
    //                Length = document.NumberLength,
    //                TypeName = "nvarchar"
    //            });
    //        }
    //        document.Properties.Add(property);
    //    }

    //    // Используется для сихронизации добавления свойства "Регистратор" между документами
    //    private readonly object syncRegister = new object();
    //    public void ConfigurePropertyРегистратор(ApplicationObject register, Document document)
    //    {
    //        lock (syncRegister)
    //        {
    //            ConfigurePropertyРегистраторSynchronized(register, document);
    //        }
    //    }

    //    public void ConfigureRegistersToPost(Document document, ConfigObject registers)
    //    {
    //        int registersCount = registers.GetInt32(new int[] { 1 }); // количество регистров
    //        if (registersCount == 0) return;

    //        int offset = 2;
    //        for (int r = 0; r < registersCount; r++)
    //        {
    //            // R.2.1 - uuid файла регистра
    //            Guid fileName = registers.GetUuid(new int[] { r + offset, 2, 1 });
    //            foreach (var collection in InfoBase.Registers)
    //            {
    //                if (collection.TryGetValue(fileName, out ApplicationObject register))
    //                {
    //                    ConfigurePropertyРегистратор(register, document);
    //                    break;
    //                }
    //            }
    //        }
    //    }

    //    #endregion

    //    #region "Publication system properties"

    //    public void ConfigurePropertyНомерОтправленного(Publication publication)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "НомерОтправленного",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_SentNo" : "_sentno")
    //        };
    //        property.PropertyType.CanBeNumeric = true;
    //        property.PropertyType.NumericScale = 0;
    //        property.PropertyType.NumericPrecision = 10;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 9,
    //            Scale = 0,
    //            Precision = 10,
    //            TypeName = "numeric"
    //        });
    //        publication.Properties.Add(property);
    //    }
    //    public void ConfigurePropertyНомерПринятого(Publication publication)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "НомерПринятого",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_ReceivedNo" : "_receivedno")
    //        };
    //        property.PropertyType.CanBeNumeric = true;
    //        property.PropertyType.NumericScale = 0;
    //        property.PropertyType.NumericPrecision = 10;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 9,
    //            Scale = 0,
    //            Precision = 10,
    //            TypeName = "numeric"
    //        });
    //        publication.Properties.Add(property);
    //    }

    //    #endregion

    //    #region "Enumeration system properties"

    //    public void ConfigurePropertyПорядок(Enumeration enumeration)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "Порядок",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_EnumOrder" : "_enumorder")
    //        };
    //        property.PropertyType.CanBeNumeric = true;
    //        property.PropertyType.NumericScale = 0;
    //        property.PropertyType.NumericPrecision = 10;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 9,
    //            Scale = 0,
    //            Precision = 10,
    //            TypeName = "numeric"
    //        });
    //        enumeration.Properties.Add(property);
    //    }

    //    #endregion



    //    #region "Accumulation register system properties"

    //    public void ConfigurePropertyВидДвижения(AccumulationRegister register)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "ВидДвижения",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_RecordKind" : "_recordkind")
    //        };
    //        property.PropertyType.CanBeNumeric = true;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 5,
    //            Precision = 1,
    //            TypeName = "numeric"
    //        });
    //        register.Properties.Add(property);
    //    }
    //    public void ConfigurePropertyDimHash(AccumulationRegister register)
    //    {
    //        MetadataProperty property = new MetadataProperty()
    //        {
    //            Name = "DimHash",
    //            FileName = Guid.Empty,
    //            Purpose = PropertyPurpose.System,
    //            DbName = (FileReader.DatabaseProvider == DatabaseProvider.SQLServer ? "_DimHash" : "_dimhash")
    //        };
    //        property.PropertyType.CanBeNumeric = true;
    //        property.Fields.Add(new DatabaseField()
    //        {
    //            Name = property.DbName,
    //            Length = 9,
    //            Precision = 10,
    //            TypeName = "numeric"
    //        });
    //        register.Properties.Add(property);
    //    }

    //    #endregion

    //    #region "Predefined values (catalogs and characteristics)"

    //    public void ConfigurePredefinedValues(ApplicationObject metaObject)
    //    {
    //        if (!(metaObject is IPredefinedValues owner)) return;

    //        int predefinedValueUuid = 3;
    //        int predefinedIsFolder = 4;
    //        int predefinedValueName = 6;
    //        int predefinedValueCode = 7;
    //        int predefinedDescription = 8;

    //        string fileName = metaObject.FileName.ToString() + ".1c"; // файл с описанием предопределённых элементов
    //        if (metaObject is Characteristic)
    //        {
    //            fileName = metaObject.FileName.ToString() + ".7";
    //            predefinedValueName = 5;
    //            predefinedValueCode = 6;
    //            predefinedDescription = 7;
    //        }

    //        IReferenceCode codeInfo = (metaObject as IReferenceCode);

    //        ConfigObject configObject = FileReader.ReadConfigObject(fileName);

    //        if (configObject == null) return;

    //        ConfigObject parentObject = configObject.GetObject(new int[] { 1, 2, 14, 2 });

    //        //string RootName = parentObject.GetString(new int[] { 6, 1 }); // имя корня предопределённых элементов = "Элементы" (уровень 0)
    //        //string RootName = parentObject.GetString(new int[] { 5, 1 }); // имя корня предопределённых элементов = "Характеристики" (уровень 0)

    //        int propertiesCount = parentObject.GetInt32(new int[] { 2 });
    //        int predefinedFlag = propertiesCount + 3;
    //        int childrenValues = propertiesCount + 4;

    //        int hasChildren = parentObject.GetInt32(new int[] { predefinedFlag }); // флаг наличия предопределённых элементов
    //        if (hasChildren == 0) return;

    //        ConfigObject predefinedValues = parentObject.GetObject(new int[] { childrenValues }); // коллекция описаний предопределённых элементов

    //        int valuesCount = predefinedValues.GetInt32(new int[] { 1 }); // количество предопределённых элементов (уровень 1)

    //        if (valuesCount == 0) return;

    //        int valueOffset = 2;
    //        for (int v = 0; v < valuesCount; v++)
    //        {
    //            PredefinedValue pv = new PredefinedValue();

    //            ConfigObject predefinedValue = predefinedValues.GetObject(new int[] { v + valueOffset });

    //            pv.Uuid = predefinedValue.GetUuid(new int[] { predefinedValueUuid, 2, 1 });
    //            pv.Name = predefinedValue.GetString(new int[] { predefinedValueName, 1 });
    //            pv.IsFolder = (predefinedValue.GetInt32(new int[] { predefinedIsFolder, 1 }) == 1);
    //            pv.Description = predefinedValue.GetString(new int[] { predefinedDescription, 1 });

    //            if (codeInfo != null && codeInfo.CodeLength > 0)
    //            {
    //                pv.Code = predefinedValue.GetString(new int[] { predefinedValueCode, 1 });
    //            }

    //            owner.PredefinedValues.Add(pv);

    //            int haveChildren = predefinedValue.GetInt32(new int[] { 9 }); // флаг наличия дочерних предопределённых элементов (0 - нет, 1 - есть)
    //            if (haveChildren == 1)
    //            {
    //                ConfigObject children = predefinedValue.GetObject(new int[] { 10 }); // коллекция описаний дочерних предопределённых элементов

    //                ConfigurePredefinedValue(children, pv, metaObject);
    //            }
    //        }
    //    }
    //    private void ConfigurePredefinedValue(ConfigObject predefinedValues, PredefinedValue parent, ApplicationObject owner)
    //    {
    //        int valuesCount = predefinedValues.GetInt32(new int[] { 1 }); // количество предопределённых элементов (уровень N)

    //        if (valuesCount == 0) return;

    //        int predefinedValueUuid = 3;
    //        int predefinedIsFolder = 4;
    //        int predefinedValueName = 6;
    //        int predefinedValueCode = 7;
    //        int predefinedDescription = 8;

    //        if (owner is Characteristic)
    //        {
    //            predefinedValueName = 5;
    //            predefinedValueCode = 6;
    //            predefinedDescription = 7;
    //        }

    //        IReferenceCode codeInfo = (owner as IReferenceCode);

    //        int valueOffset = 2;
    //        for (int v = 0; v < valuesCount; v++)
    //        {
    //            PredefinedValue pv = new PredefinedValue();

    //            ConfigObject predefinedValue = predefinedValues.GetObject(new int[] { v + valueOffset });

    //            pv.Uuid = predefinedValue.GetUuid(new int[] { predefinedValueUuid, 2, 1 });
    //            pv.Name = predefinedValue.GetString(new int[] { predefinedValueName, 1 });
    //            pv.IsFolder = (predefinedValue.GetInt32(new int[] { predefinedIsFolder, 1 }) == 1);
    //            pv.Description = predefinedValue.GetString(new int[] { predefinedDescription, 1 });

    //            if (codeInfo != null && codeInfo.CodeLength > 0)
    //            {
    //                pv.Code = predefinedValue.GetString(new int[] { predefinedValueCode, 1 });
    //            }

    //            parent.Children.Add(pv);

    //            int haveChildren = predefinedValue.GetInt32(new int[] { 9 }); // флаг наличия дочерних предопределённых элементов (0 - нет, 1 - есть)
    //            if (haveChildren == 1)
    //            {
    //                ConfigObject children = predefinedValue.GetObject(new int[] { 10 }); // коллекция описаний дочерних предопределённых элементов

    //                ConfigurePredefinedValue(children, pv, owner);
    //            }
    //        }
    //    }

    //    #endregion

    //    #region "VERSION 4.0.0"

    //    private const string ROOT_FILE_NAME = "root"; // Config
    //    private const string DBNAMES_FILE_NAME = "DBNames"; // Params

    //    private ConfigFileReader ConfigFileReader { get; }

    //    public Configurator(ConfigFileReader configFileReader)
    //    {
    //        ConfigFileReader = configFileReader ?? throw new ArgumentNullException(nameof(configFileReader));
    //    }

    //    public void OpenMetadata(in InfoBase infoBase, in List<string> errors)
    //    {
    //        infoBase.YearOffset = ConfigFileReader.YearOffset;
    //        infoBase.PlatformRequiredVersion = ConfigFileReader.PlatformVersion;

    //        if (!TryOpenConfigInfo(in infoBase, in errors))
    //        {
    //            return;
    //        }

    //        DbNamesLookup dbnames = new DbNamesLookup();
    //        if (!TryOpenDbNames(in dbnames, in errors))
    //        {
    //            return;
    //        }
    //        infoBase.DbNames = dbnames;

    //        ConfigureSharedProperties(in infoBase, in errors);

    //        // 0. TODO:
    //        // 1. Configure shared properties
    //        // 2. Configure named data type sets
    //        // 3. Read Enumerations
    //        // 4. Read Characteristics and configure CharacteristicTypes
    //        // 5. Read Catalogs
    //        // 6. Read Documents
    //        // 7. Read Publications
    //        // 8. Read InformationRegisters
    //        // 9. Read AccumulationRegisters
    //    }
    //    private bool TryOpenConfigInfo(in InfoBase infoBase, in List<string> errors)
    //    {
    //        bool success = true;

    //        try
    //        {
    //            string fileName = ConfigFileReader.ReadConfigObject(ConfigTableNames.Config, ROOT_FILE_NAME).GetString(1);

    //            using (ConfigFileReader reader = new ConfigFileReader(
    //                ConfigFileReader.ConnectionString, ConfigTableNames.Config, fileName))
    //            {
    //                new InfoBaseParser(reader).Parse(in infoBase);
    //            }
    //        }
    //        catch (Exception error)
    //        {
    //            success = false;
    //            errors.Add("Failed to open [" + ROOT_FILE_NAME + "]"
    //                + Environment.NewLine
    //                + ExceptionHelper.GetErrorMessageAndStackTrace(error));
    //        }

    //        return success;
    //    }
    //    private bool TryOpenDbNames(in DbNamesLookup dbnames, in List<string> errors)
    //    {
    //        bool success = true;

    //        try
    //        {
    //            using (ConfigFileReader reader = new ConfigFileReader(
    //                ConfigFileReader.ConnectionString, ConfigTableNames.Params, DBNAMES_FILE_NAME))
    //            {
    //                new DbNamesParser(reader).Parse(in dbnames);
    //            }
    //        }
    //        catch (Exception error)
    //        {
    //            success = false;
    //            errors.Add("Failed to open [" + DBNAMES_FILE_NAME + "]"
    //                + Environment.NewLine
    //                + ExceptionHelper.GetErrorMessageAndStackTrace(error));
    //        }

    //        return success;
    //    }
    //    private void ConfigureSharedProperties(in InfoBase infoBase, in List<string> errors)
    //    {
    //        foreach (var item in infoBase.SharedProperties)
    //        {
    //            try
    //            {
    //                item.Value.Uuid = item.Key;
    //                item.Value.FileName = item.Key;
    //                ConfigureSharedProperty(in infoBase, item.Value);
    //            }
    //            catch (Exception error)
    //            {
    //                errors.Add($"Failed to configure shared property [{item.Key}]"
    //                    + Environment.NewLine
    //                    + ExceptionHelper.GetErrorMessageAndStackTrace(error));
    //            }
    //        }
    //    }
    //    private void ConfigureSharedProperty(in InfoBase infoBase, in SharedProperty property)
    //    {
    //        using (ConfigFileReader reader = new ConfigFileReader(
    //                ConfigFileReader.ConnectionString, ConfigTableNames.Config, property.FileName.ToString()))
    //        {
    //            new SharedPropertyParser(reader).Parse(in infoBase, in property);
    //        }
    //    }

    //    #endregion
    //}
}