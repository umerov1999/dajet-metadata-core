using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Model
{
    [Flags] internal enum SimpleType : byte
    {
        None = 0x00,
        Binary = 0x01,
        String = 0x02,
        Numeric = 0x04,
        Boolean = 0x08,
        DateTime = 0x10,
        Reference = 0x20,
        ValueStorage = 0x40,
        UniqueIdentifier = 0x80
    }
    [Flags] internal enum ReferenceType : uint
    {
        Any = 0xFFFFFFFFu,
        None = 0x00000000u,
        Account = 0x00000001u,
        Catalog = 0x00000002u,
        Document = 0x00000004u,
        Publication = 0x00000008u,
        Enumeration = 0x00000010u,
        Characteristic = 0x00000020u,
        Сalculation = 0x00000040u,
        BusinessTask = 0x00000080u,
        BusinessProcess = 0x00000100u,
        BusinessRoutePoint = 0x00000200u,
        ExternalDataSource = 0x00000400u
    }
    public sealed class DataTypeSet
    {
        private Guid _uuid = Guid.Empty;
        private SimpleType _simple = SimpleType.None;
        private ReferenceType _reference = ReferenceType.None;

        #region "Simple types"

        ///<summary>Тип значения свойства "УникальныйИдентификатор", binary(16). Не поддерживает составной тип данных.</summary>
        public bool IsUuid
        {
            get { return (_simple & SimpleType.UniqueIdentifier) == SimpleType.UniqueIdentifier; }
            set
            {
                if (value)
                {
                    _simple = SimpleType.UniqueIdentifier;
                    ReferenceTypeUuid = Guid.Empty;
                    _reference = ReferenceType.None;
                }
                else if (IsUuid)
                {
                    _simple = SimpleType.None;
                }
            }
        }
        ///<summary>Типом значения свойства является byte[8] - версия данных, timestamp, rowversion. Не поддерживает составной тип данных.</summary>
        public bool IsBinary
        {
            get { return (_simple & SimpleType.Binary) == SimpleType.Binary; }
            set
            {
                if (value)
                {
                    _simple = SimpleType.Binary;
                    ReferenceTypeUuid = Guid.Empty;
                    _reference = ReferenceType.None;
                }
                else if (IsBinary)
                {
                    _simple = SimpleType.None;
                }
            }
        }
        ///<summary>Тип значения свойства "ХранилищеЗначения", varbinary(max). Не поддерживает составной тип данных.</summary>
        public bool IsValueStorage
        {
            get { return (_simple & SimpleType.ValueStorage) == SimpleType.ValueStorage; }
            set
            {
                if (value)
                {
                    _simple = SimpleType.ValueStorage;
                    ReferenceTypeUuid = Guid.Empty;
                    _reference = ReferenceType.None;
                }
                else if (IsValueStorage)
                {
                    _simple = SimpleType.None;
                }
            }
        }

        ///<summary>Типом значения свойства может быть "Булево" (поддерживает составной тип данных)</summary>
        public bool CanBeBoolean
        {
            get { return (_simple & SimpleType.Boolean) == SimpleType.Boolean; }
            set
            {
                if (IsUuid || IsBinary || IsValueStorage)
                {
                    if (value) { _simple = SimpleType.Boolean; } // false is ignored
                }
                else if (value)
                {
                    _simple |= SimpleType.Boolean;
                }
                else if (CanBeBoolean)
                {
                    _simple ^= SimpleType.Boolean;
                }
            }
        }

        ///<summary>Типом значения свойства может быть "Строка" (поддерживает составной тип данных)</summary>
        public bool CanBeString
        {
            get { return (_simple & SimpleType.String) == SimpleType.String; }
            set
            {
                if (IsUuid || IsBinary || IsValueStorage)
                {
                    if (value) { _simple = SimpleType.String; } // false is ignored
                }
                else if (value)
                {
                    _simple |= SimpleType.String;
                }
                else if (CanBeString)
                {
                    _simple ^= SimpleType.String;
                }
            }
        }
        public int StringLength { get; set; } = 10;
        public StringKind StringKind { get; set; } = StringKind.Unlimited;

        ///<summary>Типом значения свойства может быть "Число" (поддерживает составной тип данных)</summary>
        public bool CanBeNumeric
        {
            get { return (_simple & SimpleType.Numeric) == SimpleType.Numeric; }
            set
            {
                if (IsUuid || IsBinary || IsValueStorage)
                {
                    if (value) { _simple = SimpleType.Numeric; } // false is ignored
                }
                else if (value)
                {
                    _simple |= SimpleType.Numeric;
                }
                else if (CanBeNumeric)
                {
                    _simple ^= SimpleType.Numeric;
                }
            }
        }
        public int NumericScale { get; set; } = 0;
        public int NumericPrecision { get; set; } = 10;
        public NumericKind NumericKind { get; set; } = NumericKind.Unsigned;

        ///<summary>Типом значения свойства может быть "Дата" (поддерживает составной тип данных)</summary>
        public bool CanBeDateTime
        {
            get { return (_simple & SimpleType.DateTime) == SimpleType.DateTime; }
            set
            {
                if (IsUuid || IsBinary || IsValueStorage)
                {
                    if (value) { _simple = SimpleType.DateTime; } // false is ignored
                }
                else if (value)
                {
                    _simple |= SimpleType.DateTime;
                }
                else if (CanBeDateTime)
                {
                    _simple ^= SimpleType.DateTime;
                }
            }
        }
        public DateTimePart DateTimePart { get; set; } = DateTimePart.Date;

        #endregion

        #region "Reference types"

        public bool IsAnyReference
        {
            get { return _reference == ReferenceType.Any; }
            set
            {
                if (value)
                {
                    CanBeReference = true;
                    ReferenceTypeUuid = Guid.Empty;
                }
                _reference = value ? ReferenceType.Any : ReferenceType.None;
            }
        }
        public bool IsAnyAccount
        {
            get { return (_reference & ReferenceType.Account) == ReferenceType.Account; }
            set
            {
                if (value)
                {
                    CanBeReference = true;
                    ReferenceTypeUuid = Guid.Empty;
                    _reference |= ReferenceType.Account;
                }
                else if (IsAnyAccount)
                {
                    _reference ^= ReferenceType.Account;
                }
            }
        }
        public bool IsAnyCatalog
        {
            get { return (_reference & ReferenceType.Catalog) == ReferenceType.Catalog; }
            set
            {
                if (value)
                {
                    CanBeReference = true;
                    ReferenceTypeUuid = Guid.Empty;
                    _reference |= ReferenceType.Catalog;
                }
                else if (IsAnyCatalog)
                {
                    _reference ^= ReferenceType.Catalog;
                }
            }
        }
        public bool IsAnyDocument
        {
            get { return (_reference & ReferenceType.Document) == ReferenceType.Document; }
            set
            {
                if (value)
                {
                    CanBeReference = true;
                    ReferenceTypeUuid = Guid.Empty;
                    _reference |= ReferenceType.Document;
                }
                else if (IsAnyDocument)
                {
                    _reference ^= ReferenceType.Document;
                }
            }
        }
        public bool IsAnyPublication
        {
            get { return (_reference & ReferenceType.Publication) == ReferenceType.Publication; }
            set
            {
                if (value)
                {
                    CanBeReference = true;
                    ReferenceTypeUuid = Guid.Empty;
                    _reference |= ReferenceType.Publication;
                }
                else if (IsAnyPublication)
                {
                    _reference ^= ReferenceType.Publication;
                }
            }
        }
        public bool IsAnyEnumeration
        {
            get { return (_reference & ReferenceType.Enumeration) == ReferenceType.Enumeration; }
            set
            {
                if (value)
                {
                    CanBeReference = true;
                    ReferenceTypeUuid = Guid.Empty;
                    _reference |= ReferenceType.Enumeration;
                }
                else if (IsAnyEnumeration)
                {
                    _reference ^= ReferenceType.Enumeration;
                }
            }
        }
        public bool IsAnyCharacteristic
        {
            get { return (_reference & ReferenceType.Characteristic) == ReferenceType.Characteristic; }
            set
            {
                if (value)
                {
                    CanBeReference = true;
                    ReferenceTypeUuid = Guid.Empty;
                    _reference |= ReferenceType.Characteristic;
                }
                else if (IsAnyCharacteristic)
                {
                    _reference ^= ReferenceType.Characteristic;
                }
            }
        }
        public bool IsAnyСalculation
        {
            get { return (_reference & ReferenceType.Сalculation) == ReferenceType.Сalculation; }
            set
            {
                if (value)
                {
                    CanBeReference = true;
                    ReferenceTypeUuid = Guid.Empty;
                    _reference |= ReferenceType.Сalculation;
                }
                else if (IsAnyСalculation)
                {
                    _reference ^= ReferenceType.Сalculation;
                }
            }
        }
        public bool IsAnyBusinessTask
        {
            get { return (_reference & ReferenceType.BusinessTask) == ReferenceType.BusinessTask; }
            set
            {
                if (value)
                {
                    CanBeReference = true;
                    ReferenceTypeUuid = Guid.Empty;
                    _reference |= ReferenceType.BusinessTask;
                }
                else if (IsAnyBusinessTask)
                {
                    _reference ^= ReferenceType.BusinessTask;
                }
            }
        }
        public bool IsAnyBusinessProcess
        {
            get { return (_reference & ReferenceType.BusinessProcess) == ReferenceType.BusinessProcess; }
            set
            {
                if (value)
                {
                    CanBeReference = true;
                    ReferenceTypeUuid = Guid.Empty;
                    _reference |= ReferenceType.BusinessProcess;
                }
                else if (IsAnyBusinessProcess)
                {
                    _reference ^= ReferenceType.BusinessProcess;
                }
            }
        }
        public bool IsAnyBusinessRoutePoint
        {
            get { return (_reference & ReferenceType.BusinessRoutePoint) == ReferenceType.BusinessRoutePoint; }
            set
            {
                if (value)
                {
                    CanBeReference = true;
                    ReferenceTypeUuid = Guid.Empty;
                    _reference |= ReferenceType.BusinessRoutePoint;
                }
                else if (IsAnyBusinessRoutePoint)
                {
                    _reference ^= ReferenceType.BusinessRoutePoint;
                }
            }
        }
        public bool IsAnyExternalDataSource
        {
            get { return (_reference & ReferenceType.ExternalDataSource) == ReferenceType.ExternalDataSource; }
            set
            {
                if (value)
                {
                    CanBeReference = true;
                    ReferenceTypeUuid = Guid.Empty;
                    _reference |= ReferenceType.ExternalDataSource;
                }
                else if (IsAnyExternalDataSource)
                {
                    _reference ^= ReferenceType.ExternalDataSource;
                }
            }
        }

        #endregion

        ///<summary>Типом значения свойства может быть "Ссылка" (поддерживает составной тип данных)</summary>
        public bool CanBeReference
        {
            get { return (_simple & SimpleType.Reference) == SimpleType.Reference; }
            set
            {
                if (IsUuid || IsBinary || IsValueStorage)
                {
                    if (value) { _simple = SimpleType.Reference; } // false is ignored
                }
                else if (value)
                {
                    _simple |= SimpleType.Reference;
                }
                else if (CanBeReference)
                {
                    _simple ^= SimpleType.Reference;
                }
            }
        }
        public Guid ReferenceTypeUuid
        {
            get { return _uuid; }
            set
            {
                _uuid = value;

                if (_uuid != Guid.Empty)
                {
                    _reference = ReferenceType.None;
                }
            }
        }
        public List<Guid> References { get; set; } // TODO: ???

        ///<summary>Проверяет является ли свойство составным типом данных</summary>
        public bool IsMultipleType
        {
            get
            {
                if (IsUuid || IsValueStorage || IsBinary) return false;

                int count = 0;
                if (CanBeString) count++;
                if (CanBeBoolean) count++;
                if (CanBeNumeric) count++;
                if (CanBeDateTime) count++;
                if (CanBeReference) count++;
                if (count > 1) return true;

                if (CanBeReference && ReferenceTypeUuid == Guid.Empty) return true;

                return false;
            }
        }
        public override string ToString()
        {
            if (IsMultipleType) return "Multiple";
            else if (IsUuid) return "Uuid";
            else if (IsBinary) return "Binary";
            else if (IsValueStorage) return "ValueStorage";
            else if (CanBeString) return "String";
            else if (CanBeBoolean) return "Boolean";
            else if (CanBeNumeric) return "Numeric";
            else if (CanBeDateTime) return "DateTime";
            else if (CanBeReference) return "Reference";
            else return "Undefined";
        }
    }
}