using System;

namespace DaJet.Metadata.Model
{
    public abstract class ReferenceObject : ApplicationObject
    {
        ///<summary>
        ///Внутренний идентификатор ссылочного объекта метаданных.
        ///Используется для определения типов данных свойств объектов.
        ///</summary>
        public Guid TypeUuid { get; set; } = Guid.Empty;
    }
}