using LE.NotificationService.Events;
using System.Threading;
using System.Threading.Tasks;

namespace LE.NotificationService.Services
{
    public interface IStatisticLearningService
    {
        Task CreateOrUpdateStatisticLearningProcessAsync(LearningVocabProcessCalculatedEvent @event, CancellationToken cancellationToken = default);
    }
}
