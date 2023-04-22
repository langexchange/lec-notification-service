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
    public class CommentPostEvent : BaseMessage
    {
        [JsonProperty("postId")]
        public Guid PostId { get; set; }

        [JsonProperty("commentId")]
        public Guid CommentId { get; set; }

        [JsonProperty("userId")]
        public Guid UserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("currentComment")]
        public int CurrentComment { get; set; }

        [JsonProperty("notifyIds")]
        public List<Guid> NotifyIds { get; set; }

        public CommentPostEvent() : base(MessageValue.COMMENTED_POST_EVENT)
        {
        }
    }

    public class CommentPostEventHandler : IAsyncHandler<CommentPostEvent>
    {
        private readonly INotifyService _notifyService;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        public CommentPostEventHandler(INotifyService notifyService, IHubContext<NotificationHub> notificationHubContext)
        {
            _notifyService = notifyService;
            _notificationHubContext = notificationHubContext;
        }

        public async Task HandleAsync(IHandlerContext<CommentPostEvent> Context, CancellationToken cancellationToken = default)
        {
            var request = Context.Request;

            foreach (var id in request.NotifyIds)
            {
                await _notificationHubContext.Clients.Group(id.ToString()).SendAsync("ReceiveMessage", $"{request.UserName} has comment on post you commented before");
            }

            await _notifyService.AddToNotifyBoxAsync(request, cancellationToken);
        }
    }
}
