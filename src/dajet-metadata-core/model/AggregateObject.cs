using System.Collections.Generic;

namespace DaJet.Metadata.Model
{
    public abstract class AggregateObject : ReferenceObject
    {
        ///<summary>
        ///Табличные части ссылочного объекта метаданных.
        ///</summary>
        public List<TablePart> TableParts { get; set; } = new List<TablePart>();
    }
}