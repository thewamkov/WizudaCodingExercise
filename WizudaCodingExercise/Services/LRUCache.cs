
using Serilog;
using System.Collections.Concurrent;
using WizudaCodingExercise.Abstraction;

namespace WizudaCodingExercise.Services
{
    public class LRUCache<TKey, TValue> : ILRUCache<TKey, TValue>
    {
        private readonly ILogger logger;
        private readonly int capacity;
        private readonly ConcurrentDictionary<TKey, CacheNode> cache;
        private readonly LinkedList<CacheNode> lruList;

        private readonly ReaderWriterLockSlim lockObject = new ReaderWriterLockSlim();

        public LRUCache(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("Capacity must be greater than zero.");
            }

            this.capacity = capacity;
            this.cache = new ConcurrentDictionary<TKey, CacheNode>();
            this.lruList = new LinkedList<CacheNode>();
            this.logger = Log.ForContext<LRUCache<TKey, TValue>>();
        }

        public void AddOrUpdate(TKey key, TValue value)
        {
            lockObject.EnterWriteLock();

            try
            {
                if (cache.TryGetValue(key, out CacheNode node))
                {
                    // Update existing entry
                    node.Value = value;
                    node.AccessTime = DateTime.Now;
                    lruList.Remove(node);
                    lruList.AddLast(node);
                }
                else
                {
                    // Add new entry
                    if (cache.Count >= capacity)
                        Evict();

                    CacheNode newNode = new CacheNode(key, value);
                    cache.TryAdd(key, newNode);
                    lruList.AddLast(newNode);
                }
            }
            catch (Exception ex)
            {
                // Log the exception using Serilog
                logger.Error(ex, "Error in AddOrUpdate");
            }
            finally
            {
                lockObject.ExitWriteLock();
            }
        }


        public bool TryGetValue(TKey key, out TValue value)
        {
            lockObject.EnterReadLock();

            try
            {
                if (cache.TryGetValue(key, out CacheNode node))
                {
                    node.AccessTime = DateTime.Now;
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
            lockObject.EnterWriteLock();

            try
            {
                CacheNode lruNode = lruList.First.Value;
                cache.TryRemove(lruNode.Key, out _);
                lruList.RemoveFirst();

                logger.Information($"Evicted: {lruNode.Key}");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in Evict");
            }
            finally
            {
                lockObject.ExitWriteLock();
            }
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
                AccessTime = DateTime.Now;
            }
        }
    }
}