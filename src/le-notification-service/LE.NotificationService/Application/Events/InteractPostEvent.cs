using LE.Library.Kernel;
using LE.Library.MessageBus;
using LE.NotificationService.Hubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LE.NotificationService.Events
{
    public class InteractPostEvent : BaseMessage, IMessage
    {
        [JsonProperty("postId")]
        public Guid PostId { get; set; }

        [JsonProperty("interactType")]
        public string InteractType { get; set; }

        [JsonProperty("userId")]
        public Guid UserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("currentInteract")]
        public int CurrentInteract { get; set; }

        [JsonProperty("notifyIds")]
        public List<Guid> NotifyIds { get; set; }

        public InteractPostEvent(): base(MessageValue.INTERACTED_POST_EVENT)
        {
        }
    }

    public class InteractPostEventHandler : IAsyncHandler<InteractPostEvent>
    {
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        public InteractPostEventHandler(IHubContext<NotificationHub> notificationHubContext)
        {
            _notificationHubContext = notificationHubContext;
        }

        public async Task HandleAsync(IHandlerContext<InteractPostEvent> Context, CancellationToken Token = default)
        {
            var request = Context.Request;
            foreach(var id in request.NotifyIds)
            {
                await _notificationHubContext.Clients.Group(id.ToString()).SendAsync("ReceiveMessage", $"{request.UserName} has interact your post");
            }
        }
    }
}
