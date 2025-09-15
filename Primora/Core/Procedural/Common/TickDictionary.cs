using System;
using System.Collections;
using System.Collections.Generic;

namespace Primora.Core.Procedural.Common
{
    /// <summary>
    /// A dictionary with kvps that can expire based on a tick.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class TickDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : notnull
    {
        private readonly Dictionary<TKey, (TValue Value, int TicksLeft)> _cache = [];

        public TValue this[TKey key, int ttl]
        {
            set { AddOrUpdate(key, value, ttl); }
        }

        public TValue this[TKey key]
        {
            get { return _cache[key].Value; }
        }

        /// <summary>
        /// Raised when a key-value pair expires and is removed from the dictionary.
        /// </summary>
        internal event EventHandler<ExpireArgs> OnExpire;

        /// <summary>
        /// Number of items currently in the cache.
        /// </summary>
        public int Count => _cache.Count;

        /// <summary>
        /// Add or refresh an item with a given TTL in ticks.
        /// </summary>
        public void AddOrUpdate(TKey key, TValue value, int ttl)
        {
            if (ttl <= 0) throw new ArgumentOutOfRangeException(nameof(ttl), "TTL must be > 0");
            _cache[key] = (value, ttl);
        }

        /// <summary>
        /// Try to get a value from the cache without refreshing its TTL.
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                value = entry.Value;
                return true;
            }

            value = default!;
            return false;
        }

        /// <summary>
        /// Advance one tick, decrement TTLs, and remove expired entries.
        /// </summary>
        public void Tick()
        {
            List<TKey> expired = null;

            foreach (var kvp in _cache)
            {
                int newTicks = kvp.Value.TicksLeft - 1;
                if (newTicks <= 0)
                {
                    expired ??= [];
                    expired.Add(kvp.Key);
                }
                else
                {
                    _cache[kvp.Key] = (kvp.Value.Value, newTicks);
                }
            }

            if (expired != null)
            {
                foreach (var key in expired)
                {
                    if (_cache.Remove(key, out var container))
                        OnExpire?.Invoke(this, new ExpireArgs(key, container.Value));
                }
            }
        }

        /// <summary>
        /// Remove a key explicitly.
        /// </summary>
        public bool Remove(TKey key) => _cache.Remove(key);

        /// <summary>
        /// Clear the entire cache.
        /// </summary>
        public void Clear() => _cache.Clear();

        /// <summary>
        /// Enumerator for active items (ignores TTLs).
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var kvp in _cache)
                yield return new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value.Value);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal class ExpireArgs(TKey key, TValue value) : EventArgs
        {
            public TKey Key { get; } = key;
            public TValue Value { get; } = value;
        }
    }
}
