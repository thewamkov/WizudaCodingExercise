using Serilog;
using WizudaCodingExercise.Services;

//can be configired more complexly, but this is fine for now
Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

LRUCache<string, int> cache = new LRUCache<string, int>(3);

// Add some items to the cache
cache.AddOrUpdate("One", 1);
cache.AddOrUpdate("Two", 2);
cache.AddOrUpdate("Three", 3);

Log.CloseAndFlush();