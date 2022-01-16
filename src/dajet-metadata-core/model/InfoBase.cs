namespace DaJet.Metadata.Model
{
    public sealed class InfoBase : MetadataObject
    {
        /// <summary>
        /// Версия среды выполнения платформы
        /// </summary>
        public int PlatformVersion { get; set; }
        /// <summary>
        /// Режим совместимости платформы
        /// </summary>
        public int СompatibilityVersion { get; set; }
        /// <summary>
        /// Версия прикладной конфигурации
        /// </summary>
        public string AppConfigVersion { get; set; } = string.Empty;
        /// <summary>
        /// Смещение дат
        /// </summary>
        public int YearOffset { get; set; }
        /// <summary>
        /// Режим использования синхронных вызовов расширений платформы и внешних компонент
        /// </summary>
        public SyncCallsMode SyncCallsMode { get; set; }
        /// <summary>
        /// Режим управления блокировкой данных
        /// </summary>
        public DataLockingMode DataLockingMode { get; set; }
        /// <summary>
        /// Режим использования модальности
        /// </summary>
        public ModalWindowMode ModalWindowMode { get; set; }
        /// <summary>
        /// Режим автонумерации объектов
        /// </summary>
        public AutoNumberingMode AutoNumberingMode { get; set; }
        /// <summary>
        /// Режим совместимости интерфейса
        /// </summary>
        public UICompatibilityMode UICompatibilityMode { get; set; }
    }
}