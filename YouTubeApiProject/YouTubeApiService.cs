using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using YouTubeApiProject.Models;

namespace YouTubeApiProject.Services
{
    public class YouTubeApiService
    {
        private readonly string _apiKey;

        public YouTubeApiService(IConfiguration configuration)
        {
            _apiKey = configuration["YouTubeApiKey"]; // Fetch API key from appsettings.json
        }

        public async Task<List<YouTubeVideoModel>> SearchVideosAsync(string query)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _apiKey,
                ApplicationName = "YoutubeProject"
            });

            // Search videos
            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = query; // User's query from form input
            searchRequest.MaxResults = 20;
            var searchResponse = await searchRequest.ExecuteAsync();

            // Prepare list of videos
            var videos = new List<YouTubeVideoModel>();

            foreach (var searchResult in searchResponse.Items)
            {
                if (searchResult.Id.Kind == "youtube#video")
                {
                    var channelId = searchResult.Snippet.ChannelId;
                    var channelName = searchResult.Snippet.ChannelTitle;

                    // Fetch channel details to get profile picture
                    var channelRequest = youtubeService.Channels.List("snippet");
                    channelRequest.Id = channelId;
                    var channelResponse = await channelRequest.ExecuteAsync();
                    var channelProfilePictureUrl = channelResponse.Items[0].Snippet.Thumbnails.Default__.Url;

                    // Add video details to list
                    videos.Add(new YouTubeVideoModel
                    {
                        Title = searchResult.Snippet.Title,
                        Description = searchResult.Snippet.Description,
                        ThumbnailUrl = searchResult.Snippet.Thumbnails.Medium.Url,
                        ChannelName = channelName, // Channel name from search result
                        ChannelProfilePictureUrl = channelProfilePictureUrl,// Channel profile picture
                        VideoUrl = "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId // Add this line

                    });
                }
            }

            return videos;
        }
    }
}
