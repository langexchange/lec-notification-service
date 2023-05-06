using LE.NotificationService.Events;
using LE.NotificationService.Infrastructure.Infrastructure;
using LE.NotificationService.Infrastructure.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LE.NotificationService.Services
{
    public class StatisticLearningService : IStatisticLearningService
    {
        private readonly LanggeneralDbContext _context;

        public StatisticLearningService(LanggeneralDbContext context)
        {
            _context = context;
        }

        public async Task CreateOrUpdateStatisticLearningProcessAsync(LearningVocabProcessCalculatedEvent @event, CancellationToken cancellationToken = default)
        {
            var statistic = await _context.Statisticallearningprocesses.Where(x => x.Userid == @event.UserId
                                                                                   && x.UpdatedAt.Value.Month == DateTime.UtcNow.Month)
                                                                       .FirstOrDefaultAsync();

            var packageIds = @event.Result.Select(x => x.PackageId).ToList();
            var totalVocabs = @event.Result.Select(x => x.TotalVocabs).ToList().Sum();
            var currentVocab = @event.Result.Select(x => x.CurrentNumOfVocab).ToList().Sum();
            var percentProcess = 100;

            if (statistic != null)
            {
                var currentPacageIds = JsonConvert.DeserializeObject<List<Guid>>(statistic.Packageids);
                var newVocabPackages = @event.Result.Where(x => !currentPacageIds.Any(y => y == x.PackageId));

                if (newVocabPackages.Any())
                {
                    currentPacageIds.AddRange(newVocabPackages.Select(x => x.PackageId).ToList());
                    statistic.Packageids = JsonConvert.SerializeObject(currentPacageIds);
                    statistic.Totalvocabs += newVocabPackages.Select(x => x.TotalVocabs).ToList().Sum();
                    statistic.Currentvocabs += newVocabPackages.Select(x => x.CurrentNumOfVocab).ToList().Sum();
                }
                statistic.Currentvocabs = currentVocab;

                if (statistic.Totalvocabs != 0)
                    percentProcess = (int)(1.0 - (double)statistic.Currentvocabs / (double)statistic.Totalvocabs) * 100;

                statistic.Percent = percentProcess;
                statistic.UpdatedAt = DateTime.UtcNow;
                _context.Update(statistic);
                await _context.SaveChangesAsync();
                return;
            }

            if(totalVocabs != 0)
                percentProcess = (int)(1.0 - (double)currentVocab / (double)totalVocabs) * 100;

            var newStatistic = new Statisticallearningprocess
            {
                Id = Guid.NewGuid(),
                Userid = @event.UserId,
                Packageids = JsonConvert.SerializeObject(packageIds),
                Totalvocabs = totalVocabs,
                Currentvocabs = currentVocab,
                Percent = percentProcess
            };
            _context.Statisticallearningprocesses.Add(newStatistic);
            await _context.SaveChangesAsync();
        }
    }
}
