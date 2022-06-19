using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Parsers
{
    public interface IMetadataObjectParser
    {
        //TODO: void Parse(in ConfigFileReader source, out MetadataObject target);
        void Parse(in ConfigFileReader source, out MetadataObject target, out List<Guid> references);
        void Parse(in ConfigFileReader source, out MetadataObject target, out Dictionary<MetadataProperty, List<Guid>> references);
    }
}