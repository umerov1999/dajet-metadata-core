using System;

namespace DaJet.Metadata.Core
{
    public static class SystemUuid
    {
        ///<summary>Идентификатор коллекции реквизитов табличной части</summary>
        public static Guid TablePart_Properties = new Guid("888744e1-b616-11d4-9436-004095e12fc7");

        ///<summary>Идентификатор коллекции реквизитов справочника</summary>
        public static Guid Catalog_Properties = new Guid("cf4abea7-37b2-11d4-940f-008048da11f9");
        ///<summary>Идентификатор коллекции табличных частей справочника</summary>
        public static Guid Catalog_TableParts = new Guid("932159f9-95b2-4e76-a8dd-8849fe5c5ded");

        ///<summary>Идентификатор коллекции реквизитов плана видов характеристик</summary>
        public static Guid Characteristic_Properties = new Guid("31182525-9346-4595-81f8-6f91a72ebe06");
        ///<summary>Идентификатор коллекции табличных частей плана видов характеристик</summary>
        public static Guid Characteristic_TableParts = new Guid("54e36536-7863-42fd-bea3-c5edd3122fdc");

        ///<summary>Идентификатор коллекции ресурсов регистра сведений</summary>
        public static Guid InformationRegister_Measure = new Guid("13134202-f60b-11d5-a3c7-0050bae0a776");
        ///<summary>Идентификатор коллекции реквизитов регистра сведений</summary>
        public static Guid InformationRegister_Property = new Guid("a2207540-1400-11d6-a3c7-0050bae0a776");
        ///<summary>Идентификатор коллекции измерений регистра сведений</summary>
        public static Guid InformationRegister_Dimension = new Guid("13134203-f60b-11d5-a3c7-0050bae0a776");
    }
}