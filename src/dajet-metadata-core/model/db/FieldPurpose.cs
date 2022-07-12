namespace DaJet.Metadata.Model
{
    public enum FieldPurpose
    {
        /// <summary>Single type value of the property (default). _Fld</summary>
        Value,
        ///<summary>Указатель на тип составного типа данных _Fld + _TYPE
        ///<br><b>0x01</b> - Неопределено = null</br>
        ///<br><b>0x02</b> - Булево = boolean</br>
        ///<br><b>0x03</b> - Число = decimal</br>
        ///<br><b>0x04</b> - Дата = DateTime</br>
        ///<br><b>0x05</b> - Строка = string</br>
        ///<br><b>0x08</b> - Ссылка = EntityRef</br>
        ///</summary>
        Pointer,
        /// <summary>Boolean value. _Fld + _L</summary>
        Boolean,
        /// <summary>String value. _Fld + _S</summary>
        String,
        /// <summary>Numeric value. _Fld + _N</summary>
        Numeric,
        /// <summary>Date and time value. _Fld + _T</summary>
        DateTime,
        /// <summary>Type code of the reference type (class discriminator). _Fld + _RTRef</summary>
        TypeCode,
        /// <summary>Reference type primary key value. _Fld + _RRRef</summary>
        Object,
        /// <summary>Record's version (timestamp | rowversion).</summary>
        Version,
        /// <summary>Binary value (bytes array).</summary>
        Binary
    }
}