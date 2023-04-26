using AutoMapper;
using LE.NotificationService.Constant;
using LE.NotificationService.Dtos;
using LE.NotificationService.Infrastructure.Infrastructure.Entities;

namespace LE.NotificationService.AutoMappers
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<Friendnotification, NotificationDto>()
                .ForMember(d => d.Type, s => s.MapFrom(x => NotifyType.FRIEND_NOTIFY));
            CreateMap<Postnotification, NotificationDto>()
                .ForMember(d => d.Type, s => s.MapFrom(x => NotifyType.POST_NOTIFY));
            CreateMap<Commentnotification, NotificationDto>()
                .ForMember(d => d.Type, s => s.MapFrom(x => NotifyType.COMMENT_NOTIFY));
            CreateMap<Vocabpackagenotification, NotificationDto>()
                .ForMember(d => d.Type, s => s.MapFrom(x => NotifyType.VOCAB_NOTIFY));
        }
    }
}
