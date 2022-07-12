using System;

namespace DaJet.Metadata
{
    internal sealed class CacheEntry
    {
        private readonly RWLockSlim _lock = new();
        private readonly InfoBaseOptions _options;
        private readonly WeakReference<MetadataCache> _value = new(null);
        private long _lastUpdate = 0L; // milliseconds
        internal CacheEntry(InfoBaseOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        internal InfoBaseOptions Options { get { return _options; } }
        internal RWLockSlim.UpgradeableLockToken UpdateLock() { return _lock.UpgradeableLock(); }
        internal MetadataCache Value
        {
            set
            {
                using (_lock.WriteLock())
                {
                    _value.SetTarget(value);
                    _lastUpdate = Environment.TickCount64;
                }
            }
            get
            {
                using (_lock.ReadLock())
                {
                    if (_value.TryGetTarget(out MetadataCache value))
                    {
                        return value;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
        internal bool IsExpired
        {
            get
            {
                long elapsed = (Environment.TickCount64 - _lastUpdate) / 1000;

                return _options.Expiration < elapsed;
            }
        }
        internal void Dispose()
        {
            _lock.Dispose();
            _value.SetTarget(null);
            _options.ConnectionString = null;
        }
    }
}