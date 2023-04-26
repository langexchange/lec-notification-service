using LE.Library.Kernel;
using LE.Library.MessageBus;
using LE.NotificationService.Application.Events;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace LE.NotificationService.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IMessageBus _messageBus;
        private readonly IRequestHeader _requestHeader;
        private readonly ILogger<ScheduleService> _logger;

        public ScheduleService(IMessageBus messageBus, IRequestHeader requestHeader, ILogger<ScheduleService> logger)
        {
            _messageBus = messageBus;
            _requestHeader = requestHeader;
            _logger = logger;
        }

        public async Task SendStatisticalSignalAsync()
        {
            _logger.LogInformation("process signal \n");
            await _messageBus.PublishAsync(new StatisticalSignal(), _requestHeader);
        }
    }
}
