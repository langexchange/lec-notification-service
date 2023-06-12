using AutoMapper;
using LE.NotificationService.Constant;
using LE.NotificationService.Dtos;
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

            return NotifyKey.DEFAULT_SUPPORT_LOCALE;
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

            return user.Notibox.HasValue ? user.Notibox.Value : Guid.Empty;
        }

        public async Task AddToNotifyBoxAsync(PostCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            var sharingNoti = new Sharingnotification
            {
                Notiid = Guid.NewGuid(),
                NotifiKey = $"{NotifyKey.DEFAULT_SUPPORT_LOCALE}.{NotifyKey.CREATE_POST_NOTI}",
                NotifyData = JsonConvert.SerializeObject(new { @event.UserName, @event.UserId }),
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
                if (postNotify == null)
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
                        NotifyData = JsonConvert.SerializeObject(new { @event.UserName, @event.FromId, @event.ToId })
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
            if (sharingNotiEntity == null)
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
                    Notiid = sharingNotiEntity == null ? newSharingNotiId : sharingNotiEntity.Notiid
                };
                _context.Add(newNotiBoxSharing);
            }
            await _context.SaveChangesAsync();
        }

        private async Task GenerateNotifyMessage(NotificationDto dto, CancellationToken cancellationToken)
        {
            var setting = await _context.Settings.FirstOrDefaultAsync(x => x.SettingKey == dto.NotifiKey);
            if (setting == null)
                return;

            var notifyData = JsonConvert.DeserializeObject<Dictionary<string, object>>(dto.NotifyData);
            dto.NotifyMessage = setting.SettingValue;

            if (setting.SettingValue.Contains("{X}"))
            {
                //try replace X to username
                if(notifyData.TryGetValue("UserName", out var userName))
                {
                    dto.NotifyMessage = dto.NotifyMessage.Replace("{X}", (string)userName);
                }
            }
            if (setting.SettingValue.Contains("{Y}"))
            {
                if (notifyData.TryGetValue("CurrentComment", out var currentComment))
                {
                    dto.NotifyMessage = dto.NotifyMessage.Replace("{Y}", ((int)(long)currentComment).ToString());
                }
                else
                {
                    if (notifyData.TryGetValue("CurrentInteract", out var currentInteract))
                    {
                        dto.NotifyMessage = dto.NotifyMessage.Replace("{Y}", ((int)(long)currentInteract).ToString());
                    }
                    else
                    {
                        if(notifyData.TryGetValue("TotalVocabInProcess", out var totalVocabInProcess))
                            dto.NotifyMessage = dto.NotifyMessage.Replace("{Y}", ((int)(long)totalVocabInProcess).ToString());
                    }
                }
            }
            if (dto.Type == NotifyType.VOCAB_NOTIFY)
                await GenerateSubNotificationAsync(dto, cancellationToken);
        }

        private async Task GenerateSubNotificationAsync(NotificationDto dto, CancellationToken cancellationToken)
        {
            var notifyData = JsonConvert.DeserializeObject<Dictionary<string, object>>(dto.NotifyData);
            if(notifyData.TryGetValue("DetailProcess", out var detailProcessString))
            {
                var detailProcesses = JsonConvert.DeserializeObject<List<LearningVocabProcessDetailDto>>((string)detailProcessString);
                if (detailProcesses.FirstOrDefault() == null || detailProcesses.Count == 0)
                    return;
                var setting = await _context.Settings.FirstOrDefaultAsync(x => x.SettingKey == detailProcesses.FirstOrDefault().NotifiKey);
                if (setting == null)
                    return;

                var subNofies = new List<string>();
                foreach (var detailProcess in detailProcesses)
                {
                    var notify = setting.SettingValue;
                    var subNotidata = JsonConvert.DeserializeObject<Dictionary<string, object>>(detailProcess.NotifyData);

                    if (notify.Contains("{X}"))
                    {
                        //try replace X to username
                        if (subNotidata.TryGetValue("Title", out var title))
                        {
                            notify = notify.Replace("{X}", (string)title);
                        }
                    }
                    if (notify.Contains("{Y}"))
                    {
                        if (subNotidata.TryGetValue("Percent", out var percent))
                        {
                            notify = notify.Replace("{Y}", ((int)(long)percent).ToString());
                        }
                    }

                    subNofies.Add(notify);
                }
                dto.SubNotification = subNofies;
                dto.NotifyData = null;
            }    
        }

        public async Task<List<NotificationDto>> GetNotiBoxMessageAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var boxId = await CreateUserNotiBoxAsync(userId, cancellationToken);
            if (boxId == Guid.Empty)
                return null;

            var notifications = new List<NotificationDto>();

            var friendNotiMessage = await _context.Friendnotifications.Where(x => x.Boxid == boxId).ToListAsync();
            notifications.AddRange(_mapper.Map<List<NotificationDto>>(friendNotiMessage));

            var postNotiMessage = await _context.Postnotifications.Where(x => x.Boxid == boxId).ToListAsync();
            notifications.AddRange(_mapper.Map<List<NotificationDto>>(postNotiMessage));

            var commentNotiMessage = await _context.Commentnotifications.Where(x => x.Boxid == boxId).ToListAsync();
            notifications.AddRange(_mapper.Map<List<NotificationDto>>(commentNotiMessage));

            var vocabNotiMessage = await _context.Vocabpackagenotifications.Where(x => x.Boxid == boxId).ToListAsync();
            notifications.AddRange(_mapper.Map<List<NotificationDto>>(vocabNotiMessage));

            var sharingNotiMessage = from notiShating in _context.Notiboxsharings
                                     where notiShating.Boxid == boxId
                                     join o in _context.Sharingnotifications
                                     on notiShating.Notiid equals o.Notiid
                                     select new NotificationDto
                                     {
                                         Notiid = o.Notiid,
                                         NotifiKey = o.NotifiKey,
                                         NotifyData = o.NotifyData,
                                         Postid = o.Postid,
                                         Type = NotifyType.SHARING_NOTIFY,
                                         CreatedAt = o.CreatedAt,
                                         UpdatedAt = o.UpdatedAt
                                     };
            notifications.AddRange(sharingNotiMessage);
            
            //need to translate message key to message value
            var supportedLocale = await _context.Settings.Select(x => x.SettingKey).ToListAsync();

            var notifyLocale = await GetUserNotifyLocale(userId, cancellationToken);
            foreach(var notification in notifications)
            {
                var keySetting = notification.NotifiKey.Substring(notification.NotifiKey.IndexOf('.') + 1);
                var fullkey = $"{notifyLocale.ToLower()}.{keySetting}";
                if (supportedLocale.Contains(fullkey))
                    notification.NotifiKey = fullkey;
                //notification.NotifyMessage = "";
                await GenerateNotifyMessage(notification, cancellationToken);
            }

            var result = notifications.OrderByDescending(x => x.UpdatedAt).ToList();

            return result;
        }

        public async Task AddToNotifyBoxAsync(LearningVocabProcessCalculatedEvent @event, CancellationToken cancellationToken = default)
        {
            var notiBoxId = await CreateUserNotiBoxAsync(@event.UserId, cancellationToken);
            if (notiBoxId == Guid.Empty)
                return;

            var detailLearningProcess = new List<LearningVocabProcessDetailDto>();
            foreach(var vocab in @event.Result)
            {
                var percentProcess = 100;
                if(vocab.TotalVocabs != 0)
                    percentProcess = (int)(1.0 - (double)vocab.CurrentNumOfVocab/(double)vocab.TotalVocabs) *100;
                var vocabProcess = new LearningVocabProcessDetailDto
                {
                    NotifiKey = $"{NotifyKey.DEFAULT_SUPPORT_LOCALE}.{NotifyKey.SUMMARY_LEARNING_PROCESS_WEEKLY_DETAIL}",
                    NotifyData = JsonConvert.SerializeObject(new { vocab.Title, Percent = percentProcess }),
                };
                detailLearningProcess.Add(vocabProcess);
            }

            var notifyData = new Dictionary<string, object>()
            {
                ["TotalVocabInProcess"] = @event.Result.Count(),
                ["DetailProcess"] = JsonConvert.SerializeObject(detailLearningProcess)
            };

            var vocabNotify = new Vocabpackagenotification()
            {
                Notiid = Guid.NewGuid(),
                Boxid = notiBoxId,
                NotifiKey = $"{NotifyKey.DEFAULT_SUPPORT_LOCALE}.{NotifyKey.SUMMARY_LEARNING_PROCESS_WEEKLY}",
                NotifyData = JsonConvert.SerializeObject(notifyData)
            };

            _context.Vocabpackagenotifications.Add(vocabNotify);
            await _context.SaveChangesAsync();
        }
    }
}
