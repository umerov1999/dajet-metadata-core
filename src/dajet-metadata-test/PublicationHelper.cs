using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;

namespace DaJet.Metadata.Test
{
    internal static class PublicationHelper
    {
        internal static void ShowMetadataObject(Publication @object)
        {
            Console.WriteLine($"Uuid: {@object.Uuid}");
            Console.WriteLine($"Name: {@object.Name}");
            Console.WriteLine($"Alias: {@object.Alias}");

            if (@object.Publisher == null)
            {
                Console.WriteLine($"Publisher: null");
            }
            else
            {
                Console.WriteLine($"Publisher: {@object.Publisher.Code}");
            }

            Console.WriteLine($"Subscribers:");
            foreach (Subscriber subscriber in @object.Subscribers)
            {
                Console.WriteLine($" - {subscriber.Code} [{(subscriber.IsMarkedForDeletion ? "Disabled" : "Active")}]");
            }
        }
        internal static void ShowArticles(MetadataService metadata, Publication publication)
        {
            Console.WriteLine("Articles:");

            foreach (var article in publication.Articles)
            {
                MetadataItem entry = metadata.GetMetadataItem(article.Key);

                if (entry == MetadataItem.Empty)
                {
                    continue;
                }

                string mainTable = metadata.GetMainTableName(entry.Uuid);
                string changeTable = metadata.GetChangeTableName(entry.Uuid);

                Console.WriteLine($" - {entry.Name} [{mainTable}] {{{article.Key}}} {article.Value} [{changeTable}]");
            }
        }
    }
}