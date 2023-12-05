using Serilog;
using System;
using System.Collections.Concurrent;
using System.Threading;
using WizudaCodingExercise.Models;

namespace WizudaCodingExercise.Abstraction
{
    public class LRUCache<TKey, TValue> : ILRUCache<TKey, TValue>
    {
        private readonly ILogger logger;
        private readonly int capacity;
        private readonly ConcurrentDictionary<TKey, CacheNode> cache;
        private readonly LinkedList<CacheNode> lruList;

        private readonly object evictionLock = new object();
        private readonly object eventLock = new object();
        private readonly ReaderWriterLockSlim lockObject = new ReaderWriterLockSlim();
        private static readonly Lazy<LRUCache<TKey, TValue>> lazyInstance;

        private static int ConfigurableCapacity { get; set; } = 100;

        static LRUCache()
        {
            lazyInstance = new Lazy<LRUCache<TKey, TValue>>(() => new LRUCache<TKey, TValue>(ConfigurableCapacity));
        }

        private LRUCache(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("Capacity must be greater than zero.");
            }

            this.capacity = capacity;
            cache = new ConcurrentDictionary<TKey, CacheNode>();
            lruList = new LinkedList<CacheNode>();
            logger = Log.ForContext<LRUCache<TKey, TValue>>();

            ItemEvicted += (sender, args) => { };
        }

        public static LRUCache<TKey, TValue> Instance
        {
            get
            {
                lock (lazyInstance)
                {
                    return lazyInstance.Value;
                }
            }
        }

        public event EventHandler<EvictionEventArgs<TKey, TValue>> ItemEvicted;


        public static void SetCapacity(int newCapacity)
        {
            lock (lazyInstance)
            {
                if (lazyInstance.IsValueCreated)
                {
                    throw new InvalidOperationException("Cannot set capacity after the singleton instance has been created.");
                }

                ConfigurableCapacity = newCapacity;
            }
        }

        public void AddOrUpdate(TKey key, TValue value)
        {
           
            try
            {
                if (cache.TryGetValue(key, out CacheNode node))
                {
                    node.Value = value;
                    node.AccessTime = DateTime.UtcNow;
                    lruList.Remove(node);
                    lruList.AddLast(node);
                }
                else
                {
                    if (cache.Count >= capacity)
                        Evict();

                    CacheNode newNode = new CacheNode(key, value);
                    cache.TryAdd(key, newNode);
                    lruList.AddLast(newNode);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in AddOrUpdate");
            }
            
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lockObject.EnterReadLock();

            try
            {
                if (cache.TryGetValue(key, out CacheNode node))
                {
                    node.AccessTime = DateTime.UtcNow;
                    lruList.Remove(node);
                    lruList.AddLast(node);

                    value = node.Value;
                    return true;
                }

                value = default;
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in TryGetValue");

                value = default;
                return false;
            }
            finally
            {
                lockObject.ExitReadLock();
            }
        }

        private void Evict()
        {
            try
            {
                CacheNode lruNode;

                lock (lruList)
                {
                    lruNode = lruList.First.Value;
                    lruList.RemoveFirst();
                }

                if (cache.TryRemove(lruNode.Key, out _))
                {
                    logger.Information($"Evicted: Key = {lruNode.Key}, Value = {lruNode.Value}");
                    OnItemEvicted(lruNode.Key, lruNode.Value);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in Evict");
            }
        }


        protected virtual void OnItemEvicted(TKey key, TValue value)
        {
            EventHandler<EvictionEventArgs<TKey, TValue>>? handlers;

            lock (eventLock)
            {
                handlers = ItemEvicted;
            }

            handlers?.Invoke(this, new EvictionEventArgs<TKey, TValue>(key, value));
        }


        private class CacheNode
        {
            public TKey Key { get; }
            public TValue Value { get; set; }
            public DateTime AccessTime { get; set; }

            public CacheNode(TKey key, TValue value)
            {
                Key = key;
                Value = value;
                AccessTime = DateTime.UtcNow;
            }
        }
    }
}
