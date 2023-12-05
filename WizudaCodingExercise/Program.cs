// Create an instance of LRUCache with a capacity of 3
using WizudaCodingExercise.Services;

LRUCache<string, int> cache = new LRUCache<string, int>(3);

// Add some items to the cache
cache.AddOrUpdate("One", 1);
cache.AddOrUpdate("Two", 2);
cache.AddOrUpdate("Three", 3);

