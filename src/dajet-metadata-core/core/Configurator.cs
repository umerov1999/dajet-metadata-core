using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Core
{
    internal static class Configurator
    {
        internal static void ConfigureSystemProperties(in InfoBaseCache cache, in MetadataObject metadata)
        {
            if (metadata is InformationRegister register)
            {
                ConfigureInformationRegister(in register, cache.DatabaseProvider);
            }
        }
        internal static void ConfigureSharedProperties(in InfoBaseCache cache, in MetadataObject metadata)
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
        internal static void ConfigureMetadataProperties(in InfoBaseCache cache, in MetadataObject metadata, in Dictionary<MetadataProperty, List<Guid>> references)
        {
            if (metadata is not ApplicationObject entity)
            {
                return;
            }

            foreach (MetadataProperty property in entity.Properties)
            {
                if (references.TryGetValue(property, out List<Guid> referenceTypes))
                {
                    Configurator.ConfigureReferenceTypes(in cache, property.PropertyType, in referenceTypes);
                }
            }
        }
        internal static void ConfigureReferenceTypes(in InfoBaseCache cache, in DataTypeSet target, in List<Guid> references)
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

                count += ResolveAndCountReferenceType(in cache, in target, reference);

                if (count > 1) { break; }
            }

            if (count == 0)
            {
                return; // zero reference types
            }

            if (count == 1)
            {
                target.CanBeReference = true;
                target.Reference = reference; // single reference type
            }
            else
            {
                target.CanBeReference = true; // multiple reference type
            }
        }
        private static int ResolveAndCountReferenceType(in InfoBaseCache cache, in DataTypeSet target, Guid reference)
        {
            if (cache.TryGetReferenceInfo(reference, out ReferenceInfo info))
            {
                if (info.MetadataType == MetadataTypes.NamedDataTypeSet ||
                    (info.MetadataType == MetadataTypes.Characteristic && reference == info.CharacteristicUuid))
                {
                    MetadataObject metadata = cache.GetMetadataObjectCached(info.MetadataType, info.MetadataUuid);

                    if (metadata is NamedDataTypeSet source)
                    {
                        target.Apply(source.DataTypeSet);
                    }
                    else if (metadata is Characteristic characteristic)
                    {
                        target.Apply(characteristic.TypeInfo);
                    }

                    if (target.CanBeReference && target.Reference == Guid.Empty)
                    {
                        return 2;
                    }
                }
                else
                {
                    return 1;
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
        
        #region "INFORMATION REGISTER"

        // Последовательность сериализации системных свойств регистра в формат 1С JDTO
        // 1. "Регистратор" = Recorder   - uuid
        // 2. "Период"      = Period     - DateTime
        // 3. "ВидДвижения" = RecordType - string { "Receipt", "Expense" }
        // 4. "Активность"  = Active     - bool
        private static void ConfigureInformationRegister(in InformationRegister register, DatabaseProvider provider)
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
        private static void ConfigurePropertyПериод(in ApplicationObject register, DatabaseProvider provider)
        {
            MetadataProperty property = new()
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
        private static void ConfigurePropertyНомерЗаписи(in ApplicationObject register, DatabaseProvider provider)
        {
            MetadataProperty property = new()
            {
                Name = "НомерСтроки",
                Uuid = Guid.Empty,
                Purpose = PropertyPurpose.System,
                DbName = (provider == DatabaseProvider.SQLServer ? "_LineNo" : "_lineno")
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
        private static void ConfigurePropertyАктивность(in ApplicationObject register, DatabaseProvider provider)
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
        private static void ConfigurePropertyРегистратор(in ApplicationObject register, DatabaseProvider provider)
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
    }
}