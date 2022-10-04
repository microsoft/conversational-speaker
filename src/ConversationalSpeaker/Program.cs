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

    SystemOptions systemOptions = new SystemOptions();
    configurationRoot.GetSection("System").Bind(systemOptions);

    services.AddSingleton<ConcurrentMessageQueue<UserInput>>();
    services.AddSingleton<ConcurrentMessageQueue<SpeakRequest>>();
    services.AddOpenAIService((settings) =>
    {
        settings.ApiKey = (services.BuildServiceProvider().GetService<IOptions<OpenAiServiceOptions>>() as IOptions<OpenAiServiceOptions>).Value.Key;
    });
    
    // Add the listener
    if (systemOptions.TextListener)
    {
        services.AddSingleton<IListener, LocalKeyboardHandler>();
    }
    else
    {
        services.AddSingleton<IListener, AzCognitiveServicesListener>();
    }
    services.AddSingleton<ReadyToListenSignal>();
    
    // Add the speaker
    services.AddSingleton<ISpeaker, AzCognitiveServicesSpeaker>();
    
    // Add the conversation processor
    services.AddSingleton<IConversationHandler, PromptEngineHandler>();

    services.AddHostedService<ListenerHostedService>();
    services.AddHostedService<ConversationHostedService>();
    services.AddHostedService<SpeakerHostedService>();
});

IHost host = builder.Build();
await host.RunAsync();
