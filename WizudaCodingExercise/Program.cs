// Create an instance of LRUCache with a capacity of 3
using WizudaCodingExercise.Services;

LRUCache<string, int> cache = new LRUCache<string, int>(3);

// Add some items to the cache
cache.AddOrUpdate("One", 1);
cache.AddOrUpdate("Two", 2);
cache.AddOrUpdate("Three", 3);

// Try to retrieve items from the cache
if (cache.TryGetValue("One", out int value1))
{
    Console.WriteLine("Value for key 'One': " + value1);
}
else
{
    Console.WriteLine("Key 'One' not found.");
}

if (cache.TryGetValue("Two", out int value2))
{
    Console.WriteLine("Value for key 'Two': " + value2);
}
else
{
    Console.WriteLine("Key 'Two' not found.");
}

// Add another item (causing eviction)
cache.AddOrUpdate("Four", 4);

// Try to retrieve the evicted item
if (cache.TryGetValue("Three", out int value3))
{
    Console.WriteLine("Value for key 'Three': " + value3);
}
else
{
    Console.WriteLine("Key 'Three' not found (evicted).");
}

// Try to retrieve a non-existing key
if (cache.TryGetValue("Five", out int value5))
{
    Console.WriteLine("Value for key 'Five': " + value5);
}
else
{
    Console.WriteLine("Key 'Five' not found.");
}
