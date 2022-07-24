using DaJet.Metadata.Model;
using System.Collections.Generic;
using System.IO;

namespace DaJet.Metadata.Services
{
    public interface IDbViewGenerator
    {
        DbViewGeneratorOptions Options { get; }
        bool SchemaExists(string name);
        void CreateSchema(string name);
        void DropSchema(string name);
        bool TryCreateSchemaIfNotExists(out string error);
        string GenerateViewScript(in ApplicationObject metadata, string viewName);
        string GenerateEnumViewScript(in Enumeration enumeration, string viewName);
        bool TryScriptView(in StreamWriter writer, in ApplicationObject metadata, out string error);
        bool TryScriptViews(in MetadataCache cache, out int result, out List<string> errors);
        bool TryCreateView(in ApplicationObject metadata, out string error);
        bool TryCreateViews(in MetadataCache cache, out int result, out List<string> errors);
        int DropViews();
        void DropView(in ApplicationObject metadata);
    }
}