using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using SquetBot.Helpers;

namespace SquetBot.Modules
{
    
    public class VoiceInteractions : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }
        public MusicHandler MusicHandler { get; set; }

        [SlashCommand("join", "Join voice channel", false, RunMode.Async)]
        public async Task Join()
        {
            if(!MusicHandler._queues.ContainsKey(Context.Guild.Id))
            {
                await MusicHandler.JoinVoiceChannel(Context.User as SocketGuildUser);
            }
            else
            {
                Queue guild = MusicHandler._queues[Context.Guild.Id] as Queue;
                if (!guild._playing)
                {
                    await MusicHandler.JoinVoiceChannel(Context.User as SocketGuildUser);
                }
                else
                {
                    await RespondAsync("I'm already in a voice channel", ephemeral: true);
                }
            }
        }

        [SlashCommand("skip","Skip the current song", false, RunMode.Async)]
        public async Task Skip()
        {
            if (MusicHandler._queues.ContainsKey(Context.Guild.Id))
            {
                Queue guild = MusicHandler._queues[Context.Guild.Id] as Queue;
                if (guild._playing)
                {
                    _ = MusicHandler.SkipTrack(Context.Guild.Id);
                    await RespondAsync("Skipped track", ephemeral: true);
                }
                else
                {
                    await RespondAsync("I'm not in a voice channel", ephemeral: true);
                }
            }
            else
            {
                await RespondAsync("I'm not in a voice channel", ephemeral: true);
            }
        }

        [SlashCommand("leave", "Leave voice channel", false, RunMode.Async)]
        public async Task Leave()
        {
            if (MusicHandler._queues.ContainsKey(Context.Guild.Id))
            {
                Queue guild = MusicHandler._queues[Context.Guild.Id] as Queue;
                if (guild._playing)
                {
                    await MusicHandler.LeaveVoiceChannel(Context.Guild.Id);
                    await RespondAsync("Left voice channel", ephemeral: true);
                }
                else
                {
                    await RespondAsync("I'm not in a voice channel", ephemeral: true);
                }
            }
            else
            {
                await RespondAsync("I'm not in a voice channel", ephemeral: true);
            }
        }

        [SlashCommand("play", "Play audio from youtube in voice channel")]
        public async Task Play([Summary(description:"Search term or youtube link")] string input)
        {
            var voiceChannel = (Context.User as SocketGuildUser)?.VoiceChannel;
            var Queue = MusicHandler._queues[Context.Guild.Id] as Queue;
            if (Queue == null)
            {
                await MusicHandler.JoinVoiceChannel(Context.User as SocketGuildUser);
                Queue = MusicHandler._queues[Context.Guild.Id] as Queue;
            }
            else if (voiceChannel == null)
            {
                await RespondAsync("You must be in a voice channel to use this command", ephemeral: true);
                return;
            }
            else if (voiceChannel != Context.Guild.CurrentUser.VoiceChannel && Queue._playing)
            {
                await RespondAsync("You must be in the same voice channel as me to use this command", ephemeral: true);
                return;
            }
            else if (!Queue._playing)
            {
                await MusicHandler.JoinVoiceChannel(Context.User as SocketGuildUser);
            }

            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(input);

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            if (streamInfo == null)
            {
                await RespondAsync("This video has no audio");
                return;
            }
            Queue.AddTrack(new Track(video.Title, input, streamInfo.Url));

            await RespondAsync($"Queued {video.Title}");
        }

        [SlashCommand("queue", "Show the current queue")]
        public async Task Queue()
        {
            var Queue = MusicHandler._queues[Context.Guild.Id] as Queue;
            if (Queue == null)
            {
                await RespondAsync("I'm not in a voice channel", ephemeral: true);
            }
            else if (Queue._playing)
            {
                //build queue string
                string queueString = "";
                foreach (Track track in Queue._queue)
                {
                    queueString += $"{track.Title}\n{track.Url}\n\n";
                }
                await RespondAsync(queueString);
            }
            else
            {
                await RespondAsync("I'm not in a voice channel", ephemeral: true);
            }
        }

    }
}
