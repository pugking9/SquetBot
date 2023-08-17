using System;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace SquetBot.Helpers
{
    // Track class to store track information
    public class Track
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string StreamUrl { get; set; }

        public Track(string title, string url, string streamUrl)
        {
            Title = title;
            Url = url;
            StreamUrl = streamUrl;
        }
    }
    internal class MusicHelper
    {
        //Determine if url or search term
        public static bool IsUrl(string input)
        {
            return Uri.TryCreate(input, UriKind.Absolute, out Uri? uriResult)
                && (uriResult?.Scheme == Uri.UriSchemeHttp || uriResult?.Scheme == Uri.UriSchemeHttps);
        }

        //Determine which platform the url is from, return stream url
        /*public static string GetStreamUrl(string input)
        {
            
        }*/

        //Search for track on youtube, return stream url
        public static async Task<string?> SearchYoutubeAsync(string input)
        {
            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(input);

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            if (streamInfo == null)
            {
                return null;
            }
            return streamInfo.Url;
        }
    }
}
