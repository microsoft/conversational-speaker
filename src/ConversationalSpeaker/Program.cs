using ConversationalSpeaker;
using Microsoft.AI.PromptEngine.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.GPT3.Extensions;
using System.Reflection;

IHostBuilder builder = Host.CreateDefaultBuilder(args);

builder.ConfigureLogging((context, builder) =>
{
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = false;
        options.IncludeScopes = false;
        options.TimestampFormat = "hh:mm:ss ";
    });
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

    // Add the OpenAI service
    services.AddOpenAIService((settings) =>
    {
        settings.ApiKey = (services.BuildServiceProvider().GetService<IOptions<OpenAiServiceOptions>>()).Value.Key;
    });

    // Add the listener
    services.AddSingleton<AzCognitiveServicesListener>();
    // services.AddSingleton<LocalKeyboardHandler>();

    // Add the speaker
    services.AddSingleton<AzCognitiveServicesSpeaker>();
    
    // Add the conversation processor
    services.AddSingleton<PromptEngineHandler>();

    // Add the primary hosted service to start the loop.
    services.AddHostedService<SimpleLoopHostedService>();
});

IHost host = builder.Build();
await host.RunAsync();
