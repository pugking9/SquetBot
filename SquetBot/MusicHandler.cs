using Discord.Audio;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Collections;
using SquetBot.Helpers;

namespace SquetBot
{
    public class Queue
    {
        public bool _playing { get; set; }
        public AudioOutStream? _stream { get; set; }
        public int _index { get; set; }
        public List<Track> _queue { get; set; }
        public IAudioClient? _audioClient { get; set; }
        public string? _channelId { get; set; }
        public Queue()
        {
            _queue = new List<Track>();
            _playing = false;
            _index = 0;
        }

        public async Task AddTrack(Track track)
        {
            _queue.Add(track);
            if(!_playing)
            {
                _playing = true;
                await PlayAsync(track.StreamUrl);
            }
        }

        public void RemoveTrack(int index)
        {
            _queue.RemoveAt(index);
        }

        public async Task SkipTrack()
        {
            if (_playing)
            {
                // Stop the current track playback
                _stream?.Dispose();
                _stream = null;

                // Move to the next track in the queue
                _index++;

                if (_index < _queue.Count)
                {
                    await PlayLoop(_queue[_index]);
                }
                else
                {
                    _playing = false;
                }
            }
        }

        private Process CreateStream(string path)
        {
            // Populate process info, ignore errors and redirect output
            string args = $"-reconnect 1 -err_detect ignore_err -hide_banner -loglevel panic -i {path} -ac 2 -f s16le -ar 48000 pipe:1";
            var processInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            var process = new Process { StartInfo = processInfo };

            process.Start();

            return process;
        }

        private async Task PlayLoop(Track track)
        {
            await PlayAsync(track.StreamUrl);
        }

        private async Task PlayAsync(string streamUrl)
        {
            await PlayStreamAsync(streamUrl);
            _index++;
            if(_index < _queue.Count)
            {
                await PlayLoop(_queue[_index]);
            }
            else
            {
                _playing = false;
            }
        }

        private async Task PlayStreamAsync(string streamUrl)
        {
            using var ffmpeg = CreateStream(streamUrl);
            using var output = ffmpeg.StandardOutput.BaseStream;

            if(_stream == null)
                _stream = _audioClient?.CreatePCMStream(AudioApplication.Music);

            await output.CopyToAsync(_stream);
            await _stream.FlushAsync();
        }
    }
    public class MusicHandler
    {
        public Hashtable _queues { get; }
        public MusicHandler()
        {
            _queues = new Hashtable();
        }

        public async Task AddTrack(ulong guildId, Track track)
        {
            if(_queues.ContainsKey(guildId))
            {
                var queue = _queues[guildId] as Queue;
                await queue.AddTrack(track);
            }
            else
            {
                var queue = new Queue();
                _queues.Add(guildId, queue);
                await queue.AddTrack(track);
            }
        }

        public void RemoveTrack(ulong guildId, int index)
        {
            if(_queues.ContainsKey(guildId))
            {
                var queue = _queues[guildId] as Queue;
                queue.RemoveTrack(index);
            }
        }

        public async Task JoinVoiceChannel(SocketGuildUser user)
        {
            if(_queues.ContainsKey(user.Guild.Id))
            {
                var queue = _queues[user.Guild.Id] as Queue;
                if(queue._audioClient == null)
                {
                    queue._audioClient = await user.VoiceChannel.ConnectAsync();
                }
            }
            else
            {
                var queue = new Queue();
                queue._audioClient = await user.VoiceChannel.ConnectAsync();
                _queues.Add(user.Guild.Id, queue);
            }
        }

        public async Task LeaveVoiceChannel(ulong guildId)
        {
            if(_queues.ContainsKey(guildId))
            {
                var queue = _queues[guildId] as Queue;
                if(queue._audioClient != null)
                {
                    await queue._audioClient.StopAsync();
                    queue._audioClient = null;
                }
                _queues.Remove(guildId);
            }
        }

        public async Task SkipTrack(ulong guildId)
        {
            if(_queues.ContainsKey(guildId))
            {
                var queue = _queues[guildId] as Queue;
                await queue.SkipTrack();
            }
        }
    }
}
