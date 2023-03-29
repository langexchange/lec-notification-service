using AutoMapper;
using LE.NotificationService.Infrastructure.Infrastructure;

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
    }
}
