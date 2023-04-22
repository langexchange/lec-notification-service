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
    public class FriendRequestAcceptedEvent : BaseMessage
    {
        [JsonProperty("fromId")]
        public Guid FromId { get; set; }

        [JsonProperty("toId")]
        public Guid ToId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("notifyIds")]
        public List<Guid> NotifyIds { get; set; }

        public FriendRequestAcceptedEvent() : base(MessageValue.FRIEND_REQUEST_ACCEPT_EVENT)
        {

        }
    }

    public class FriendRequestAcceptedEventHandler : IAsyncHandler<FriendRequestAcceptedEvent>
    {
        private readonly INotifyService _notifyService;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        public FriendRequestAcceptedEventHandler(INotifyService notifyService, IHubContext<NotificationHub> notificationHubContext)
        {
            _notifyService = notifyService;
            _notificationHubContext = notificationHubContext;
        }

        public async Task HandleAsync(IHandlerContext<FriendRequestAcceptedEvent> Context, CancellationToken cancellationToken = default)
        {
            var request = Context.Request;
            foreach (var id in request.NotifyIds)
            {
                await _notificationHubContext.Clients.Group(id.ToString()).SendAsync("ReceiveMessage", $"{request.UserName} accepted your friend request");
            }

            await _notifyService.AddToNotifyBoxAsync(request, cancellationToken);
        }
    }
}
