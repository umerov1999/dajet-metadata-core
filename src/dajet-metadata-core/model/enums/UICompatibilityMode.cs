﻿namespace DaJet.Metadata.Model
{
    /// <summary>
    /// Режим совместимости интерфейса
    /// </summary>
    public enum UICompatibilityMode
    {
        /// <summary>
        /// Версия 8.2
        /// </summary>
        Version82 = 0,
        /// <summary>
        /// Версия 8.2. Разрешить Такси
        /// </summary>
        Version82AllowTaxi = 1,
        /// <summary>
        /// Такси. Разрешить Версия 8.2
        /// </summary>
        TaxiAllowVersion82 = 2,
        /// <summary>
        /// Такси
        /// </summary>
        Taxi = 3,
    }
}