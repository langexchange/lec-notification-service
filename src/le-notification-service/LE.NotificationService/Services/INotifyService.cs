using System.Threading;
using System.Threading.Tasks;

namespace LE.NotificationService.Services
{
    public interface INotifyService
    {
        Task<bool> SeedDataAsync(CancellationToken cancellationToken = default);
    }
}
