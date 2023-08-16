using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions;
using Microsoft.Extensions.Configuration;

namespace SquetBot
{
    public class Program
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        // Configuring the DiscordSocketClient to define its behavior
        private readonly DiscordSocketConfig _socketConfig = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
            MessageCacheSize = 100
        };

        public Program()
        {
            _configuration = ConfigBuilder(); // Build configuration settings from environment variables
            _serviceProvider = CreateProvider(_configuration); // Create a service provider for dependency injection
        }

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        static IConfiguration ConfigBuilder()
        {
            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables("Squet"); // Configure the builder to add environment variables prefixed with "Squet"

            return builder.Build(); // Build and return the configuration
        }

        static IServiceProvider CreateProvider(IConfiguration progConfig)
        {
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged, // Enable intents for all unprivileged events
                LogLevel = LogSeverity.Verbose, // Set log level to verbose
                MessageCacheSize = 100 // Limit the message cache size to 100 messages
            };

            var serviceConfig = new InteractionServiceConfig()
            {
                LogLevel = LogSeverity.Verbose // Set log level for interaction service to verbose
            };

            var services = new ServiceCollection()
                .AddSingleton(progConfig) // Add configuration as a singleton service
                .AddSingleton(config) // Add DiscordSocketConfig as a singleton service
                .AddSingleton<DiscordSocketClient>() // Add DiscordSocketClient as a singleton service
                .AddSingleton(serviceConfig) // Add InteractionServiceConfig as a singleton service
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>())) // Add InteractionService with a client dependency
                .AddSingleton<InteractionHandler>() // Add InteractionHandler as a singleton service
                .AddSingleton<MusicHandler>(); // Add MusicHandler as a singleton service

            return services.BuildServiceProvider(); // Build and return the service provider
        }

        public async Task MainAsync()
        {
            var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();

            client.Log += LoggingHandler.LogAsync; // Attach a log event handler

            await _serviceProvider.GetRequiredService<InteractionHandler>()
                .InitializeAsync(); // Initialize the interaction handler

            var _token = _configuration["Token"]; // Retrieve the bot token from configuration settings

            await client.LoginAsync(TokenType.Bot, _token); // Log in to Discord using the bot token
            await client.StartAsync(); // Start the Discord client

            await Task.Delay(-1); // Keep the application running indefinitely
        }

        public static bool IsDebug()
        {
#if DEBUG
            return true;
#else
                return false;
#endif
        }
    }
}