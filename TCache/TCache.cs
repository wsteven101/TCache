using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Cache.TCache
{
    public class TCache<K, T> : IEnumerable<KeyValuePair<K,T>> where K : class
    {

        private class CachedValue<T> {

            public T Value;
            public DateTime CreationDate = DateTime.Now;
            // last date value was created or replaced
            public DateTime LastSetDate;
            public DateTime LastAccessedDate;

            public CachedValue(T val)
            {
                Value = val;
                CreationDate = DateTime.Now;
                LastSetDate = CreationDate;
                LastAccessedDate = CreationDate;
            }
        }

        private ConcurrentDictionary<K, CachedValue<T>> _dataStore = new ConcurrentDictionary<K, CachedValue<T>>();

        public int StaleDataPeriod { get; set; } = 60 * 60 * 12; // default - data stale after 12 hours
        
        public T this[K key] { 

            get 
            {
                ClearStaleData();

                if (_dataStore.TryGetValue(key, out CachedValue<T> val))
                {
                    val.LastAccessedDate = DateTime.Now;
                    return val.Value;
                }
                return default;
            } 

            set {
                _dataStore[key] = new CachedValue<T>( value );

                ClearStaleData();
            } 

        }

        public void ClearStaleData(int olderThanSeconds = 0)
        {
            olderThanSeconds = (olderThanSeconds == 0) ? StaleDataPeriod : olderThanSeconds;

            var keys = new List<KeyValuePair<K,CachedValue<T>>>();
            var now = DateTime.Now;

            foreach(var i in _dataStore)
            {
                var diff = now - i.Value.LastSetDate;
                if (diff.TotalSeconds > olderThanSeconds)
                {
                    keys.Add(i);
                }
            }

            foreach(var r in keys)
            {
                _dataStore.TryRemove(r);
            }
        }

        public void Add(K key, T value) => this[key] = value;
                
        public IEnumerator<KeyValuePair<K, T>> GetEnumerator()
        {
            foreach(var i in _dataStore)
            {
                yield return new KeyValuePair<K, T>(i.Key,i.Value.Value);
            }
        }

        System.Collections.IEnumerator 
            System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        

    }
}
