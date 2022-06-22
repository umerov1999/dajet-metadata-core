using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Core
{
    internal static class Configurator
    {
        internal static void ConfigureSystemProperties(in MetadataCache cache, in MetadataObject metadata)
        {
            if (metadata is Catalog catalog)
            {
                ConfigureCatalog(in cache, in catalog);
            }
            else if (metadata is Document document)
            {
                ConfigureDocument(in document);
            }
            else if (metadata is Enumeration enumeration)
            {
                ConfigureEnumeration(in enumeration);
            }
            else if (metadata is Publication publication)
            {
                ConfigurePublication(in publication);
            }
            else if (metadata is Characteristic characteristic)
            {
                ConfigureCharacteristic(in characteristic);
            }
            else if (metadata is InformationRegister register1)
            {
                ConfigureInformationRegister(in register1);
            }
            else if (metadata is AccumulationRegister register2)
            {
                ConfigureAccumulationRegister(in register2);
            }
        }
        internal static void ConfigureSharedProperties(in MetadataCache cache, in MetadataObject metadata)
        {
            if (metadata is not ApplicationObject target)
            {
                return;
            }

            foreach (SharedProperty property in cache.GetMetadataObjects(MetadataTypes.SharedProperty))
            {
                if (property.UsageSettings.TryGetValue(target.Uuid, out SharedPropertyUsage usage))
                {
                    if (usage == SharedPropertyUsage.Use)
                    {
                        target.Properties.Add(property);
                    }
                }
                else // Auto
                {
                    if (property.AutomaticUsage == AutomaticUsage.Use)
                    {
                        target.Properties.Add(property);
                    }
                }
            }
        }
        internal static void ConfigureDataTypeSet(in MetadataCache cache, in DataTypeSet target, in List<Guid> references)
        {
            if (references == null || references.Count == 0)
            {
                return;
            }

            int count = 0;
            Guid reference = Guid.Empty;

            for (int i = 0; i < references.Count; i++)
            {
                reference = references[i];

                if (reference == Guid.Empty) { continue; }

                count += ResolveAndCountReferenceTypes(in cache, in target, reference);

                if (count > 1) { break; }
            }

            if (count == 0) // zero reference types
            {
                target.CanBeReference = false;
                target.Reference = Guid.Empty;
                return; 
            }

            if (count == 1) // single reference type
            {
                target.CanBeReference = true;

                if (cache.TryGetReferenceInfo(reference, out MetadataEntry entry))
                {
                    target.Reference = entry.MetadataUuid; // uuid объекта метаданных
                }
                else
                {
                    // unsupported reference type, например "БизнесПроцесс"
                    target.Reference = reference;
                }
            }
            else // multiple reference type
            {
                target.CanBeReference = true;
                target.Reference = Guid.Empty;
            }
        }
        private static int ResolveAndCountReferenceTypes(in MetadataCache cache, in DataTypeSet target, Guid reference)
        {
            // RULES (правила разрешения ссылочных типов данных для объекта "ОписаниеТипов"):
            // 1. DataTypeSet (property type) can have only one reference to NamedDataTypeSet or Characteristic
            //    Additional references to another data types are not allowed in this case. (!)
            // 2. NamedDataTypeSet and Characteristic can not reference them self or each other. (!)
            // 3. Если ссылочный тип имеет значение, например, "СправочникСсылка", то есть любой справочник,
            //    в таком случае необходимо вычислить количество справочников в составе конфигурации:
            //    если возможным справочником будет только один, то это будет single reference type. (!)
            // 4. То же самое, что и для пункта #3, касается значения типа "ЛюбаяСсылка". (!)

            if (cache.TryGetReferenceInfo(reference, out MetadataEntry info))
            {
                if (info.MetadataType == MetadataTypes.NamedDataTypeSet ||
                    (info.MetadataType == MetadataTypes.Characteristic && cache.IsCharacteristic(reference)))
                {
                    // Lazy-load of NamedDataTypeSets and Characteristics: recursion is avoided because of rule #2.
                    // THINK: Pre-load NamedDataTypeSets and Characteristics !?
                    MetadataObject metadata = cache.GetMetadataObjectCached(info.MetadataType, info.MetadataUuid);

                    if (metadata is NamedDataTypeSet namedSet)
                    {
                        target.Apply(namedSet.DataTypeSet);
                    }
                    else if (metadata is Characteristic characteristic)
                    {
                        target.Apply(characteristic.DataTypeSet);
                    }

                    if (target.CanBeReference && target.Reference == Guid.Empty)
                    {
                        return 2; // multiple reference type (may be more then 2 in fact)
                    }
                }
                else
                {
                    return 1; // single reference type
                }
            }

            int count = 0;

            if (reference == ReferenceTypes.Catalog)
            {
                count = cache.CountMetadataObjects(MetadataTypes.Catalog);
            }
            else if (reference == ReferenceTypes.Document)
            {
                count = cache.CountMetadataObjects(MetadataTypes.Document);
            }
            else if (reference == ReferenceTypes.Enumeration)
            {
                count = cache.CountMetadataObjects(MetadataTypes.Enumeration);
            }
            else if (reference == ReferenceTypes.Publication)
            {
                count = cache.CountMetadataObjects(MetadataTypes.Publication);
            }
            else if (reference == ReferenceTypes.Characteristic)
            {
                count = cache.CountMetadataObjects(MetadataTypes.Characteristic);
            }
            else if (reference == ReferenceTypes.AnyReference)
            {
                count += cache.CountMetadataObjects(MetadataTypes.Catalog);
                if (count > 1) { return count; }
                count += cache.CountMetadataObjects(MetadataTypes.Document);
                if (count > 1) { return count; }
                count += cache.CountMetadataObjects(MetadataTypes.Enumeration);
                if (count > 1) { return count; }
                count += cache.CountMetadataObjects(MetadataTypes.Publication);
                if (count > 1) { return count; }
                count += cache.CountMetadataObjects(MetadataTypes.Characteristic);
                if (count > 1) { return count; }
            }

            return count;
        }

        #region "TABLE PARTS"

        internal static void ConfigureTableParts(in MetadataCache cache, in ApplicationObject owner)
        {
            if (owner is not IAggregate aggregate)
            {
                return;
            }

            foreach (TablePart tablePart in aggregate.TableParts)
            {
                tablePart.Owner = owner;
                ConfigurePropertyСсылка(in owner, in tablePart);
                ConfigurePropertyКлючСтроки(in tablePart);
                ConfigurePropertyНомерСтроки(in cache, in tablePart);
            }
        }
        private static void ConfigurePropertyСсылка(in ApplicationObject owner, in TablePart tablePart)
        {
            MetadataProperty property = new MetadataProperty()
            {
                Name = "Ссылка",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = owner.TableName + "_IDRRef"
            };
            property.PropertyType.IsUuid = true;

            property.Fields.Add(new DatabaseField()
            {
                Name = property.DbName,
                Length = 16,
                TypeName = "binary",
                KeyOrdinal = 1,
                IsPrimaryKey = true
            });

            tablePart.Properties.Add(property);
        }
        private static void ConfigurePropertyКлючСтроки(in TablePart tablePart)
        {
            MetadataProperty property = new MetadataProperty()
            {
                Name = "КлючСтроки",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = "_KeyField"
            };
            property.PropertyType.IsBinary = true;

            property.Fields.Add(new DatabaseField()
            {
                Name = property.DbName,
                Length = 4,
                TypeName = "binary",
                KeyOrdinal = 2,
                IsPrimaryKey = true
            });

            tablePart.Properties.Add(property);
        }
        private static void ConfigurePropertyНомерСтроки(in MetadataCache cache, in TablePart tablePart)
        {
            DbName db = cache.GetLineNo(tablePart.Uuid);

            MetadataProperty property = new MetadataProperty()
            {
                Name = "НомерСтроки",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = CreateDbName(db.Name, db.Code)
            };
            property.PropertyType.CanBeNumeric = true;
            property.PropertyType.NumericKind = NumericKind.AlwaysPositive;
            property.PropertyType.NumericPrecision = 5;

            property.Fields.Add(new DatabaseField()
            {
                Name = property.DbName,
                Length = 5,
                Precision = 5,
                TypeName = "numeric"
            });

            tablePart.Properties.Add(property);
        }

        #endregion

        #region "ENUMERATION"

        private static void ConfigureEnumeration(in Enumeration enumeration)
        {

        }

        #endregion

        #region "CATALOG"

        // Последовательность сериализации системных свойств в формат 1С JDTO
        // 1. ЭтоГруппа        = IsFolder           - bool (invert)
        // 2. Ссылка           = Ref                - uuid 
        // 3. ПометкаУдаления  = DeletionMark       - bool
        // 4. Владелец         = Owner              - { #type + #value }
        // 5. Родитель         = Parent             - uuid
        // 6. Код              = Code               - string | number
        // 7. Наименование     = Description        - string
        // 8. Предопределённый = PredefinedDataName - string

        private static void ConfigureCatalog(in MetadataCache cache, in Catalog catalog)
        {
            if (catalog.IsHierarchical)
            {
                if (catalog.HierarchyType == HierarchyType.Groups)
                {
                    ConfigurePropertyЭтоГруппа(catalog);
                }
            }

            ConfigurePropertyСсылка(catalog);
            ConfigurePropertyПометкаУдаления(catalog);

            List<Guid> owners = cache.GetCatalogOwners(catalog.Uuid);

            if (owners != null && owners.Count > 0)
            {
                ConfigurePropertyВладелец(in catalog, in owners);
            }

            if (catalog.IsHierarchical)
            {
                ConfigurePropertyРодитель(catalog);
            }

            if (catalog.CodeLength > 0)
            {
                ConfigurePropertyКод(catalog);
            }

            if (catalog.DescriptionLength > 0)
            {
                ConfigurePropertyНаименование(catalog);
            }

            ConfigurePropertyПредопределённый(catalog);

            ConfigurePropertyВерсияДанных(catalog);
        }
        private static void ConfigurePropertyСсылка(in ApplicationObject metadata)
        {
            MetadataProperty property = new MetadataProperty()
            {
                Name = "Ссылка",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = "_IDRRef"
            };
            property.PropertyType.IsUuid = true;

            property.Fields.Add(new DatabaseField()
            {
                Name = property.DbName,
                Length = 16,
                TypeName = "binary",
                KeyOrdinal = 1,
                IsPrimaryKey = true
            });

            metadata.Properties.Add(property);
        }
        private static void ConfigurePropertyВерсияДанных(in ApplicationObject metadata)
        {
            MetadataProperty property = new MetadataProperty()
            {
                Name = "ВерсияДанных",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = "_Version"
            };
            property.PropertyType.IsBinary = true;

            property.Fields.Add(new DatabaseField()
            {
                Name = property.DbName,
                Length = 8,
                TypeName = "timestamp"
            });

            metadata.Properties.Add(property);
        }
        private static void ConfigurePropertyПометкаУдаления(in ApplicationObject metadata)
        {
            MetadataProperty property = new MetadataProperty()
            {
                Name = "ПометкаУдаления",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = "_Marked"
            };
            property.PropertyType.CanBeBoolean = true;

            property.Fields.Add(new DatabaseField()
            {
                Name = property.DbName,
                Length = 1,
                TypeName = "binary"
            });

            metadata.Properties.Add(property);
        }
        private static void ConfigurePropertyПредопределённый(in ApplicationObject metadata)
        {
            //FIXME: version 8.2 (?) used _IsMetadata property of boolean type instead of this one !!!
            // Свойство "ИмяПредопределенныхДанных" доступно, начиная с версии 8.3.3

            MetadataProperty property = new MetadataProperty()
            {
                Name = "Предопределённый",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = "_PredefinedID"
            };
            property.PropertyType.IsUuid = true;

            property.Fields.Add(new DatabaseField()
            {
                Name = property.DbName,
                Length = 16,
                TypeName = "binary"
            });

            metadata.Properties.Add(property);
        }
        private static void ConfigurePropertyКод(in ApplicationObject metadata)
        {
            if (metadata is not IReferenceCode code)
            {
                throw new InvalidOperationException($"Metadata object \"{metadata.Name}\" does not implement IReferenceCode interface.");
            }

            MetadataProperty property = new MetadataProperty()
            {
                Name = "Код",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = "_Code"
            };

            if (code.CodeType == CodeType.String)
            {
                property.PropertyType.CanBeString = true;
                property.PropertyType.StringLength = code.CodeLength;

                property.Fields.Add(new DatabaseField()
                {
                    Name = property.DbName,
                    Length = code.CodeLength,
                    TypeName = "nvarchar"
                });
            }
            else
            {
                property.PropertyType.CanBeNumeric = true;
                property.PropertyType.NumericKind = NumericKind.AlwaysPositive;
                property.PropertyType.NumericPrecision = code.CodeLength;

                property.Fields.Add(new DatabaseField()
                {
                    Name = property.DbName,
                    Precision = code.CodeLength,
                    TypeName = "numeric"
                });
            }

            metadata.Properties.Add(property);
        }
        private static void ConfigurePropertyНаименование(in ApplicationObject metadata)
        {
            if (metadata is not IDescription description)
            {
                throw new InvalidOperationException($"Metadata object \"{metadata.Name}\" does not implement IDescription interface.");
            }

            MetadataProperty property = new MetadataProperty()
            {
                Name = "Наименование",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = "_Description"
            };
            property.PropertyType.CanBeString = true;
            property.PropertyType.StringLength = description.DescriptionLength;

            property.Fields.Add(new DatabaseField()
            {
                Name = property.DbName,
                Length = description.DescriptionLength,
                TypeName = "nvarchar"
            });

            metadata.Properties.Add(property);
        }
        private static void ConfigurePropertyРодитель(in ApplicationObject metadata)
        {
            MetadataProperty property = new MetadataProperty()
            {
                Name = "Родитель",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = "_ParentIDRRef"
            };
            property.PropertyType.CanBeReference = true;
            property.PropertyType.Reference = metadata.Uuid; // single reference type
            
            property.Fields.Add(new DatabaseField()
            {
                Name = property.DbName,
                Length = 16,
                TypeName = "binary"
            });

            metadata.Properties.Add(property);
        }
        private static void ConfigurePropertyЭтоГруппа(in ApplicationObject metadata)
        {
            MetadataProperty property = new MetadataProperty()
            {
                Name = "ЭтоГруппа",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = "_Folder"
            };
            property.PropertyType.CanBeBoolean = true;

            property.Fields.Add(new DatabaseField()
            {
                Name = property.DbName,
                Length = 1,
                TypeName = "binary"
            });

            metadata.Properties.Add(property);
        }
        private static void ConfigurePropertyВладелец(in Catalog catalog, in List<Guid> owners)
        {
            MetadataProperty property = new MetadataProperty
            {
                Name = "Владелец",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = "_OwnerID"
            };
            property.PropertyType.CanBeReference = true;

            if (owners.Count == 1) // Single type value
            {
                property.PropertyType.Reference = owners[0]; // FIXME (?) owner is always metadata object uuid
                
                property.Fields.Add(new DatabaseField()
                {
                    Name = "_OwnerIDRRef",
                    Length = 16,
                    TypeName = "binary"
                });
            }
            else // Multiple type value
            {
                property.PropertyType.Reference = Guid.Empty;

                property.Fields.Add(new DatabaseField()
                {
                    Name = "_OwnerID_TYPE",
                    Length = 1,
                    TypeName = "binary",
                    Purpose = FieldPurpose.Discriminator
                });
                property.Fields.Add(new DatabaseField()
                {
                    Name = "_OwnerID_RTRef",
                    Length = 4,
                    TypeName = "binary",
                    Purpose = FieldPurpose.TypeCode
                });
                property.Fields.Add(new DatabaseField()
                {
                    Name = "_OwnerID_RRRef",
                    Length = 16,
                    TypeName = "binary",
                    Purpose = FieldPurpose.Object
                });
            }

            catalog.Properties.Add(property);
        }

        #endregion

        #region "CHARACTERISTIC"

        private static void ConfigureCharacteristic(in Characteristic characteristic)
        {
            if (characteristic.IsHierarchical)
            {
                if (characteristic.HierarchyType == HierarchyType.Groups)
                {
                    ConfigurePropertyЭтоГруппа(characteristic);
                }
            }

            ConfigurePropertyСсылка(characteristic);
            ConfigurePropertyПометкаУдаления(characteristic);

            if (characteristic.IsHierarchical)
            {
                ConfigurePropertyРодитель(characteristic);
            }

            if (characteristic.CodeLength > 0)
            {
                ConfigurePropertyКод(characteristic);
            }

            if (characteristic.DescriptionLength > 0)
            {
                ConfigurePropertyНаименование(characteristic);
            }

            ConfigurePropertyПредопределённый(characteristic);

            ConfigurePropertyТипЗначения(in characteristic);

            ConfigurePropertyВерсияДанных(characteristic);
        }
        private static void ConfigurePropertyТипЗначения(in Characteristic characteristic)
        {
            MetadataProperty property = new MetadataProperty()
            {
                Name = "ТипЗначения",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = "_Type"
            };
            property.PropertyType.IsBinary = true;

            property.Fields.Add(new DatabaseField()
            {
                Name = property.DbName,
                Length = -1,
                IsNullable = true,
                TypeName = "varbinary"
            });

            characteristic.Properties.Add(property);
        }

        #endregion

        #region "PUBLICATION"

        private static void ConfigurePublication(in Publication publication)
        {

        }

        #endregion

        #region "DOCUMENT"

        // Последовательность сериализации системных свойств в формат 1С JDTO
        // 1. Ссылка          = Ref          - uuid
        // 2. ПометкаУдаления = DeletionMark - bool
        // 3. Дата            = Date         - DateTime
        // 4. Номер           = Number       - string | number
        // 5. Проведён        = Posted       - bool

        private static void ConfigureDocument(in Document document)
        {

        }

        #endregion

        #region "INFORMATION REGISTER"

        // Последовательность сериализации системных свойств в формат 1С JDTO
        // 1. "Регистратор" = Recorder   - uuid { #type + #value }
        // 2. "Период"      = Period     - DateTime
        // 3. "ВидДвижения" = RecordType - string { "Receipt", "Expense" }
        // 4. "Активность"  = Active     - bool

        private static void ConfigureInformationRegister(in InformationRegister register)
        {
            if (register == null)
            {
                return;
            }

            if (register.UseRecorder)
            {
                // Описание типов свойства "Регистратор" конфигурируется после чтения метаданных документов:
                // именно объект метаданных "Документ" содержит ссылки на свои регистры движения.
                ConfigurePropertyРегистратор(register);
            }

            if (register.Periodicity != RegisterPeriodicity.None)
            {
                ConfigurePropertyПериод(register);
            }

            if (register.UseRecorder)
            {
                ConfigurePropertyАктивность(register);
                ConfigurePropertyНомерЗаписи(register);
            }
        }
        private static void ConfigurePropertyПериод(in ApplicationObject register)
        {
            MetadataProperty property = new()
            {
                Name = "Период",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = "_Period"
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
        private static void ConfigurePropertyНомерЗаписи(in ApplicationObject register)
        {
            MetadataProperty property = new()
            {
                Name = "НомерСтроки",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = "_LineNo"
            };
            property.PropertyType.CanBeNumeric = true;
            property.PropertyType.NumericPrecision = 9;
            property.PropertyType.NumericScale = 0;
            property.PropertyType.NumericKind = NumericKind.AlwaysPositive;

            property.Fields.Add(new DatabaseField()
            {
                Name = property.DbName,
                Length = 5,
                Scale = 0,
                Precision = 9,
                TypeName = "numeric"
            });
            
            register.Properties.Add(property);
        }
        private static void ConfigurePropertyАктивность(in ApplicationObject register)
        {
            MetadataProperty property = new MetadataProperty()
            {
                Name = "Активность",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = "_Active"
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
        private static void ConfigurePropertyРегистратор(in ApplicationObject register)
        {
            MetadataProperty property = new MetadataProperty()
            {
                Uuid = Guid.Empty,
                Name = "Регистратор",
                Purpose = PropertyPurpose.System,
                DbName = "_Recorder"
            };
            property.PropertyType.CanBeReference = true;

            property.Fields.Add(new DatabaseField()
            {
                Name = "_RecorderRRef",
                Length = 16,
                TypeName = "binary",
                IsPrimaryKey = true,
                Purpose = FieldPurpose.Value
            });

            register.Properties.Add(property); // TODO: register.Properties.Insert(0, property); ???

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

        #region "ACCUMULATION REGISTER"

        private static void ConfigureAccumulationRegister(in AccumulationRegister register)
        {

        }

        #endregion

        #region "Predefined values (catalogs and characteristics)"

        public static void ConfigurePredefinedValues(in MetadataCache cache, in MetadataObject metadata)
        {
            if (metadata is not IPredefinedValues owner) return;

            int predefinedValueUuid = 3;
            int predefinedIsFolder = 4;
            int predefinedValueName = 6;
            int predefinedValueCode = 7;
            int predefinedDescription = 8;

            string fileName = metadata.Uuid.ToString() + ".1c"; // файл с описанием предопределённых элементов
            if (metadata is Characteristic)
            {
                fileName = metadata.Uuid.ToString() + ".7";
                predefinedValueName = 5;
                predefinedValueCode = 6;
                predefinedDescription = 7;
            }

            IReferenceCode codeInfo = (metadata as IReferenceCode);

            ConfigObject configObject;

            using (ConfigFileReader reader = new(cache.DatabaseProvider, cache.ConnectionString, ConfigTables.Config, fileName))
            {
                configObject = new ConfigFileParser().Parse(reader);
            }

            if (configObject == null) return;

            ConfigObject parentObject = configObject.GetObject(new int[] { 1, 2, 14, 2 });

            //string RootName = parentObject.GetString(new int[] { 6, 1 }); // имя корня предопределённых элементов = "Элементы" (уровень 0)
            //string RootName = parentObject.GetString(new int[] { 5, 1 }); // имя корня предопределённых элементов = "Характеристики" (уровень 0)

            int propertiesCount = parentObject.GetInt32(new int[] { 2 });
            int predefinedFlag = propertiesCount + 3;
            int childrenValues = propertiesCount + 4;

            int hasChildren = parentObject.GetInt32(new int[] { predefinedFlag }); // флаг наличия предопределённых элементов
            if (hasChildren == 0) return;

            ConfigObject predefinedValues = parentObject.GetObject(new int[] { childrenValues }); // коллекция описаний предопределённых элементов

            int valuesCount = predefinedValues.GetInt32(new int[] { 1 }); // количество предопределённых элементов (уровень 1)

            if (valuesCount == 0) return;

            int valueOffset = 2;
            for (int v = 0; v < valuesCount; v++)
            {
                PredefinedValue pv = new PredefinedValue();

                ConfigObject predefinedValue = predefinedValues.GetObject(new int[] { v + valueOffset });

                pv.Uuid = predefinedValue.GetUuid(new int[] { predefinedValueUuid, 2, 1 });
                pv.Name = predefinedValue.GetString(new int[] { predefinedValueName, 1 });
                pv.IsFolder = (predefinedValue.GetInt32(new int[] { predefinedIsFolder, 1 }) == 1);
                pv.Description = predefinedValue.GetString(new int[] { predefinedDescription, 1 });

                if (codeInfo != null && codeInfo.CodeLength > 0)
                {
                    pv.Code = predefinedValue.GetString(new int[] { predefinedValueCode, 1 });
                }

                owner.PredefinedValues.Add(pv);

                int haveChildren = predefinedValue.GetInt32(new int[] { 9 }); // флаг наличия дочерних предопределённых элементов (0 - нет, 1 - есть)
                if (haveChildren == 1)
                {
                    ConfigObject children = predefinedValue.GetObject(new int[] { 10 }); // коллекция описаний дочерних предопределённых элементов

                    ConfigurePredefinedValue(children, pv, metadata);
                }
            }
        }
        private static void ConfigurePredefinedValue(ConfigObject predefinedValues, PredefinedValue parent, MetadataObject owner)
        {
            int valuesCount = predefinedValues.GetInt32(new int[] { 1 }); // количество предопределённых элементов (уровень N)

            if (valuesCount == 0) return;

            int predefinedValueUuid = 3;
            int predefinedIsFolder = 4;
            int predefinedValueName = 6;
            int predefinedValueCode = 7;
            int predefinedDescription = 8;

            if (owner is Characteristic)
            {
                predefinedValueName = 5;
                predefinedValueCode = 6;
                predefinedDescription = 7;
            }

            IReferenceCode codeInfo = (owner as IReferenceCode);

            int valueOffset = 2;
            for (int v = 0; v < valuesCount; v++)
            {
                PredefinedValue pv = new PredefinedValue();

                ConfigObject predefinedValue = predefinedValues.GetObject(new int[] { v + valueOffset });

                pv.Uuid = predefinedValue.GetUuid(new int[] { predefinedValueUuid, 2, 1 });
                pv.Name = predefinedValue.GetString(new int[] { predefinedValueName, 1 });
                pv.IsFolder = (predefinedValue.GetInt32(new int[] { predefinedIsFolder, 1 }) == 1);
                pv.Description = predefinedValue.GetString(new int[] { predefinedDescription, 1 });

                if (codeInfo != null && codeInfo.CodeLength > 0)
                {
                    pv.Code = predefinedValue.GetString(new int[] { predefinedValueCode, 1 });
                }

                parent.Children.Add(pv);

                int haveChildren = predefinedValue.GetInt32(new int[] { 9 }); // флаг наличия дочерних предопределённых элементов (0 - нет, 1 - есть)
                if (haveChildren == 1)
                {
                    ConfigObject children = predefinedValue.GetObject(new int[] { 10 }); // коллекция описаний дочерних предопределённых элементов

                    ConfigurePredefinedValue(children, pv, owner);
                }
            }
        }

        #endregion

        #region "CONFIGURE DATABASE NAMES"

        private static string CreateDbName(string token, int code)
        {
            return $"_{token}{code}";

            //if (_provider == DatabaseProvider.SQLServer)
            //{
            //    return $"_{token}{code}";
            //}
            //
            //return $"_{token}{code}".ToLowerInvariant();
        }
        internal static void ConfigureDatabaseNames(in MetadataCache cache, in MetadataObject metadata)
        {
            DbName db = cache.GetDbName(metadata.Uuid);

            if (metadata is SharedProperty property)
            {
                property.DbName = CreateDbName(db.Name, db.Code);

                ConfigureDatabaseFields(in cache, property);
                
                return;
            }

            if (metadata is not ApplicationObject entity)
            {
                return;
            }

            entity.TableName = CreateDbName(db.Name, db.Code);

            ConfigureDatabaseProperties(in cache, in entity);
            
            ConfigureDatabaseTableParts(in cache, in entity);
        }
        private static void ConfigureDatabaseProperties(in MetadataCache cache, in ApplicationObject entity)
        {
            foreach (MetadataProperty property in entity.Properties)
            {
                if (property is SharedProperty)
                {
                    continue;
                }

                if (property.Purpose == PropertyPurpose.System)
                {
                    continue;
                }
                
                DbName db = cache.GetDbName(property.Uuid);

                property.DbName = CreateDbName(db.Name, db.Code);

                ConfigureDatabaseFields(in cache, in property);
            }
        }
        private static void ConfigureDatabaseTableParts(in MetadataCache cache, in ApplicationObject entity)
        {
            if (entity is not IAggregate aggregate)
            {
                return;
            }

            foreach (TablePart tablePart in aggregate.TableParts)
            {
                DbName db = cache.GetDbName(tablePart.Uuid);

                tablePart.TableName = entity.TableName + CreateDbName(db.Name, db.Code);

                ConfigureDatabaseProperties(in cache, tablePart);
            }
        }
        private static void ConfigureDatabaseFields(in MetadataCache cache, in MetadataProperty property)
        {
            if (property.PropertyType.IsMultipleType)
            {
                ConfigureDatabaseFieldsForMultipleType(in property);
            }
            else
            {
                ConfigureDatabaseFieldsForSingleType(in property);
            }
        }
        private static void ConfigureDatabaseFieldsForSingleType(in MetadataProperty property)
        {
            if (property.PropertyType.IsUuid)
            {
                property.Fields.Add(new DatabaseField(property.DbName, "binary", 16)); // bytea
            }
            else if (property.PropertyType.IsBinary)
            {
                // is used only for system properties of system types
                // TODO: log if it happens eventually
            }
            else if (property.PropertyType.IsValueStorage)
            {
                property.Fields.Add(new DatabaseField(property.DbName, "varbinary", -1)); // bytea
            }
            else if (property.PropertyType.CanBeString)
            {
                if (property.PropertyType.StringKind == StringKind.Fixed)
                {
                    property.Fields.Add(new DatabaseField(property.DbName, "nchar", property.PropertyType.StringLength)); // mchar
                }
                else
                {
                    property.Fields.Add(new DatabaseField(property.DbName, "nvarchar", property.PropertyType.StringLength)); // mvarchar
                }
            }
            else if (property.PropertyType.CanBeNumeric)
            {
                // length can be updated from database
                property.Fields.Add(new DatabaseField(
                    property.DbName,
                    "numeric", 9,
                    property.PropertyType.NumericPrecision,
                    property.PropertyType.NumericScale));
            }
            else if (property.PropertyType.CanBeBoolean)
            {
                property.Fields.Add(new DatabaseField(property.DbName, "binary", 1)); // boolean
            }
            else if (property.PropertyType.CanBeDateTime)
            {
                // length, precision and scale can be updated from database
                property.Fields.Add(new DatabaseField(property.DbName, "datetime2", 6, 19, 0)); // "timestamp without time zone"
            }
            else if (property.PropertyType.CanBeReference)
            {
                property.Fields.Add(new DatabaseField(property.DbName + MetadataTokens.RRef, "binary", 16)); // bytea
            }
        }
        private static void ConfigureDatabaseFieldsForMultipleType(in MetadataProperty property)
        {
            property.Fields.Add(new DatabaseField(property.DbName + "_" + MetadataTokens.TYPE, "binary", 1)
            {
                Purpose = FieldPurpose.Discriminator
            });

            if (property.PropertyType.CanBeBoolean)
            {
                property.Fields.Add(new DatabaseField(property.DbName + "_" + MetadataTokens.L, "binary", 1)
                {
                    Purpose = FieldPurpose.Boolean
                });
            }

            if (property.PropertyType.CanBeNumeric)
            {
                // length can be updated from database
                property.Fields.Add(new DatabaseField(
                    property.DbName + "_" + MetadataTokens.N,
                    "numeric", 9,
                    property.PropertyType.NumericPrecision,
                    property.PropertyType.NumericScale)
                {
                    Purpose = FieldPurpose.Numeric
                });
            }

            if (property.PropertyType.CanBeDateTime)
            {
                // length, precision and scale can be updated from database
                property.Fields.Add(new DatabaseField(property.DbName + "_" + MetadataTokens.T, "datetime2", 6, 19, 0)
                {
                    Purpose = FieldPurpose.DateTime
                });
            }

            if (property.PropertyType.CanBeString)
            {
                if (property.PropertyType.StringKind == StringKind.Fixed)
                {
                    
                    property.Fields.Add(new DatabaseField(
                        property.DbName + "_" + MetadataTokens.S,
                        "nchar",
                        property.PropertyType.StringLength)
                    {
                        Purpose = FieldPurpose.String
                    });
                }
                else
                {
                    
                    property.Fields.Add(new DatabaseField(
                        property.DbName + "_" + MetadataTokens.S,
                        "nvarchar",
                        property.PropertyType.StringLength)
                    {
                        Purpose = FieldPurpose.String
                    });
                }
            }
            
            if (property.PropertyType.CanBeReference)
            {
                if (property.PropertyType.Reference == Guid.Empty) // miltiple refrence type
                {
                    property.Fields.Add(new DatabaseField(property.DbName + "_" + MetadataTokens.RTRef, "binary", 4)
                    {
                        Purpose = FieldPurpose.TypeCode
                    });
                }
                
                property.Fields.Add(new DatabaseField(property.DbName + "_" + MetadataTokens.RRRef, "binary", 16)
                {
                    Purpose = FieldPurpose.Object
                });
            }
        }

        #endregion
    }
}