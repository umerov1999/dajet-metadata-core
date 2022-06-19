using System;

namespace DaJet.Metadata.Core
{
    ///<summary>
    ///Структура информации о ссылках объекта метаданных:
    ///<br>MetadataType - UUID общего типа метаданных, например, "Справочник"</br>
    ///<br>MetadataUuid - UUID типа метаданных, например, "Справочник.Номенклатура"</br>
    ///<br>ReferenceUuid - UUID типа данных "Ссылка", например, "СправочникСсылка.Номенклатура"</br>
    ///<br>CharacteristicUuid - UUID типа данных "Характеристика", например, "Характеристика.ВидыСубконтоХозрасчетные"</br>
    ///</summary>
    public readonly struct ReferenceInfo // TODO: rename to MetaInfo ? !!!
    {
        internal ReferenceInfo(Guid type, Guid metadata, Guid reference)
        {
            MetadataType = type;
            MetadataUuid = metadata;
            ReferenceUuid = reference;
            CharacteristicUuid = Guid.Empty;
        }
        internal ReferenceInfo(Guid type, Guid metadata, Guid reference, Guid characteristic)
        {
            MetadataType = type;
            MetadataUuid = metadata;
            ReferenceUuid = reference;
            CharacteristicUuid = characteristic;
        }
        ///<summary>UUID общего типа метаданных, например, "Справочник"</summary>
        public readonly Guid MetadataType { get; }
        ///<summary>UUID типа метаданных, например, "Справочник.Номенклатура"</summary>
        public readonly Guid MetadataUuid { get; }
        ///<summary>UUID типа данных "Ссылка", например, "СправочникСсылка.Номенклатура"</summary>
        public readonly Guid ReferenceUuid { get; }
        ///<summary>UUID типа данных "Характеристика", например, "Характеристика.ВидыСубконтоХозрасчетные"</summary>
        public readonly Guid CharacteristicUuid { get; }
    }
}