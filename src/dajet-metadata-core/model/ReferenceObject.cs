using System;

namespace DaJet.Metadata.Model
{
    public abstract class ReferenceObject : ApplicationObject
    {
        ///<summary>
        ///Идентификатор ссылочного типа данных, например,
        ///"СправочникСсылка.Номенклатура" или "ДокументСсылка.ЗаказКлиента".
        ///<br>
        ///Используется для определения типов данных свойств объектов.
        ///</br>
        ///</summary>
        public Guid Reference { get; set; } = Guid.Empty;
    }
}