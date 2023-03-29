using LE.Library.Kernel;
using LE.NotificationService.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LE.NotificationService.Controllers
{
    [ApiController]
    [Route("notifications")]
    public class NotifyController : ControllerBase
    {
        private readonly IRequestHeader _requestHeader;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        public NotifyController(IHubContext<NotificationHub> notificationHubContext, IRequestHeader requestHeader)
        {
            _notificationHubContext = notificationHubContext;
            _requestHeader = requestHeader;
        }

        [HttpPost("test-send-notify-message")]
        public async Task<IActionResult> SendNotifyAsync(string message)
        {
            //await _notificationHubContext.Clients.All.SendAsync("ReceiveMessage", message);
            await _notificationHubContext.Clients.Group("123").SendAsync("ReceiveMessage", message);
            return Ok();
        }

        [HttpGet()]
        public async Task<IActionResult> GetNotiBoxAsync(CancellationToken cancellationToken)
        {
            var uuid = _requestHeader.GetOwnerId();
            if (uuid == Guid.Empty)
                return BadRequest("Require Access token");
            //await _notificationHubContext.Clients.All.SendAsync("ReceiveMessage", message);
            return Ok();
        }

    }
}
