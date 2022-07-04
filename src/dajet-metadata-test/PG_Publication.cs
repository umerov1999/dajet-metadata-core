using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DaJet.Metadata.Test
{
    [TestClass] public class PG_Publication
    {
        private const string PUBLICATION_NAME = "DaJetMessaging";
        private const string PG_CONNECTION_STRING = "Host=127.0.0.1;Port=5432;Database=dajet-messaging-pg;Username=postgres;Password=postgres;";

        private readonly MetadataService _metadata = new();
        public PG_Publication()
        {
            MetadataServiceOptions options = new()
            {
                ConnectionString = PG_CONNECTION_STRING,
                DatabaseProvider = DatabaseProvider.PostgreSQL
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