using Hangfire;
using LE.NotificationService.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace LE.NotificationService
{
    public class RecurringJobMiddleware
    {
        private readonly RequestDelegate _next;

        public RecurringJobMiddleware(RequestDelegate next)
        {
            _next = next;
            Subscribe();
        }

        private void Subscribe()
        {
            RecurringJob.AddOrUpdate<IScheduleService>(job => job.SendStatisticalSignalAsync(), Cron.Weekly);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            await _next.Invoke(context);
        }
    }
}
