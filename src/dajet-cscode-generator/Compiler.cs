using DaJet.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace DaJet.CSharp.Generator
{
    internal sealed class Compiler
    {
        public byte[] Compile(string sourceCode, string assemblyName, in List<string> errors)
        {
            using (var stream = new MemoryStream())
            {
                var result = GenerateCode(sourceCode, assemblyName).Emit(stream);

                if (!result.Success)
                {
                    var failures = result.Diagnostics
                        .Where(diagnostic => diagnostic.IsWarningAsError
                        || diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (var diagnostic in failures)
                    {
                        errors.Add(string.Format("{0}: {1}", diagnostic.Id, diagnostic.GetMessage()));
                    }
                    return null!;
                }
                stream.Seek(0, SeekOrigin.Begin);
                return stream.ToArray();
            }
        }

        private static CSharpCompilation GenerateCode(string sourceCode, string assemblyName)
        {
            var codeString = SourceText.From(sourceCode);
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10);

            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

            //The location of the .NET assemblies
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location); // System.Private.CoreLib.dll

            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Reflection.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")), // aka mscorlib
                MetadataReference.CreateFromFile(typeof(EntityRef).Assembly.Location)
            };
            
            return CSharpCompilation.Create($"{assemblyName}",
                new[] { parsedSyntaxTree },
                references: references,
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release));
        }
    }
}