using AutoMapper;
using LE.NotificationService.Infrastructure.Infrastructure;
using LE.NotificationService.Infrastructure.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LE.NotificationService.Services
{
    public class NotifyService : INotifyService
    {
        private LanggeneralDbContext _context;
        private readonly IMapper _mapper;
        public NotifyService(LanggeneralDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> SeedDataAsync(CancellationToken cancellationToken = default)
        {
            var setting = await _context.Settings.FirstOrDefaultAsync();
            if (setting != null)
                return false;
            var filename = "Jsonfiles/settingnotification.json";
            var text = File.ReadAllText(filename);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, NotifySetting>>(text);
            
            var localeSettings = dictionary.SelectMany(x =>
            {
                var values = x.Value.NotifyService;
                return values.Select(y =>
                {
                    return new KeyValuePair<string, string>($"{x.Key}.notify-service.{y.Key}", y.Value);
                }).ToDictionary(z => z.Key, z => z.Value);
            }).ToDictionary(pair => pair.Key, pair => pair.Value);

            var settings = localeSettings.Select(x => 
                                                new Setting { Id = System.Guid.NewGuid(), ServiceName = "notify-service", SettingKey = x.Key, SettingValue = x.Value }
                                                );
            await _context.Settings.AddRangeAsync(settings);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
