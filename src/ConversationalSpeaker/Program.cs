using System.Reflection;
using ConversationalSpeaker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.SemanticKernel;

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
    services.Configure<AzureOpenAiOptions>(configurationRoot.GetSection("AzureOpenAI"));
    services.Configure<GeneralOptions>(configurationRoot.GetSection("General"));
    services.Configure<OpenAiServiceOptions>(configurationRoot.GetSection("OpenAI"));

    // Add Semantic Kernel
    services.AddSingleton<IKernel>(serviceProvider => Kernel.Builder.Build());

    // Add Skills
    services.AddSingleton<AzCognitiveServicesSpeechSkill>();
    services.AddSingleton<OpenAISkill>();
    services.AddSingleton<AzOpenAISkill>();

    // Add wake phrase listener
    services.AddSingleton<AzCognitiveServicesWakeWordListener>();

    // Add the primary hosted service to start the loop.
    services.AddHostedService<ConversationLoopHostedService>();
});

IHost host = builder.Build();
await host.RunAsync();
