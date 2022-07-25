using DaJet.Metadata;
using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System.Reflection;
using System.Text;

namespace DaJet.CSharp.Generator
{
    public sealed class EntityModelGenerator
    {
        private readonly IMetadataCache _cache;
        private readonly EntityModelGeneratorOptions _options;
        public EntityModelGenerator(EntityModelGeneratorOptions options, IMetadataCache cache)
        {
            _cache = cache;
            _options = options;
        }
        public Assembly Generate(out List<string> errors)
        {
            errors = new List<string>();

            string assemblyName = $"{_cache.InfoBase.Name}_{_options.Version.Replace('.', '_')}";

            string sourceCode = GenerateSourceCode();

            if (!string.IsNullOrWhiteSpace(_options.OutputFile))
            {
                File.WriteAllText(_options.OutputFile, sourceCode);
            }

            string assemblyPath = Path.GetDirectoryName(_options.OutputFile);
            assemblyPath = Path.Combine(assemblyPath, $"{assemblyName}.dll");

            Compiler compiler = new Compiler();
            byte[] buffer = compiler.Compile(sourceCode, _cache.InfoBase.Name, in errors);

            if (buffer == null)
            {
                return null!;
            }

            File.WriteAllBytes(assemblyPath, buffer);
            
            return Assembly.Load(buffer);
        }
        private string GenerateSourceCode()
        {
            int indent = 0;

            StringBuilder code = new StringBuilder();

            code.AppendLine("using DaJet.Data;");
            code.AppendLine("using System;");
            code.AppendLine("using System.Reflection;");
            code.AppendLine("[assembly: AssemblyTitle(\"DaJet.CSharp.Generator\")]");
            code.AppendLine($"[assembly: AssemblyVersion(\"{_options.Version}\")]");
            code.AppendLine();

            code.AppendLine($"namespace {_cache.InfoBase.Name}");
            code.AppendLine("{"); // open database namespace

            foreach (Guid type in MetadataTypes.ApplicationObjectTypes)
            {
                string _namespace = MetadataTypes.ResolveNameRu(type);

                code.AppendLine($"\tnamespace {_namespace}");
                code.AppendLine("\t{"); // open namespace

                foreach (MetadataItem item in _cache.GetMetadataItems(type))
                {
                    MetadataObject metadata = _cache.GetMetadataObject(item);

                    GenerateClassCode(code, metadata, 2);
                }
                
                code.AppendLine("\t}"); // close namespace
            }

            code.Append("}"); // close database namespace

            return code.ToString();
        }
        private void GenerateClassCode(StringBuilder code, MetadataObject metadata, int indent)
        {
            if (metadata is not ApplicationObject entity)
            {
                return;
            }

            code.Append($"{"\t".PadLeft(indent)}public sealed class {metadata.Name}");
            //if (metadata is Catalog || metadata is Document)
            //{
            //    code.Append(" : ReferenceObject");
            //}
            code.AppendLine();
            code.AppendLine($"{"\t".PadLeft(indent)}{{"); // open class

            foreach (MetadataProperty property in entity.Properties)
            {
                GeneratePropertyCode(code, entity, property, indent + 2);
            }

            if (entity is ITablePartOwner owner)
            {
                foreach (TablePart table in owner.TableParts)
                {
                    // TODO: generate property for table part items collection

                    GenerateClassCode(code, table, indent + 1);
                }
            }
            
            code.AppendLine($"{"\t".PadLeft(indent)}}}"); // close class
        }
        private void GeneratePropertyCode(StringBuilder code, ApplicationObject entity, MetadataProperty property, int indent)
        {
            string propertyType = "string";
            string propertyName = property.Name;

            if (property.PropertyType.IsMultipleType)
            {
                propertyType = "object"; //TODO: Union
            }
            else
            {
                if (property.PropertyType.IsBinary || property.PropertyType.IsValueStorage)
                {
                    propertyType = "byte[]";
                }
                else if (property.PropertyType.CanBeBoolean)
                {
                    propertyType = "bool";
                }
                else if (property.PropertyType.CanBeNumeric)
                {
                    propertyType = "decimal";
                }
                else if (property.PropertyType.CanBeString)
                {
                    propertyType = "string";
                }
                else if (property.PropertyType.CanBeDateTime)
                {
                    propertyType = "DateTime";
                }
                else if (property.PropertyType.IsUuid)
                {
                    propertyType = "Guid";
                }
                else if (property.PropertyType.CanBeReference)
                {
                    propertyType = "EntityRef";
                }
            }

            code.AppendLine($"{"\t".PadLeft(indent)}public {propertyType} {propertyName} {{ get; set; }}");
        }
    }
}