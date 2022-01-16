using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;

namespace DaJet.Metadata.Services
{
    public static class MetadataObjectFactory
    {
        private static Dictionary<Guid, Func<MetadataObject>> _factories = new Dictionary<Guid, Func<MetadataObject>>()
        {
            { new Guid("37f2fa9a-b276-11d4-9435-004095e12fc7"), null }, // Подсистемы
            { new Guid("c045099e-13b9-4fb6-9d50-fca00202971e"), () => { return new NamedDataTypeSet(); } }, // Определяемые типы
            { new Guid("15794563-ccec-41f6-a83c-ec5f7b9a5bc1"), () => { return new SharedProperty(); } }, // Общие реквизиты
            { new Guid("cf4abea6-37b2-11d4-940f-008048da11f9"), () => { return new Catalog(); } },
            { new Guid("0195e80c-b157-11d4-9435-004095e12fc7"), () => { return new Constant(); } },
            { new Guid("061d872a-5787-460e-95ac-ed74ea3a3e84"), () => { return new Document(); } },
            { new Guid("f6a80749-5ad7-400b-8519-39dc5dff2542"), () => { return new Enumeration(); } },
            { new Guid("857c4a91-e5f4-4fac-86ec-787626f1c108"), () => { return new Publication(); } }, // Планы обмена
            { new Guid("82a1b659-b220-4d94-a9bd-14d757b95a48"), () => { return new Characteristic(); } },
            { new Guid("13134201-f60b-11d5-a3c7-0050bae0a776"), () => { return new InformationRegister(); } },
            { new Guid("b64d9a40-1642-11d6-a3c7-0050bae0a776"), () => { return new AccumulationRegister(); } }
        };
        public static Func<MetadataObject> GetFactory(Guid uuid)
        {
            if (_factories.TryGetValue(uuid, out Func<MetadataObject> factory))
            {
                return factory;
            }
            return null;
        }
        public static MetadataObject CreateObject(Guid uuid)
        {
            Func<MetadataObject> factory = GetFactory(uuid);
            if (factory == null)
            {
                return null;
            }
            return factory();
        }
    }
}