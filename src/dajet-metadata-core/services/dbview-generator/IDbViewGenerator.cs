using DaJet.Metadata.Model;
using System.Collections.Generic;
using System.IO;

namespace DaJet.Metadata.Services
{
    public interface IDbViewGenerator
    {
        bool SchemaExists(string name);
        void CreateSchema(string name);
        void DropSchema(string name);
        string GenerateViewScript(in ApplicationObject metadata);
        string GenerateEnumViewScript(in Enumeration enumeration);
        bool TryScriptView(in StreamWriter writer, in ApplicationObject metadata, out string error);
        bool TryScriptViews(in InfoBase infoBase, out int result, out List<string> errors);
        bool TryCreateView(in ApplicationObject metadata, out string error);
        bool TryCreateViews(in InfoBase infoBase, out int result, out List<string> errors);
        int DropViews();
        void DropView(in ApplicationObject metadata);
    }
}