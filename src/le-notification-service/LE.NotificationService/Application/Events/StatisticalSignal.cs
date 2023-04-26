using LE.Library.Kernel;
using LE.NotificationService.Services;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace LE.NotificationService.Application.Events
{
    public class StatisticalSignal : BaseMessage
    {
        public StatisticalSignal() : base(MessageValue.STATISTICAL_SIGNAL)
        {

        }
    }
}
