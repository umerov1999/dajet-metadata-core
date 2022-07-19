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
                    using (ScriptParser parser = new(script))
                    {
                        foreach (ScriptToken token in parser.Parse())
                        {
                            Console.WriteLine(token);
                        }
                    }
                }
            }
        }
    }
}