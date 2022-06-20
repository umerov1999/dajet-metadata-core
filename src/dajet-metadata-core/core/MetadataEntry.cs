using DaJet.Metadata.Model;
using System;

namespace DaJet.Metadata.Core
{
    ///<summary>
    ///<b>Элемент описания объекта метаданных:</b>
    ///<br>MetadataType - UUID общего типа метаданных, например, "Справочник"</br>
    ///<br>MetadataUuid - UUID объекта метаданных, например, "Справочник.Номенклатура"</br>
    ///<br>Name - имя объекта метаданных</br>
    ///<br>DbName - идентификатор СУБД объекта метаданных</br>
    ///<br>TypeCode - уникальный код объекта СУБД и метаданных</br>
    ///<br>Entry - кэшируемый объект метаданных</br>
    ///</summary>
    public sealed class MetadataEntry
    {
        ///<summary>UUID общего типа метаданных, например, "Справочник"</summary>
        public Guid MetadataType { get; set; }
        ///<summary>UUID объекта метаданных, например, "Справочник.Номенклатура"</summary>
        public Guid MetadataUuid { get; set; }
        ///<summary>Имя объекта метаданных</summary>
        public string Name { get; set; }
        ///<summary>Уникальный код объекта СУБД и метаданных</summary>
        public int TypeCode { get; set; }
        ///<summary>Идентификатор СУБД объекта метаданных</summary>
        public string DbName { get; set; }
        ///<summary>Кэшируемый объект метаданных</summary>
        public WeakReference<MetadataObject> Entry { get; set; }
    }
}