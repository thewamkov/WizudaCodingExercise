
using WizudaCodingExercise.Abstraction;

namespace WizudaCodingExercise.Services
{
    public class LRUCache<TKey, TValue>: ILRUCache<TKey, TValue>
    {
        private readonly int capacity;
        private readonly Dictionary<TKey, CacheNode> cache;
        private readonly LinkedList<CacheNode> lruList;

        private readonly object lockObject = new object();

        public LRUCache(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("Capacity must be greater than zero.");
            }

            this.capacity = capacity;
            cache = new Dictionary<TKey, CacheNode>(capacity);
            lruList = new LinkedList<CacheNode>();
        }

        public void AddOrUpdate(TKey key, TValue value)
        {
            lock (lockObject)
            {
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
                        {
                            // Evict least recently used item
                            Evict();
                        }

                        CacheNode newNode = new CacheNode(key, value);
                        cache.TryAdd(key, newNode);
                        lruList.AddLast(newNode);
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions (log, rethrow, or take appropriate action)
                    Console.WriteLine($"Exception in AddOrUpdate: {ex.Message}");
                }
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (lockObject)
            {
                try
                {
                    if (cache.TryGetValue(key, out CacheNode node))
                    {
                        // Move the accessed item to the end of the LRU list
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
                    Console.WriteLine($"Exception in TryGetValue: {ex.Message}");
                    value = default;
                    return false;
                }
            }
        }

        private void Evict()
        {
            // Implement your eviction strategy here (LRU, LFU, etc.)
            CacheNode lruNode = lruList.First.Value;
            cache.Remove(lruNode.Key);
            lruList.RemoveFirst();

            Console.WriteLine($"Evicted: {lruNode.Key}");
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