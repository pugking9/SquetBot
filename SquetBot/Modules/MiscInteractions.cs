using Discord.WebSocket;
using Discord.Interactions;
using System.Threading.Tasks;

namespace SquetBot.Modules
{
    public class MiscInteractions : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }

        [SlashCommand("test-command", "This is a test command")]
        public async Task Test()
        {
            await RespondAsync("This is a test command");
        }

        [SlashCommand("name", "Respond with your name")]
        public async Task Name()
        {
            var name = (Context.User as SocketGuildUser)?.Nickname ?? Context.User.Username;
            await RespondAsync($"Your name is {name}");
        }
    }
}
