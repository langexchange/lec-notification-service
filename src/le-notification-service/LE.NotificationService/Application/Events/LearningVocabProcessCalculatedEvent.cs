using LE.Library.Kernel;
using LE.NotificationService.Services;
using LE.UserService.Dtos;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LE.NotificationService.Events
{
    public class LearningVocabProcessCalculatedEvent : BaseMessage
    {
        public Guid UserId { get; set; }
        public List<PracticeResultDto> Result { get; set; }

        public LearningVocabProcessCalculatedEvent() : base(MessageValue.LEARNING_PROCESS_CALCULATED_EVENT)
        {
            Result = new List<PracticeResultDto>();
        }
    }

    public class LearningVocabProcessCalculatedEventHandler : IAsyncHandler<LearningVocabProcessCalculatedEvent>
    {
        private readonly INotifyService _notifyService;

        public LearningVocabProcessCalculatedEventHandler(INotifyService notifyService)
        {
            _notifyService = notifyService;
        }

        public async Task HandleAsync(IHandlerContext<LearningVocabProcessCalculatedEvent> Context, CancellationToken cancellationToken = default)
        {
            var request = Context.Request;
            await _notifyService.AddToNotifyBoxAsync(request, cancellationToken);
        }
    }
}
