using LE.Library.Kernel;
using LE.NotificationService.Hubs;
using LE.NotificationService.Services;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LE.NotificationService.Events
{
    public class PostCreatedEvent : BaseMessage
    {
        [JsonProperty("postId")]
        public Guid PostId { get; set; }

        [JsonProperty("userId")]
        public Guid UserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("notifyIds")]
        public List<Guid> NotifyIds { get; set; }

        public PostCreatedEvent() : base(MessageValue.POST_CREATED_EVENT)
        {
        }
    }

    public class PostCreatedEventHandler : IAsyncHandler<PostCreatedEvent>
    {
        private readonly INotifyService _notifyService;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        public PostCreatedEventHandler(INotifyService notifyService, IHubContext<NotificationHub> notificationHubContext)
        {
            _notifyService = notifyService;
            _notificationHubContext = notificationHubContext;
        }

        public async Task HandleAsync(IHandlerContext<PostCreatedEvent> Context, CancellationToken cancellationToken = default)
        {
            var request = Context.Request;
            foreach (var id in request.NotifyIds)
            {
                await _notificationHubContext.Clients.Group(id.ToString()).SendAsync("ReceiveMessage", $"{request.UserName} has added new post");
            }

            await _notifyService.AddToNotifyBoxAsync(request, cancellationToken);
        }
    }
}
