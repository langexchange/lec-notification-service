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
    public class InteractCommentEvent : BaseMessage
    {
        [JsonProperty("postId")]
        public Guid PostId { get; set; }

        [JsonProperty("commentId")]
        public Guid CommentId { get; set; }

        [JsonProperty("userId")]
        public Guid UserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("currentInteract")]
        public int CurrentInteract { get; set; }

        [JsonProperty("notifyIds")]
        public List<Guid> NotifyIds { get; set; }

        public InteractCommentEvent() : base(MessageValue.INTERACTED_COMMENT_EVENT)
        {

        }
    }

    public class InteractCommentEventHandler : IAsyncHandler<InteractCommentEvent>
    {
        private readonly INotifyService _notifyService;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        public InteractCommentEventHandler(INotifyService notifyService, IHubContext<NotificationHub> notificationHubContext)
        {
            _notifyService = notifyService;
            _notificationHubContext = notificationHubContext;
        }

        public async Task HandleAsync(IHandlerContext<InteractCommentEvent> Context, CancellationToken cancellationToken = default)
        {
            var request = Context.Request;
            foreach (var id in request.NotifyIds)
            {
                await _notificationHubContext.Clients.Group(id.ToString()).SendAsync("ReceiveMessage", $"{request.UserName} has interact your comment");
            }
            
            await _notifyService.AddToNotifyBoxAsync(request, cancellationToken);
        }
    }
}
