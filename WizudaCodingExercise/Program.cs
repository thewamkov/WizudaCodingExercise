using Serilog;
using WizudaCodingExercise.Services;

//can be configired more complexly, but this is fine for now
Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();


Log.CloseAndFlush();