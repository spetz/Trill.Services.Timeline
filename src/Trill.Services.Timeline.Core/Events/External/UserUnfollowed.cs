using System;
using Convey.CQRS.Events;
using Convey.MessageBrokers;

namespace Trill.Services.Timeline.Core.Events.External
{
    [Message("users")]
    public class UserUnfollowed : IEvent
    {
        public Guid FollowerId { get; }
        public Guid FolloweeId { get; }

        public UserUnfollowed(Guid followerId, Guid followeeId)
        {
            FollowerId = followerId;
            FolloweeId = followeeId;
        }
    }
}