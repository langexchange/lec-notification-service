using AutoMapper;
using LE.NotificationService.Constant;
using LE.NotificationService.Events;
using LE.NotificationService.Infrastructure.Infrastructure;
using LE.NotificationService.Infrastructure.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
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

        public async Task AddSupportLocaleAsync(List<string> locale, CancellationToken cancellationToken = default)
        {
            var supportedLocaleSetting = await _context.Settings.FirstOrDefaultAsync(x => x.SettingKey == NotifyKey.SUPPORT_LOCALE);
            if(supportedLocaleSetting == null)
            {
                _context.Add(new Setting
                {
                    Id = System.Guid.NewGuid(),
                    ServiceName = "notify-service",
                    SettingKey = NotifyKey.SUPPORT_LOCALE,
                    SettingValue = JsonConvert.SerializeObject(locale),
                });
            }
            else
            {
                var supportLocale = JsonConvert.DeserializeObject<List<string>>(supportedLocaleSetting.SettingValue);
                locale.ForEach(x => x.ToLower());
                supportLocale.AddRange(locale);
                supportLocale = supportLocale.Distinct().ToList();

                supportedLocaleSetting.SettingValue = JsonConvert.SerializeObject(supportLocale);
                _context.Update(supportedLocaleSetting);
            }

            await _context.SaveChangesAsync();
        }

        private async Task<string> GetUserNotifyLocale(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Userid == userId);
            var nativeLocale = await _context.Languages.FirstOrDefaultAsync(x => x.Langid == user.NativeLang);
            var targetLang = await _context.Targetlangs.Where(x => x.Userid == user.Userid).Select(x => x.Langid).ToListAsync();
            var targetLangLocale = await _context.Languages.Where(x => targetLang.Contains(x.Langid)).ToListAsync();

            var supportedLocaleSetting = await _context.Settings.FirstOrDefaultAsync(x => x.SettingKey == NotifyKey.SUPPORT_LOCALE);
            var supportedLocale = JsonConvert.DeserializeObject<List<string>>(supportedLocaleSetting.SettingValue);
            if (supportedLocale.Contains(nativeLocale.LocaleCode))
            {
                return nativeLocale.LocaleCode;
            }
            else
            {
                foreach (var item in targetLangLocale)
                    if (supportedLocale.Contains(item.LocaleCode))
                    {
                        return item.LocaleCode;
                    }
            }

            return "vi";
        }
        private async Task<Guid> CreateUserNotiBoxAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Userid == id);
            if (user == null)
                return Guid.Empty;
            if (!user.Notibox.HasValue)
            {
                var userNotiBox = new Notibox
                {
                    Boxid = Guid.NewGuid(),
                    Userid = id
                };
                _context.Notiboxes.Add(userNotiBox);
                user.Notibox = userNotiBox.Boxid;
                _context.Update(user);
            }
            await _context.SaveChangesAsync();

            return user.Notibox.HasValue? user.Notibox.Value: Guid.Empty;
        }

        public async Task AddToNotifyBoxAsync(PostCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            var sharingNoti = new Sharingnotification
            {
                Notiid = Guid.NewGuid(),
                NotifiKey = $"{NotifyKey.DEFAULT_SUPPORT_LOCALE}.{NotifyKey.CREATE_POST_NOTI}",
                NotifyData = JsonConvert.SerializeObject(new {@event.UserName, @event.UserId}),
                Postid = @event.PostId
            };
            _context.Sharingnotifications.Add(sharingNoti);
            await _context.SaveChangesAsync();

            foreach (var id in @event.NotifyIds)
            {
                var notiBoxId = await CreateUserNotiBoxAsync(id, cancellationToken);
                if (notiBoxId == Guid.Empty)
                    continue;
                var newNotiBoxSharing = new Notiboxsharing
                {
                    Boxid = notiBoxId,
                    Notiid = sharingNoti.Notiid
                };
                _context.Add(newNotiBoxSharing);
            }
            await _context.SaveChangesAsync();
        }

        public async Task AddToNotifyBoxAsync(InteractPostEvent @event, CancellationToken cancellationToken = default)
        {
            foreach (var id in @event.NotifyIds)
            {
                var notiBoxId = await CreateUserNotiBoxAsync(id, cancellationToken);
                if (notiBoxId == Guid.Empty)
                    continue;
                var postNotify = await _context.Postnotifications.FirstOrDefaultAsync(x => x.Postid == @event.PostId);
                if(postNotify == null)
                {
                    var postNoti = new Postnotification
                    {
                        Boxid = notiBoxId,
                        Notiid = Guid.NewGuid(),
                        NotifiKey = $"{NotifyKey.DEFAULT_SUPPORT_LOCALE}.{NotifyKey.FIRST_INTERACT_POST_OWNER_NOTI}",
                        NotifyData = JsonConvert.SerializeObject(new { @event.UserName, @event.UserId, @event.InteractType, @event.CurrentInteract }),
                        Postid = @event.PostId,
                        Type = NotifyPostAction.INTERACT
                    };
                    _context.Postnotifications.Add(postNoti);
                }
                else
                {
                    postNotify.NotifiKey = $"{NotifyKey.DEFAULT_SUPPORT_LOCALE}.{NotifyKey.COMMENT_POST_OWNER_NOTI}";
                    postNotify.NotifyData = JsonConvert.SerializeObject(new { @event.UserName, @event.UserId, @event.InteractType, @event.CurrentInteract });
                    postNotify.UpdatedAt = DateTime.UtcNow;
                    _context.Postnotifications.Update(postNotify);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task AddToNotifyBoxAsync(InteractCommentEvent @event, CancellationToken cancellationToken = default)
        {
            foreach (var id in @event.NotifyIds)
            {
                var notiBoxId = await CreateUserNotiBoxAsync(id, cancellationToken);
                if (notiBoxId == Guid.Empty)
                    continue;
                var commentNotify = await _context.Commentnotifications.FirstOrDefaultAsync(x => x.Commentid == @event.CommentId);
                if (commentNotify == null)
                {
                    var postNoti = new Commentnotification
                    {
                        Boxid = notiBoxId,
                        Notiid = Guid.NewGuid(),
                        NotifiKey = $"{NotifyKey.DEFAULT_SUPPORT_LOCALE}.{NotifyKey.FIRST_INTERACT_COMMENT_OWNER_NOTI}",
                        NotifyData = JsonConvert.SerializeObject(new { @event.UserName, @event.UserId, @event.CurrentInteract }),
                        Commentid = @event.CommentId
                    };
                    _context.Commentnotifications.Add(postNoti);
                }
                else
                {
                    commentNotify.NotifiKey = $"{NotifyKey.DEFAULT_SUPPORT_LOCALE}.{NotifyKey.INTERACT_COMMENT_OWNER_NOTI}";
                    commentNotify.NotifyData = JsonConvert.SerializeObject(new { @event.UserName, @event.UserId, @event.CurrentInteract });
                    commentNotify.UpdatedAt = DateTime.UtcNow;
                    _context.Commentnotifications.Update(commentNotify);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task AddToNotifyBoxAsync(FriendRequestSentEvent @event, CancellationToken cancellationToken = default)
        {
            foreach (var id in @event.NotifyIds)
            {
                var notiBoxId = await CreateUserNotiBoxAsync(id, cancellationToken);
                if (notiBoxId == Guid.Empty)
                    continue;
                var friendNotify = await _context.Friendnotifications.FirstOrDefaultAsync(x => x.Boxid == notiBoxId && x.NotifyData.Contains(@event.FromId.ToString())
                                                                                               && x.NotifyData.Contains(@event.ToId.ToString()));
                if (friendNotify == null)
                {
                    var friendRequestNoti = new Friendnotification
                    {
                        Boxid = notiBoxId,
                        Notiid = Guid.NewGuid(),
                        NotifiKey = $"{NotifyKey.DEFAULT_SUPPORT_LOCALE}.{NotifyKey.FRIEND_REQUEST_SENT}",
                        NotifyData = JsonConvert.SerializeObject(new { @event.UserName, @event.FromId, @event.ToId })
                    };
                    _context.Friendnotifications.Add(friendRequestNoti);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task AddToNotifyBoxAsync(FriendRequestAcceptedEvent @event, CancellationToken cancellationToken = default)
        {
            foreach (var id in @event.NotifyIds)
            {
                var notiBoxId = await CreateUserNotiBoxAsync(id, cancellationToken);
                if (notiBoxId == Guid.Empty)
                    continue;
                var friendNotify = await _context.Friendnotifications.FirstOrDefaultAsync(x => x.Boxid == notiBoxId && x.NotifyData.Contains(@event.FromId.ToString())
                                                                                               && x.NotifyData.Contains(@event.ToId.ToString()));
                if (friendNotify == null)
                {
                    var friendRequestNoti = new Friendnotification
                    {
                        Boxid = notiBoxId,
                        Notiid = Guid.NewGuid(),
                        NotifiKey = $"{NotifyKey.DEFAULT_SUPPORT_LOCALE}.{NotifyKey.FRIEND_REQUEST_ACCEPTED}",
                        NotifyData = JsonConvert.SerializeObject(new { @event.UserName, @event.FromId, @event.ToId})
                    };
                    _context.Friendnotifications.Add(friendRequestNoti);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task AddToNotifyBoxAsync(CommentPostEvent @event, CancellationToken cancellationToken = default)
        {
            var sharingNotiEntity = await _context.Sharingnotifications.FirstOrDefaultAsync(x => x.Postid == @event.PostId 
                                                                                           && x.NotifiKey.Contains(NotifyKey.COMMENT_POST_OWNER_NOTI));

            var newSharingNotiId = Guid.NewGuid();
            if(sharingNotiEntity == null)
            {
                var sharingNoti = new Sharingnotification
                {
                    Notiid = newSharingNotiId,
                    NotifiKey = $"{NotifyKey.DEFAULT_SUPPORT_LOCALE}.{NotifyKey.COMMENT_POST_SHARING_NOTI}",
                    NotifyData = JsonConvert.SerializeObject(new { @event.UserName, @event.UserId, @event.CurrentComment }),
                    Postid = @event.PostId
                };
                _context.Sharingnotifications.Add(sharingNoti);
                await _context.SaveChangesAsync();
            }
            else
            {
                sharingNotiEntity.NotifyData = JsonConvert.SerializeObject(new { @event.UserName, @event.UserId, @event.CurrentComment });
                sharingNotiEntity.UpdatedAt = DateTime.UtcNow;
                _context.Sharingnotifications.Update(sharingNotiEntity);
            }
           

            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Postid == @event.PostId);
            if (post == null)
                return;

            @event.NotifyIds.Remove(post.Userid.Value);
            //add notiboxowner
            var ownerNotiBoxId = await CreateUserNotiBoxAsync(post.Userid.Value, cancellationToken);
            var postNotify = await _context.Postnotifications.FirstOrDefaultAsync(x => x.Postid == @event.PostId && x.Type == NotifyPostAction.COMMENT);
            if (postNotify == null)
            {
                var postNoti = new Postnotification
                {
                    Boxid = ownerNotiBoxId,
                    Notiid = Guid.NewGuid(),
                    NotifiKey = $"{NotifyKey.DEFAULT_SUPPORT_LOCALE}.{NotifyKey.FIRST_COMMENT_POST_OWNER_NOTI}",
                    NotifyData = JsonConvert.SerializeObject(new { @event.UserName, @event.UserId, @event.CurrentComment }),
                    Postid = @event.PostId,
                    Type = NotifyPostAction.COMMENT
                };
                _context.Postnotifications.Add(postNoti);
            }
            else
            {
                postNotify.NotifiKey = $"{NotifyKey.DEFAULT_SUPPORT_LOCALE}.{NotifyKey.COMMENT_POST_OWNER_NOTI}";
                postNotify.NotifyData = JsonConvert.SerializeObject(new { @event.UserName, @event.UserId, @event.CurrentComment });
                postNotify.UpdatedAt = DateTime.UtcNow;
                _context.Postnotifications.Update(postNotify);
            }
            //add notiboxsharing
            foreach (var id in @event.NotifyIds)
            {
                var notiBoxId = await CreateUserNotiBoxAsync(id, cancellationToken);
                if (notiBoxId == Guid.Empty)
                    continue;
                var newNotiBoxSharing = new Notiboxsharing
                {
                    Boxid = notiBoxId,
                    Notiid = sharingNotiEntity == null? newSharingNotiId: sharingNotiEntity.Notiid
                };
                _context.Add(newNotiBoxSharing);
            }
            await _context.SaveChangesAsync();
        }
    }
}
