/*
 * TCache.cs is a class wrapper around the .NET ConcurrentDictionary 
 * which stores key value pairs and enables the values to become stale
 * after which the value objects are removed.
 * 
    Copyright (C) 2021  Steven Walsh

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

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
