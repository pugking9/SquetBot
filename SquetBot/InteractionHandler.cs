using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SquetBot
{
    internal class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;

        public InteractionHandler (DiscordSocketClient client, InteractionService interactionService, IServiceProvider services, IConfiguration configuration)
        {
            _client = client;
            _interactionService = interactionService;
            _services = services;
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            _client.Ready += ReadyAsync;
            _interactionService.Log += LoggingHandler.LogAsync;

            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            foreach (var module in _interactionService.Modules)
                Console.WriteLine(module.Name);

            _client.InteractionCreated += HandleInteraction;
        }

        private async Task ReadyAsync()
        {
            if(Program.IsDebug())
                await _interactionService.RegisterCommandsToGuildAsync(ulong.Parse(_configuration["GUILD_ID"]), true);
            else
                await _interactionService.RegisterCommandsGloballyAsync(true);
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
                var context = new SocketInteractionContext(_client, interaction);

                // Execute the incoming command.
                var result = await _interactionService.ExecuteCommandAsync(context, _services);

                if (!result.IsSuccess)
                    await interaction.RespondAsync($"Sorry, something went wrong during the command execution: {result.ErrorReason}");
            }
            catch
            {
                // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (interaction.Type is InteractionType.ApplicationCommand)
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}
