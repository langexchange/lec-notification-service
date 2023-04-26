using LE.Library.Kernel;

namespace LE.NotificationService.Application.Events
{
    public class StatisticalSignal : BaseMessage
    {
        public StatisticalSignal() : base(MessageValue.STATISTICAL_SIGNAL)
        {

        }
    }
}
