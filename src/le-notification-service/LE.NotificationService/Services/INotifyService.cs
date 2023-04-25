using LE.NotificationService.Dtos;
using LE.NotificationService.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LE.NotificationService.Services
{
    public interface INotifyService
    {
        Task<bool> SeedDataAsync(CancellationToken cancellationToken = default);
        Task AddSupportLocaleAsync(List<string> supportLocale, CancellationToken cancellationToken = default);
        Task AddToNotifyBoxAsync(PostCreatedEvent @event, CancellationToken cancellationToken = default);
        Task AddToNotifyBoxAsync(InteractPostEvent @event, CancellationToken cancellationToken = default);
        Task AddToNotifyBoxAsync(InteractCommentEvent @event, CancellationToken cancellationToken = default);
        Task AddToNotifyBoxAsync(FriendRequestSentEvent @event, CancellationToken cancellationToken = default);
        Task AddToNotifyBoxAsync(FriendRequestAcceptedEvent @event, CancellationToken cancellationToken = default);
        Task AddToNotifyBoxAsync(CommentPostEvent @event, CancellationToken cancellationToken = default);
        Task<List<NotificationDto>> GetNotiBoxMessageAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
