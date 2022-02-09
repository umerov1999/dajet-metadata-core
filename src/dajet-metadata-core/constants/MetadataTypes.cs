using System;

namespace DaJet.Metadata.Core
{
    public static class MetadataTypes
    {
        public static Guid InfoBase = Guid.Empty; // Корень конфигурации информационной базы 1С
        public static Guid Subsystem = new Guid("37f2fa9a-b276-11d4-9435-004095e12fc7"); // Подсистемы
        public static Guid SharedProperty = new Guid("15794563-ccec-41f6-a83c-ec5f7b9a5bc1"); // Общие реквизиты
        public static Guid NamedDataTypeSet = new Guid("c045099e-13b9-4fb6-9d50-fca00202971e"); // Определяемые типы
        public static Guid Catalog = new Guid("cf4abea6-37b2-11d4-940f-008048da11f9"); // Справочники
        public static Guid Constant = new Guid("0195e80c-b157-11d4-9435-004095e12fc7"); // Константы
        public static Guid Document = new Guid("061d872a-5787-460e-95ac-ed74ea3a3e84"); // Документы
        public static Guid Enumeration = new Guid("f6a80749-5ad7-400b-8519-39dc5dff2542"); // Перечисления
        public static Guid Publication = new Guid("857c4a91-e5f4-4fac-86ec-787626f1c108"); // Планы обмена
        public static Guid Characteristic = new Guid("82a1b659-b220-4d94-a9bd-14d757b95a48"); // Планы видов характеристик
        public static Guid InformationRegister = new Guid("13134201-f60b-11d5-a3c7-0050bae0a776"); // Регистры сведений
        public static Guid AccumulationRegister = new Guid("b64d9a40-1642-11d6-a3c7-0050bae0a776"); // Регистры накопления
    }
}