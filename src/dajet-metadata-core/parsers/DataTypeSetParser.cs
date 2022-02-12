using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Parsers
{
    ///<summary>Парсер для чтения объекта "ОписаниеТипов".</summary>
    public sealed class DataTypeSetParser
    {
        ///<summary>
        ///Объект чтения файла метаданных <see cref="ConfigFileReader"/> перед вызовом этого метода
        ///<br>
        ///должен быть позиционирован на корневом узле объекта описания типа данных:
        ///</br>
        ///<br>
        ///<c><![CDATA[source.Char == '{' && source.Token == TokenType.StartObject]]></c>
        ///</br>
        ///</summary>
        ///<param name="source">объект чтения файла метаданных.</param>
        ///<param name="target">объект описания типа данных.</param>
        ///<param name="references">список ссылок на специальные и ссылочные типы данных для преобразования.</param>
        public void Parse(in ConfigFileReader source, out DataTypeSet target, out List<Guid> references)
        {
            target = new DataTypeSet();
            references = new List<Guid>();

            _ = source.Read(); // 0 index
            if (source.Value != "Pattern")
            {
                return; // Это не объект "ОписаниеТипов" !
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
                target.StringLength = 0; // Неограниченная длина - nvarchar(max)
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

            Guid type = new(_qualifiers[_pointer]);

            if (type == SingleTypes.ValueStorage) // ХранилищеЗначения - varbinary(max)
            {
                target.IsValueStorage = true; // Не может быть составным типом !
                return;
            }
            
            if (type == SingleTypes.Uniqueidentifier) // УникальныйИдентификатор - binary(16)
            {
                target.IsUuid = true; // Не может быть составным типом !
                return;
            }

            target.CanBeReference = true;

            references.Add(type);

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
        
        //private void ApplyCharacteristic(in Characteristic source, in DataTypeSet target, in List<Guid> references)
        //{
        //    // TODO: use internal flags fields of the DataTypeSet class to perform bitwise operations

        //    if (!target.CanBeString && source.TypeInfo.CanBeString) target.CanBeString = true;
        //    if (!target.CanBeBoolean && source.TypeInfo.CanBeBoolean) target.CanBeBoolean = true;
        //    if (!target.CanBeNumeric && source.TypeInfo.CanBeNumeric) target.CanBeNumeric = true;
        //    if (!target.CanBeDateTime && source.TypeInfo.CanBeDateTime) target.CanBeDateTime = true;
        //    if (!target.CanBeReference && source.TypeInfo.CanBeReference) target.CanBeReference = true;
        //    if (!target.IsUuid && source.TypeInfo.IsUuid) target.IsUuid = true;
        //    if (!target.IsBinary && source.TypeInfo.IsBinary) target.IsBinary = true;
        //    if (!target.IsValueStorage && source.TypeInfo.IsValueStorage) target.IsValueStorage = true;

        //    if (source.TypeInfo.CanBeReference)
        //    {
        //        references.Add(source.TypeInfo.ReferenceTypeUuid);
        //    }
        //}
        //private void ApplyNamedDataTypeSet(in NamedDataTypeSet source, in DataTypeSet target, in List<Guid> references)
        //{
        //    // TODO: add internal flags field to the DataTypeInfo class so as to use bitwise operations

        //    if (!target.CanBeString && source.TypeInfo.CanBeString) target.CanBeString = true;
        //    if (!target.CanBeBoolean && source.TypeInfo.CanBeBoolean) target.CanBeBoolean = true;
        //    if (!target.CanBeNumeric && source.TypeInfo.CanBeNumeric) target.CanBeNumeric = true;
        //    if (!target.CanBeDateTime && source.TypeInfo.CanBeDateTime) target.CanBeDateTime = true;
        //    if (!target.CanBeReference && source.TypeInfo.CanBeReference) target.CanBeReference = true;
        //    if (!target.IsUuid && source.TypeInfo.IsUuid) target.IsUuid = true;
        //    if (!target.IsBinary && source.TypeInfo.IsBinary) target.IsBinary = true;
        //    if (!target.IsValueStorage && source.TypeInfo.IsValueStorage) target.IsValueStorage = true;

        //    if (source.TypeInfo.CanBeReference)
        //    {
        //        references.Add(source.TypeInfo.ReferenceTypeUuid);
        //    }
        //}
    }
}