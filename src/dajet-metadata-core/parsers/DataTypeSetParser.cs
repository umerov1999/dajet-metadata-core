using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Parsers
{
    public sealed class DataTypeSetParser
    {
        private readonly HashSet<Guid> ReferenceBaseTypes = new HashSet<Guid>()
        {
            MetadataRegistry.ANY_REFERENCE,
            MetadataRegistry.ACCOUNT_REFERENCE,
            MetadataRegistry.CATALOG_REFERENCE,
            MetadataRegistry.DOCUMENT_REFERENCE,
            MetadataRegistry.ENUMERATION_REFERENCE,
            MetadataRegistry.PUBLICATION_REFERENCE,
            MetadataRegistry.CHARACTERISTIC_REFERENCE
        };

        public void Parse(in ConfigFileReader source, out DataTypeSet target)
        {
            // Параметр source должен быть позиционирован в данный момент на корневом узле
            // объекта описания типа данных свойства объекта метаданных (токен = '{')
            // source.Char == '{' && source.Token == TokenType.StartObject

            target = new DataTypeSet();
            List<Guid> references = new List<Guid>();

            _ = source.Read(); // 0 index
            if (source.Value != "Pattern")
            {
                return; // это не объект описания типов
            }
            
            while (source.Read())
            {
                if (source.Token == TokenType.EndObject)
                {
                    break;
                }
                else if (source.Token == TokenType.StartObject)
                {
                    // read the next data type description
                    _pointer = -1;
                    _qualifiers[0] = null;
                    _qualifiers[1] = null;
                    _qualifiers[2] = null;
                }
                else if (source.Token == TokenType.Value || source.Token == TokenType.String)
                {
                    if (source.Path[source.Level] == 0) // 0 - discriminator
                    {
                        if (source.Value == MetadataTokens.B) // {"B"}
                        {
                            ReadBoolean(in target);
                        }
                        else if (source.Value == MetadataTokens.D) // {"D"} | {"D","D"} | {"D","T"}
                        {
                            ReadDateTime(in source, in target);
                        }
                        else if (source.Value == MetadataTokens.S) // {"S"} | {"S",10,0} | {"S",10,1}
                        {
                            ReadString(in source, in target);
                        }
                        else if (source.Value == MetadataTokens.N) // {"N",10,2,0} | {"N",10,2,1}
                        {
                            ReadNumeric(in source, in target);
                        }
                        else if (source.Value == MetadataTokens.R) // {"#",70497451-981e-43b8-af46-fae8d65d16f2}
                        {
                            ReadReference(in source, in target, in references);
                        }
                    }
                }
            }

            target.References = references;
        }

        private int _pointer;
        private string[] _qualifiers = new string[3];
        private void ReadQualifiers(in ConfigFileReader reader)
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
        private void ReadBoolean(in DataTypeSet target)
        {
            target.CanBeBoolean = true;
        }
        private void ReadDateTime(in ConfigFileReader reader, in DataTypeSet target)
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
        private void ReadString(in ConfigFileReader reader, in DataTypeSet target)
        {
            target.CanBeString = true;

            ReadQualifiers(in reader);

            if (_pointer == -1)
            {
                target.StringLength = -1; // Неограниченная длина - nvarchar(max)
                target.StringKind = StringKind.Variable;
            }
            else if (_pointer == 1)
            {
                target.StringLength = int.Parse(_qualifiers[0]);
                target.StringKind = (StringKind)int.Parse(_qualifiers[1]);
            }
        }
        private void ReadNumeric(in ConfigFileReader reader, in DataTypeSet target)
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
        private void ReadReference(in ConfigFileReader reader, in DataTypeSet target, in List<Guid> references)
        {
            ReadQualifiers(in reader);

            if (_pointer != 0) { return; }

            Guid typeUuid = new Guid(_qualifiers[_pointer]);

            if (typeUuid == MetadataRegistry.VALUE_STORAGE) // ХранилищеЗначения - varbinary(max)
            {
                target.IsValueStorage = true; // Не может быть составным типом !
                return;
            }
            
            if (typeUuid == MetadataRegistry.UNIQUEIDENTIFIER) // УникальныйИдентификатор - binary(16)
            {
                target.IsUuid = true; // Не может быть составным типом !
                return;
            }

            target.CanBeReference = true;

            if (ReferenceBaseTypes.Contains(typeUuid))
            {
                ApplyReferenceTypeQualifier(in target, typeUuid);
            }
            else
            {
                references.Add(typeUuid);
            }

            // TODO:
            //else if (context.CompoundTypes.TryGetValue(typeUuid, out NamedDataTypeSet compound))
            //{
            //    // since 8.3.3
            //    ApplyNamedDataTypeSet(in compound, in target, in references);
            //}
            //else if (context.CharacteristicTypes.TryGetValue(typeUuid, out Characteristic characteristic))
            //{
            //    ApplyCharacteristic(in characteristic, in target, in references);
            //}
        }
        private void ApplyReferenceTypeQualifier(in DataTypeSet target, Guid typeUuid)
        {
            if (typeUuid == MetadataRegistry.ANY_REFERENCE)
            {
                target.IsAnyReference = true;
            }
            else if (typeUuid == MetadataRegistry.ACCOUNT_REFERENCE)
            {
                target.IsAnyAccount = true;
            }
            else if (typeUuid == MetadataRegistry.CATALOG_REFERENCE)
            {
                target.IsAnyCatalog = true;
            }
            else if (typeUuid == MetadataRegistry.DOCUMENT_REFERENCE)
            {
                target.IsAnyDocument = true;
            }
            else if (typeUuid == MetadataRegistry.ENUMERATION_REFERENCE)
            {
                target.IsAnyEnumeration = true;
            }
            else if (typeUuid == MetadataRegistry.PUBLICATION_REFERENCE)
            {
                target.IsAnyPublication = true;
            }
            else if (typeUuid == MetadataRegistry.CHARACTERISTIC_REFERENCE)
            {
                target.IsAnyCharacteristic = true;
            }
        }
        private void ApplyCharacteristic(in Characteristic source, in DataTypeSet target, in List<Guid> references)
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
        private void ApplyNamedDataTypeSet(in NamedDataTypeSet source, in DataTypeSet target, in List<Guid> references)
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