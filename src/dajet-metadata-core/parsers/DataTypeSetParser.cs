using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Parsers
{
    public static class DataTypeSetParser
    {
        private static readonly Guid VALUE_STORAGE = new Guid("e199ca70-93cf-46ce-a54b-6edc88c3a296"); // ХранилищеЗначения - varbinary(max)
        private static readonly Guid UNIQUEIDENTIFIER = new Guid("fc01b5df-97fe-449b-83d4-218a090e681e"); // УникальныйИдентификатор - binary(16)

        private static readonly Guid ANY_REFERENCE = new Guid("280f5f0e-9c8a-49cc-bf6d-4d296cc17a63"); // ЛюбаяСсылка
        private static readonly Guid ACCOUNT_REFERENCE = new Guid("ac606d60-0209-4159-8e4c-794bc091ce38"); // ПланСчетовСсылка
        private static readonly Guid CATALOG_REFERENCE = new Guid("e61ef7b8-f3e1-4f4b-8ac7-676e90524997"); // СправочникСсылка
        private static readonly Guid DOCUMENT_REFERENCE = new Guid("38bfd075-3e63-4aaa-a93e-94521380d579"); // ДокументСсылка
        private static readonly Guid ENUMERATION_REFERENCE = new Guid("474c3bf6-08b5-4ddc-a2ad-989cedf11583"); // ПеречислениеСсылка
        private static readonly Guid PUBLICATION_REFERENCE = new Guid("0a52f9de-73ea-4507-81e8-66217bead73a"); // ПланОбменаСсылка
        private static readonly Guid CHARACTERISTIC_REFERENCE = new Guid("99892482-ed55-4fb5-a7f7-20888820a758"); // ПланВидовХарактеристикСсылка

        private static readonly HashSet<Guid> ReferenceBaseTypes = new HashSet<Guid>()
        {
            ANY_REFERENCE,
            ACCOUNT_REFERENCE,
            CATALOG_REFERENCE,
            DOCUMENT_REFERENCE,
            ENUMERATION_REFERENCE,
            PUBLICATION_REFERENCE,
            CHARACTERISTIC_REFERENCE
        };
        private static Dictionary<Guid, ApplicationObject> GetCollection(in InfoBase context, in Guid typeUuid)
        {
            if (typeUuid == ANY_REFERENCE) return null;
            //else if (typeUuid == ACCOUNT_REFERENCE) return context.Accounts;
            //else if (typeUuid == CATALOG_REFERENCE) return context.Catalogs;
            //else if (typeUuid == DOCUMENT_REFERENCE) return context.Documents;
            //else if (typeUuid == ENUMERATION_REFERENCE) return context.Enumerations;
            //else if (typeUuid == PUBLICATION_REFERENCE) return context.Publications;
            //else if (typeUuid == CHARACTERISTIC_REFERENCE) return context.Characteristics;
            return null;
        }

        public static void Parse(in ConfigFileReader reader, out DataTypeSet target)
        {
            target = new DataTypeSet();
            List<Guid> references = new List<Guid>();

            int level = reader.Level; // root level of the data type set description object

            _ = reader.Read(); // 0 index
            if (reader.Value != "Pattern")
            {
                return; // это не объект описания типов
            }
            
            while (reader.Read())
            {
                if (reader.Token == TokenType.StartObject)
                {
                    // read the next data type description
                    _pointer = -1;
                    _qualifiers[0] = null;
                    _qualifiers[1] = null;
                    _qualifiers[2] = null;
                }
                else if (reader.Token == TokenType.Value || reader.Token == TokenType.String)
                {
                    if (reader.Path[reader.Level] == 0) // 0 - discriminator
                    {
                        if (reader.Value == MetadataTokens.B) // {"B"}
                        {
                            ReadBoolean(in target);
                        }
                        else if (reader.Value == MetadataTokens.D) // {"D"} | {"D","D"} | {"D","T"}
                        {
                            ReadDateTime(in reader, in target);
                        }
                        else if (reader.Value == MetadataTokens.S) // {"S"} | {"S",10,0} | {"S",10,1}
                        {
                            ReadString(in reader, in target);
                        }
                        else if (reader.Value == MetadataTokens.N) // {"N",10,2,0} | {"N",10,2,1}
                        {
                            ReadNumeric(in reader, in target);
                        }
                        else if (reader.Value == MetadataTokens.R) // {"#",70497451-981e-43b8-af46-fae8d65d16f2}
                        {
                            ReadReference(in reader, in target, in references);
                        }
                    }
                }
                else if (reader.Token == TokenType.EndObject)
                {
                    if (level == reader.Level)
                    {
                        break;
                    }
                }
            }

            if (references.Count > 0)
            {
                target.References = references;
            }

            if (references.Count == 1) // single reference type value
            {
                target.ReferenceTypeUuid = references[0];
            }
            else
            {
                // TODO: flush references to sqlite database !?
            }
        }

        private static int _pointer;
        private static string[] _qualifiers = new string[3];
        private static void ReadQualifiers(in ConfigFileReader reader)
        {
            while (reader.Read())
            {
                if (reader.Token == TokenType.EndObject)
                {
                    break;
                }
                else if (reader.Token == TokenType.Value || reader.Token == TokenType.String)
                {
                    if (reader.Value == null) { continue; }

                    _pointer++;
                    _qualifiers[_pointer] = reader.Value;
                }
            }
        }
        private static void ReadBoolean(in DataTypeSet target)
        {
            target.CanBeBoolean = true;
        }
        private static void ReadDateTime(in ConfigFileReader reader, in DataTypeSet target)
        {
            target.CanBeDateTime = true;

            ReadQualifiers(in reader);

            if (_pointer == -1)
            {
                target.DateTimePart = DateTimePart.DateTime;
            }
            else if (_pointer == 0 && _qualifiers[_pointer] == MetadataTokens.D)
            {
                target.DateTimePart = DateTimePart.Date;
            }
            else
            {
                target.DateTimePart = DateTimePart.Time;
            }
        }
        private static void ReadString(in ConfigFileReader reader, in DataTypeSet target)
        {
            target.CanBeString = true;

            ReadQualifiers(in reader);

            if (_pointer == -1)
            {
                target.StringLength = -1;
                target.StringKind = StringKind.Unlimited;
            }
            else if (_pointer == 1)
            {
                target.StringLength = int.Parse(_qualifiers[0]);
                target.StringKind = (StringKind)int.Parse(_qualifiers[1]);
            }
        }
        private static void ReadNumeric(in ConfigFileReader reader, in DataTypeSet target)
        {
            target.CanBeNumeric = true;

            ReadQualifiers(in reader);

            if (_pointer == 2)
            {
                target.NumericPrecision = int.Parse(_qualifiers[0]);
                target.NumericScale = int.Parse(_qualifiers[1]);
                target.NumericKind = (NumericKind)int.Parse(_qualifiers[2]);
            }
        }
        private static void ReadReference(in ConfigFileReader reader, in DataTypeSet target, in List<Guid> references)
        {
            ReadQualifiers(in reader);

            if (_pointer != 0) { return; }

            Guid typeUuid = new Guid(_qualifiers[_pointer]);

            if (typeUuid == VALUE_STORAGE) // ХранилищеЗначения - varbinary(max)
            {
                target.IsValueStorage = true; // Не может быть составным типом !
                return;
            }
            
            if (typeUuid == UNIQUEIDENTIFIER) // УникальныйИдентификатор - binary(16)
            {
                target.IsUuid = true; // Не может быть составным типом !
                return;
            }

            target.CanBeReference = true;

            if (ReferenceBaseTypes.Contains(typeUuid))
            {
                ApplyReferenceType(in target, in references, typeUuid);
            }
            //else if (context.CompoundTypes.TryGetValue(typeUuid, out NamedDataTypeSet compound))
            //{
            //    // since 8.3.3
            //    ApplyNamedDataTypeSet(in compound, in target, in references);
            //}
            //else if (context.CharacteristicTypes.TryGetValue(typeUuid, out Characteristic characteristic))
            //{
            //    ApplyCharacteristic(in characteristic, in target, in references);
            //}
            else
            {
                references.Add(typeUuid);
            }
        }
        private static void ApplyReferenceType(in DataTypeSet target, in List<Guid> references, Guid typeUuid)
        {
            if (typeUuid == ANY_REFERENCE)
            {
                target.IsAnyReference = true;
                return;
            }

            Dictionary<Guid, ApplicationObject> collection = null;

            if (typeUuid == ACCOUNT_REFERENCE)
            {
                target.IsAnyAccount = true;
                //collection = context.Accounts;
            }
            else if (typeUuid == CATALOG_REFERENCE)
            {
                target.IsAnyCatalog = true;
                //collection = context.Catalogs;
            }
            else if (typeUuid == DOCUMENT_REFERENCE)
            {
                target.IsAnyDocument = true;
                //collection = context.Documents;
            }
            else if (typeUuid == ENUMERATION_REFERENCE)
            {
                target.IsAnyEnumeration = true;
                //collection = context.Enumerations;
            }
            else if (typeUuid == PUBLICATION_REFERENCE)
            {
                target.IsAnyPublication = true;
                //collection = context.Publications;
            }
            else if (typeUuid == CHARACTERISTIC_REFERENCE)
            {
                target.IsAnyCharacteristic = true;
                //collection = context.Characteristics;
            }

            if (collection == null || collection.Count == 0)
            {
                return;
            }

            if (collection.Count > 1)
            {
                references.Add(Guid.Empty); // Множественный ссылочный тип данных
                return;
            }

            foreach (var item in collection) // collection.Count == 1
            {
                if (item.Value.Uuid != Guid.Empty)
                {
                    references.Add(item.Value.Uuid); // Единственный объект метаданных в коллекции
                }
                else
                {
                    // TODO: register DataTypeSet for delayed resolvation !?
                    // context.RegisterUnloadedType ...
                }
            }
        }
        private static void ApplyCharacteristic(in Characteristic source, in DataTypeSet target, in List<Guid> references)
        {
            // TODO: use internal flags fields of the DataTypeSet class to perform bitwise operations

            if (!target.CanBeString && source.TypeInfo.CanBeString) target.CanBeString = true;
            if (!target.CanBeBoolean && source.TypeInfo.CanBeBoolean) target.CanBeBoolean = true;
            if (!target.CanBeNumeric && source.TypeInfo.CanBeNumeric) target.CanBeNumeric = true;
            if (!target.CanBeDateTime && source.TypeInfo.CanBeDateTime) target.CanBeDateTime = true;
            if (!target.CanBeReference && source.TypeInfo.CanBeReference) target.CanBeReference = true;
            if (!target.IsUuid && source.TypeInfo.IsUuid) target.IsUuid = true;
            if (!target.IsBinary && source.TypeInfo.IsBinary) target.IsBinary = true;
            if (!target.IsValueStorage && source.TypeInfo.IsValueStorage) target.IsValueStorage = true;

            if (source.TypeInfo.CanBeReference)
            {
                references.Add(source.TypeInfo.ReferenceTypeUuid);
            }
        }
        private static void ApplyNamedDataTypeSet(in NamedDataTypeSet source, in DataTypeSet target, in List<Guid> references)
        {
            // TODO: add internal flags field to the DataTypeInfo class so as to use bitwise operations

            if (!target.CanBeString && source.TypeInfo.CanBeString) target.CanBeString = true;
            if (!target.CanBeBoolean && source.TypeInfo.CanBeBoolean) target.CanBeBoolean = true;
            if (!target.CanBeNumeric && source.TypeInfo.CanBeNumeric) target.CanBeNumeric = true;
            if (!target.CanBeDateTime && source.TypeInfo.CanBeDateTime) target.CanBeDateTime = true;
            if (!target.CanBeReference && source.TypeInfo.CanBeReference) target.CanBeReference = true;
            if (!target.IsUuid && source.TypeInfo.IsUuid) target.IsUuid = true;
            if (!target.IsBinary && source.TypeInfo.IsBinary) target.IsBinary = true;
            if (!target.IsValueStorage && source.TypeInfo.IsValueStorage) target.IsValueStorage = true;

            if (source.TypeInfo.CanBeReference)
            {
                references.Add(source.TypeInfo.ReferenceTypeUuid);
            }
        }
    }
}