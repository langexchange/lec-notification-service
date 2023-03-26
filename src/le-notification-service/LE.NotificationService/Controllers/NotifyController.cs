using LE.NotificationService.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace LE.NotificationService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotifyController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        public NotifyController(IHubContext<NotificationHub> notificationHubContext)
        {
            _notificationHubContext = notificationHubContext;
        }

        [HttpPost()]
        public async Task<IActionResult> SendNotifyAsync(string message)
        {
            //await _notificationHubContext.Clients.All.SendAsync("ReceiveMessage", message);
            await _notificationHubContext.Clients.Group("123").SendAsync("ReceiveMessage", message);
            return Ok();
        }
    }
}
