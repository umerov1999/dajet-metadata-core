using DaJet.Metadata.Core;
using DaJet.Metadata.Model;

namespace DaJet.Metadata.Parsers
{
    public interface IMetadataObjectParser
    {
        void Parse(in ConfigFileReader source, out MetadataEntry target);
        void Parse(in ConfigFileReader source, out MetadataObject target);
    }
}