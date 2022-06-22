﻿using System;

namespace DaJet.Metadata.Model
{
    ///<summary>Смотри также <see cref="DataTypes"/></summary>
    [Flags] internal enum DataTypeFlags : byte
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

    ///<summary>
    ///Описание типов: определяет допустимые типы данных для значений свойств прикладных объектов метаданных.
    ///<br>
    ///Составной тип данных может допускать использование нескольких типов данных для одного и того же свойства.
    ///</br>
    ///<br>
    ///Внимание! Следующие типы данных не допускают использование составного типа данных:
    ///</br>
    ///<br>
    ///"УникальныйИдентификатор", "ХранилищеЗначения", "ОпределяемыйТип", "Характеристика" и строка неограниченной длины.
    ///</br>
    ///</summary>
    public sealed class DataTypeSet
    {
        private DataTypeFlags _flags = DataTypeFlags.None;

        #region "SIMPLE DATA TYPES"

        ///<summary>Тип значения свойства "УникальныйИдентификатор", binary(16). Не поддерживает составной тип данных.</summary>
        public bool IsUuid
        {
            get { return (_flags & DataTypeFlags.UniqueIdentifier) == DataTypeFlags.UniqueIdentifier; }
            set
            {
                if (value)
                {
                    _flags = DataTypeFlags.UniqueIdentifier;
                    Reference = Guid.Empty;
                }
                else if (IsUuid)
                {
                    _flags = DataTypeFlags.None;
                }
            }
        }
        ///<summary>Типом значения свойства является byte[8] - версия данных, timestamp, rowversion. Не поддерживает составной тип данных.</summary>
        public bool IsBinary
        {
            get { return (_flags & DataTypeFlags.Binary) == DataTypeFlags.Binary; }
            set
            {
                if (value)
                {
                    _flags = DataTypeFlags.Binary;
                    Reference = Guid.Empty;
                }
                else if (IsBinary)
                {
                    _flags = DataTypeFlags.None;
                }
            }
        }
        ///<summary>Тип значения свойства "ХранилищеЗначения", varbinary(max). Не поддерживает составной тип данных.</summary>
        public bool IsValueStorage
        {
            get { return (_flags & DataTypeFlags.ValueStorage) == DataTypeFlags.ValueStorage; }
            set
            {
                if (value)
                {
                    _flags = DataTypeFlags.ValueStorage;
                    Reference = Guid.Empty;
                }
                else if (IsValueStorage)
                {
                    _flags = DataTypeFlags.None;
                }
            }
        }

        ///<summary>Типом значения свойства может быть "Булево" (поддерживает составной тип данных)</summary>
        public bool CanBeBoolean
        {
            get { return (_flags & DataTypeFlags.Boolean) == DataTypeFlags.Boolean; }
            set
            {
                if (IsUuid || IsValueStorage || IsBinary)
                {
                    if (value) { _flags = DataTypeFlags.Boolean; } // false is ignored
                }
                else if (value)
                {
                    _flags |= DataTypeFlags.Boolean;
                }
                else if (CanBeBoolean)
                {
                    _flags ^= DataTypeFlags.Boolean;
                }
            }
        }

        ///<summary>Типом значения свойства может быть "Строка" (поддерживает составной тип данных)</summary>
        public bool CanBeString
        {
            get { return (_flags & DataTypeFlags.String) == DataTypeFlags.String; }
            set
            {
                if (IsUuid || IsValueStorage || IsBinary)
                {
                    if (value) { _flags = DataTypeFlags.String; } // false is ignored
                }
                else if (value)
                {
                    _flags |= DataTypeFlags.String;
                }
                else if (CanBeString)
                {
                    _flags ^= DataTypeFlags.String;
                }
            }
        }
        ///<summary>Квалификатор: длина строки в символах. Неограниченная длина равна 0.</summary>
        public int StringLength { get; set; } = 10; // TODO: Строка неограниченной длины не поддерживает составной тип данных!
        ///<summary>
        ///Квалификатор: фиксированная (дополняется пробелами) или переменная длина строки.
        ///<br>
        ///Строка неограниченной длины (длина равна 0) всегда является переменной строкой.
        ///</br>
        ///</summary>
        public StringKind StringKind { get; set; } = StringKind.Variable;

        ///<summary>Типом значения свойства может быть "Число" (поддерживает составной тип данных)</summary>
        public bool CanBeNumeric
        {
            get { return (_flags & DataTypeFlags.Numeric) == DataTypeFlags.Numeric; }
            set
            {
                if (IsUuid || IsValueStorage || IsBinary)
                {
                    if (value) { _flags = DataTypeFlags.Numeric; } // false is ignored
                }
                else if (value)
                {
                    _flags |= DataTypeFlags.Numeric;
                }
                else if (CanBeNumeric)
                {
                    _flags ^= DataTypeFlags.Numeric;
                }
            }
        }
        ///<summary>Квалификатор: определяет допустимое количество знаков после запятой.</summary>
        public int NumericScale { get; set; } = 0;
        ///<summary>Квалификатор: определяет разрядность числа (сумма знаков до и после запятой).</summary>
        public int NumericPrecision { get; set; } = 10;
        ///<summary>Квалификатор: определяет возможность использования отрицательных значений.</summary>
        public NumericKind NumericKind { get; set; } = NumericKind.CanBeNegative;

        ///<summary>Типом значения свойства может быть "Дата" (поддерживает составной тип данных)</summary>
        public bool CanBeDateTime
        {
            get { return (_flags & DataTypeFlags.DateTime) == DataTypeFlags.DateTime; }
            set
            {
                if (IsUuid || IsValueStorage || IsBinary)
                {
                    if (value) { _flags = DataTypeFlags.DateTime; } // false is ignored
                }
                else if (value)
                {
                    _flags |= DataTypeFlags.DateTime;
                }
                else if (CanBeDateTime)
                {
                    _flags ^= DataTypeFlags.DateTime;
                }
            }
        }
        ///<summary>Квалификатор: определяет используемые части даты.</summary>
        public DateTimePart DateTimePart { get; set; } = DateTimePart.Date;

        #endregion

        #region "REFERENCE DATA TYPE"

        ///<summary>Типом значения свойства может быть "Ссылка" (поддерживает составной тип данных)</summary>
        public bool CanBeReference
        {
            get { return (_flags & DataTypeFlags.Reference) == DataTypeFlags.Reference; }
            set
            {
                if (IsUuid || IsValueStorage || IsBinary)
                {
                    if (value) { _flags = DataTypeFlags.Reference; } // false is ignored
                }
                else if (value)
                {
                    _flags |= DataTypeFlags.Reference;
                }
                else if (CanBeReference)
                {
                    _flags ^= DataTypeFlags.Reference;
                }
            }
        }
        ///<summary>
        ///Значение (по умолчанию) <see cref="Guid.Empty"/> допускает множественный ссылочный тип данных (TRef + RRef).
        ///<br>
        ///Конкретное значение <see cref="Guid"/> допускает использование единственного ссылочного типа данных (RRef).
        ///</br>
        ///<br>
        ///Выполняет роль квалификатора ссылочного типа данных.
        ///</br>
        ///</summary>
        public Guid Reference { get; set; } = Guid.Empty;

        #endregion

        internal void Apply(in DataTypeSet source)
        {
            _flags = source._flags;

            StringKind = source.StringKind;
            StringLength = source.StringLength;

            NumericKind = source.NumericKind;
            NumericScale = source.NumericScale;
            NumericPrecision = source.NumericPrecision;

            DateTimePart = source.DateTimePart;
            
            Reference = source.Reference;
        }

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

                if (CanBeReference && Reference == Guid.Empty) return true;

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