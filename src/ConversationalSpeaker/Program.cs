using ConversationalSpeaker;
using Microsoft.AI.PromptEngine.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Reflection;

IHostBuilder builder = Host.CreateDefaultBuilder(args);

builder.ConfigureLogging((context, loggingBuilder) =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddConfiguration(context.Configuration.GetSection("Logging"));
    loggingBuilder.AddConsole(options => options.FormatterName = nameof(CleanConsoleFormatter))
        .AddConsoleFormatter<CleanConsoleFormatter, ConsoleFormatterOptions>();
});

string configurationFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "configuration.json");
    
builder.ConfigureAppConfiguration((builder) => builder
    .AddJsonFile(configurationFilePath)
    .AddEnvironmentVariables()  
    .AddUserSecrets<Program>());

builder.ConfigureServices((context, services) =>
{
    // Setup configuration options
    IConfiguration configurationRoot = context.Configuration;
    services.Configure<AzureCognitiveServicesOptions>(configurationRoot.GetSection("AzureCognitiveServices"));
    services.Configure<OpenAiServiceOptions>(configurationRoot.GetSection("OpenAI"));
    services.Configure<Settings>(configurationRoot.GetSection("PromptEngine"));

    // Add the listener
    services.AddSingleton<AzCognitiveServicesWakeWordListener>();

    // Add the listener
    services.AddSingleton<AzCognitiveServicesListener>();
    
    // Add the speaker
    services.AddSingleton<AzCognitiveServicesSpeaker>();
    
    // Add the conversation processor
    services.AddSingleton<PromptEngineHandler>();

    // Add the primary hosted service to start the loop.
    services.AddHostedService<ConversationLoopHostedService>();
});

IHost host = builder.Build();
await host.RunAsync();
