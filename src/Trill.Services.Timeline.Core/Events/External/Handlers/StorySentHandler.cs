using System.Linq;
using System.Threading.Tasks;
using Convey.CQRS.Events;
using Microsoft.Extensions.Logging;
using Trill.Services.Timeline.Core.Data;

namespace Trill.Services.Timeline.Core.Events.External.Handlers
{
    internal sealed class StorySentHandler : IEventHandler<StorySent>
    {
        private readonly IStorage _storage;
        private readonly ILogger<StorySentHandler> _logger;

        public StorySentHandler(IStorage storage, ILogger<StorySentHandler> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public async Task HandleAsync(StorySent @event)
        {
            var story = new Story
            {
                Id = @event.StoryId,
                Title = @event.Title,
                Author = new Author
                {
                    Id = @event.Author.Id,
                    Name = @event.Author.Name
                },
                CreatedAt = @event.CreatedAt,
                Tags = @event.Tags,
                Visibility = new Visibility
                {
                    From = @event.Visibility.From,
                    To = @event.Visibility.To,
                    Highlighted = @event.Visibility.Highlighted
                }
            };

            await _storage.AddStoryAsync(story);
            _logger.LogInformation($"Added a story with ID: '{@event.StoryId}'.");
            var followers = await _storage.GetAllFollowersAsync(@event.Author.Id);
            if (!followers.Any())
            {
                _logger.LogInformation($"No followers of author: '{@event.Author.Id}' have been found to add a story: '{@event.StoryId}'.");
                return;
            }
            
            foreach (var follower in followers)
            {
                await _storage.AddStoryToTimelineAsync(follower, story);
            }
            
            _logger.LogInformation($"Pushed a story with ID: '{@event.StoryId}' to {followers.Count} timelines.");
        }
    }
}