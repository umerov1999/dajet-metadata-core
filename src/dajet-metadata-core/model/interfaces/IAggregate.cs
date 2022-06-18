using System.Collections.Generic;

namespace DaJet.Metadata.Model
{
    public interface IAggregate
    {
        List<TablePart> TableParts { get; set; }
    }
}