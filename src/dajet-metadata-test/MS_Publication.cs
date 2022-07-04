using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DaJet.Metadata.Test
{
    [TestClass] public class MS_Publication
    {
        private const string PUBLICATION_NAME = "DaJetMessaging";
        private const string MS_CONNECTION_STRING = "Data Source=ZHICHKIN;Initial Catalog=dajet-messaging-ms;Integrated Security=True;Encrypt=False;";

        private readonly MetadataService _metadata = new();
        public MS_Publication()
        {
            MetadataServiceOptions options = new()
            {
                ConnectionString = MS_CONNECTION_STRING,
                DatabaseProvider = DatabaseProvider.SQLServer
            };

            _metadata.Configure(options);

            if (!_metadata.TryOpenInfoBase(out InfoBase _, out string error))
            {
                throw new InvalidOperationException(error);
            }
        }

        [TestMethod] public void Select_Publication()
        {
            Publication publication = _metadata.GetPublication(PUBLICATION_NAME);

            if (publication == null)
            {
                Console.WriteLine($"Metadata object \"{PUBLICATION_NAME}\" is not found.");
            }
            else
            {
                PublicationHelper.ShowMetadataObject(publication);
            }

            Console.WriteLine();
            PublicationHelper.ShowArticles(_metadata, publication);
        }
    }
}