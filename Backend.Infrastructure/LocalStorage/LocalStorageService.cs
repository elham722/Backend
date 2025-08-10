using Hanssens.Net;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.LocalStorage
{
    public class LocalStorageService : ILocalStorageService
    {
        private readonly Hanssens.Net.LocalStorage _storage;
        private readonly LocalStorageOptions _options;

        public LocalStorageService(IOptions<LocalStorageOptions> options)
        {
            _options = options.Value;
            
            var config = new LocalStorageConfiguration()
            {
                AutoLoad = _options.AutoLoad,
                AutoSave = _options.AutoSave,
                Filename = _options.Filename
            };
            
            _storage = new Hanssens.Net.LocalStorage(config);
        }

        public void ClearStorage(List<string> keys)
        {
            foreach (var key in keys)
            {
                _storage.Remove(key);
            }
            _storage.Persist();
        }

        public void SetStorageValue<T>(string key, T value)
        {
            _storage.Store(key, value);
            _storage.Persist();
        }

        public T GetStorageValue<T>(string key)
        {
            return _storage.Get<T>(key);
        }

        public bool Exists(string key)
        {
            return _storage.Exists(key);
        }

        public void Remove(string key)
        {
            _storage.Remove(key);
            _storage.Persist();
        }

        public void ClearAll()
        {
            _storage.Destroy();
        }
    }
} 