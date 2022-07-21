using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace DaJet.Scripting.Test
{
    [TestClass]
    public class TEST_Scripting
    {
        [TestMethod] public void Tokenize()
        {
            foreach (string filePath in Directory.GetFiles("C:\\temp\\scripting-test"))
            {
                Console.WriteLine("***");
                Console.WriteLine(filePath);

                using (StreamReader reader = new(filePath, Encoding.UTF8))
                {
                    string script = reader.ReadToEnd();
                    using (ScriptScanner scanner = new(script))
                    {
                        foreach (ScriptToken token in scanner.Scan())
                        {
                            Console.WriteLine(token);
                        }
                    }
                }
            }
        }
        [TestMethod] public void Parse()
        {
            foreach (string filePath in Directory.GetFiles("C:\\temp\\scripting-test"))
            {
                Console.WriteLine("***");
                Console.WriteLine(filePath);

                using (StreamReader reader = new(filePath, Encoding.UTF8))
                {
                    string script = reader.ReadToEnd();
                    using (ScriptParser parser = new(script))
                    {
                        SyntaxTree tree = parser.Parse();

                        foreach (SyntaxNode node in tree.Nodes)
                        {
                            ShowSyntaxNode(node, 0);
                        }
                    }
                }
            }
        }
        private void ShowSyntaxNode(SyntaxNode node, int level)
        {
            string offset = "-".PadLeft(level + 1);

            Console.WriteLine($"{offset} {node.Token.Text}");

            foreach (SyntaxNode child in node.Children)
            {
                ShowSyntaxNode(child, level + 1);
            }
        }
    }
}