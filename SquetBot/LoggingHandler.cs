using Discord.Commands;
using Discord;
using System;
using System.Threading.Tasks;

namespace SquetBot
{
    internal class LoggingHandler
    {
        public static async Task LogAsync(LogMessage msg)
        {
            if (msg.Exception is CommandException cmdException)
            {
                Console.WriteLine($"[Command/{msg.Severity}] {cmdException.Command.Aliases[0]}"
                                   + $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine(cmdException);
            }
            else
            {
                Console.WriteLine($"[General/{msg.Severity}] {msg}");
            }
        }
    }
}
