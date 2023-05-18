using AutoMapper;
using Hangfire;
using Hangfire.PostgreSql;
using LE.Library.Consul;
using LE.Library.Host;
using LE.Library.Kernel;
using LE.Library.MessageBus.Extensions;
using LE.Library.MessageBus.Kafka;
using LE.Library.Warmup;
using LE.NotificationService.AutoMappers;
using LE.NotificationService.Events;
using LE.NotificationService.Extensions;
using LE.NotificationService.Hubs;
using LE.NotificationService.Infrastructure.Infrastructure;
using LE.NotificationService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LE.NotificationService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected virtual Assembly GetMessageChannelProviderAssembly() => AssemblyManager.GetAssemblies(a => a.GetName().Name == "LE.Library.MessageBus.Kafka").FirstOrDefault();
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionConfig = services.ConfigMessageBusConnection(Configuration);
            AssemblyManager.Load();
            services.WarmupServiceStartup();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "LE.NotificationService", Version = "v1" });
            });

            services.AddCors();
            services.AddSignalR();
            services.AddHttpContextAccessor();
            services.AddCustomAuthorization(Configuration);
            services.AddConsul();
            services.AddRequestHeader();
            services.AddScoped<INotifyService, NotifyService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            //services.AddScoped<IUserService, UserService>();

            AddAutoMappers(services);
            AddDbContext(services);
            AddHangfireCronJob(services);

            services.AddHangfireServer(serverOptions =>
            {
                serverOptions.ServerName = "LE Schedule Server";
            });

            services.AddMessageBus(Configuration, new Dictionary<Type, string>
            {
                [typeof(PostCreatedEvent)] = MessageValue.POST_CREATED_EVENT,
                [typeof(InteractPostEvent)] = MessageValue.INTERACTED_POST_EVENT,
                [typeof(InteractCommentEvent)] = MessageValue.INTERACTED_COMMENT_EVENT,
                [typeof(CommentPostEvent)] = MessageValue.COMMENTED_POST_EVENT,
                [typeof(FriendRequestSentEvent)] = MessageValue.FRIEND_REQUEST_SENT_EVENT,
                [typeof(FriendRequestAcceptedEvent)] = MessageValue.FRIEND_REQUEST_ACCEPT_EVENT,
                [typeof(LearningVocabProcessCalculatedEvent)] = MessageValue.LEARNING_PROCESS_CALCULATED_EVENT,
            }, GetMessageChannelProviderAssembly(), connectionConfig);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LE.NotificationService v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseConsul();

            app.UseCors(x => x
              .AllowAnyMethod()
              .AllowAnyHeader()
              .SetIsOriginAllowed(origin => true) // allow any origin
              .AllowCredentials()); // allow credentials

            //app.UseCustomAuthorization();

            app.UseHangfireServer();
            app.UseHangfireDashboard();
            //RecurringJob.AddOrUpdate(() => Console.WriteLine("Minutely Job"), Cron.Minutely);
            app.UseMiddleware<RecurringJobMiddleware>();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("/hub/notification");
                endpoints.MapHangfireDashboard();
            });
        }

        private void AddAutoMappers(IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(mc => {
                mc.AddProfile(new NotificationProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
        }
        private void AddDbContext(IServiceCollection services)
        {
            services.AddDbContext<LanggeneralDbContext>(options => options.UseNpgsql(Env.DB_CONNECTION_STRING));
        }

        private void AddHangfireCronJob(IServiceCollection services)
        {
            services.AddHangfire(configuration => configuration
                  .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UsePostgreSqlStorage(Env.CRON_JOB_DB_CONNECTION_STRING, new PostgreSqlStorageOptions
                  {
                      SchemaName = "le.schedule"
                  }));
        }
    }
}
