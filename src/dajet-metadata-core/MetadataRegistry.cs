using System;

namespace DaJet.Metadata.Core
{
    public static class MetadataRegistry
    {
        #region "ИДЕНТИФИКАТОРЫ ПОДСИСТЕМ ОБЪЕКТОВ МЕТАДАННЫХ"

        public static Guid Subsystem_Common = new Guid("9cd510cd-abfc-11d4-9434-004095e12fc7"); // 3.0 - Общие объекты
        public static Guid Subsystem_Operations = new Guid("9fcd25a0-4822-11d4-9414-008048da11f9"); // 4.0 - Оперативный учёт
        public static Guid Subsystem_Accounting = new Guid("e3687481-0a87-462c-a166-9f34594f9bba"); // 5.0 - Бухгалтерский учёт
        public static Guid Subsystem_Calculation = new Guid("9de14907-ec23-4a07-96f0-85521cb6b53b"); // 6.0 - Расчёт
        public static Guid Subsystem_BusinessProcess = new Guid("51f2d5d8-ea4d-4064-8892-82951750031e"); // 7.0 - Бизнес-процессы

        #endregion

        #region "ИДЕНТИФИКАТОРЫ КОЛЛЕКЦИЙ ОБЪЕКТОВ МЕТАДАННЫХ"

        public static Guid Subsystems = new Guid("37f2fa9a-b276-11d4-9435-004095e12fc7"); // Подсистемы
        public static Guid SharedProperties = new Guid("15794563-ccec-41f6-a83c-ec5f7b9a5bc1"); // Общие реквизиты
        public static Guid NamedDataTypeSets = new Guid("c045099e-13b9-4fb6-9d50-fca00202971e"); // Определяемые типы
        public static Guid Catalogs = new Guid("cf4abea6-37b2-11d4-940f-008048da11f9");
        public static Guid Constants = new Guid("0195e80c-b157-11d4-9435-004095e12fc7");
        public static Guid Documents = new Guid("061d872a-5787-460e-95ac-ed74ea3a3e84");
        public static Guid Enumerations = new Guid("f6a80749-5ad7-400b-8519-39dc5dff2542");
        public static Guid Publications = new Guid("857c4a91-e5f4-4fac-86ec-787626f1c108"); // Планы обмена
        public static Guid Characteristics = new Guid("82a1b659-b220-4d94-a9bd-14d757b95a48");
        public static Guid InformationRegisters = new Guid("13134201-f60b-11d5-a3c7-0050bae0a776");
        public static Guid AccumulationRegisters = new Guid("b64d9a40-1642-11d6-a3c7-0050bae0a776");

        #endregion

        #region "ИДЕНТИФИКАТОРЫ ТИПОВ ДАННЫХ"

        public static readonly Guid VALUE_STORAGE = new Guid("e199ca70-93cf-46ce-a54b-6edc88c3a296"); // ХранилищеЗначения - varbinary(max)
        public static readonly Guid UNIQUEIDENTIFIER = new Guid("fc01b5df-97fe-449b-83d4-218a090e681e"); // УникальныйИдентификатор - binary(16)
        public static readonly Guid ANY_REFERENCE = new Guid("280f5f0e-9c8a-49cc-bf6d-4d296cc17a63"); // ЛюбаяСсылка
        public static readonly Guid ACCOUNT_REFERENCE = new Guid("ac606d60-0209-4159-8e4c-794bc091ce38"); // ПланСчетовСсылка
        public static readonly Guid CATALOG_REFERENCE = new Guid("e61ef7b8-f3e1-4f4b-8ac7-676e90524997"); // СправочникСсылка
        public static readonly Guid DOCUMENT_REFERENCE = new Guid("38bfd075-3e63-4aaa-a93e-94521380d579"); // ДокументСсылка
        public static readonly Guid ENUMERATION_REFERENCE = new Guid("474c3bf6-08b5-4ddc-a2ad-989cedf11583"); // ПеречислениеСсылка
        public static readonly Guid PUBLICATION_REFERENCE = new Guid("0a52f9de-73ea-4507-81e8-66217bead73a"); // ПланОбменаСсылка
        public static readonly Guid CHARACTERISTIC_REFERENCE = new Guid("99892482-ed55-4fb5-a7f7-20888820a758"); // ПланВидовХарактеристикСсылка

        #endregion
    }
}