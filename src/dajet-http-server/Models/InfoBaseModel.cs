﻿namespace DaJet.Http.Model
{
    public sealed class InfoBaseModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DatabaseProvider { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
    }
}