using SpeechToClipboard.Services.Implementation;

const string defaultFileName = "FindReplace.json";
const LogLevel logLevel = LogLevel.Error;
const int port = 7654; 

// Override the file name from the command line arguments
var fileName = args.Any() ? args[0] : defaultFileName;

// Initialize the cancellation token source and web app
var cts = CreateCancellationTokenSource();
var app = CreateWebApp(fileName, args);

// Start the web app and wait for the escape key
var runTask = app.RunAsync(cts.Token);
var escapeTask = WaitForEscapeAsync(cts);

Console.WriteLine("Press Esc to exit");

// Wait for either task to complete
await Task.WhenAny(runTask, escapeTask);

Console.WriteLine();
Console.WriteLine("Exiting");

return 0;

static CancellationTokenSource CreateCancellationTokenSource()
{
    var cts = new CancellationTokenSource();
    
    // Register the cancellation token with the process exit events (kill, ctrl+c, etc)
    AppDomain.CurrentDomain.ProcessExit += (_, _) => cts.Cancel();
    Console.CancelKeyPress += (_, _) => cts.Cancel();
    
    return cts;
}

static WebApplication CreateWebApp(string fileName, string[] args)
{
    var findReplaceService = FindReplaceService.FromFile(fileName);
    
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSingleton(findReplaceService);
    builder.Services.AddControllersWithViews();
    builder.Logging.SetMinimumLevel(logLevel);
    builder.WebHost.UseKestrel(x => x.ListenAnyIP(port));

    var app = builder.Build();
    app.UseRouting();
    app.MapControllers();

    return app;
}

static async Task WaitForEscapeAsync(CancellationTokenSource cts)
{
    // The dotnet console SDK has always been weak.
    // This is the best way to check for a key press without blocking 
    while (!cts.IsCancellationRequested)
    {
        // KeyAvailable does not block
        if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Escape)
        {
            // Capturing esc will back out one character from console output
            // so write a character to overwrite it
            Console.Write('e');
            cts.Cancel();
        }
        else
            await Task.Delay(200);
    }
}