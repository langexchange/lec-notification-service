using System.Threading;
using System.Threading.Tasks;

namespace LE.NotificationService.Services
{
    public interface IScheduleService
    {
        Task SendStatisticalSignalAsync();
    }
}
