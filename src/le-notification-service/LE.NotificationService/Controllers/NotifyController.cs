using LE.Library.Kernel;
using LE.NotificationService.Hubs;
using LE.NotificationService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LE.NotificationService.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotifyController : ControllerBase
    {
        private readonly IRequestHeader _requestHeader;
        private readonly INotifyService _notifyService;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        public NotifyController(IHubContext<NotificationHub> notificationHubContext, IRequestHeader requestHeader, INotifyService notifyService)
        {
            _notificationHubContext = notificationHubContext;
            _requestHeader = requestHeader;
            _notifyService = notifyService;
        }

        [HttpPost("test-send-notify-message")]
        public async Task<IActionResult> SendNotifyAsync(Guid id, string message)
        {
            //await _notificationHubContext.Clients.All.SendAsync("ReceiveMessage", message);
            await _notificationHubContext.Clients.Group(id.ToString()).SendAsync("ReceiveMessage", message);
            return Ok();
        }

        [HttpGet()]
        public async Task<IActionResult> GetNotiBoxAsync(CancellationToken cancellationToken)
        {
            var uuid = _requestHeader.GetOwnerId();
            if (uuid == Guid.Empty)
                return BadRequest("Require Access token");

            var dtos = await _notifyService.GetNotiBoxMessageAsync(uuid, cancellationToken);
            return Ok(dtos);
        }

        [HttpPost("seed-data")]
        public async Task<IActionResult> SeedDataAsync(CancellationToken cancellationToken)
        {
            await _notifyService.SeedDataAsync(cancellationToken);
            return Ok();
        }

        [HttpPost("api/notifications/settings/support-locale")]
        public async Task<IActionResult> AddSettingSupportLocaleAsync(List<string> locale, CancellationToken cancellationToken)
        {
            await _notifyService.AddSupportLocaleAsync(locale, cancellationToken);
            return Ok();
        }
    }
}
