using AutoMapper;
using LE.NotificationService.Dtos;
using LE.NotificationService.Infrastructure.Infrastructure.Entities;

namespace LE.NotificationService.AutoMappers
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<Friendnotification, NotificationDto>()
                .ForMember(d => d.Type, s => s.MapFrom(x => "friend notify"));
            CreateMap<Postnotification, NotificationDto>()
                .ForMember(d => d.Type, s => s.MapFrom(x => "post notify"));
            CreateMap<Commentnotification, NotificationDto>()
                .ForMember(d => d.Type, s => s.MapFrom(x => "comment notify"));
            CreateMap<Vocabpackagenotification, NotificationDto>()
                .ForMember(d => d.Type, s => s.MapFrom(x => "vocab notify"));
        }
    }
}
