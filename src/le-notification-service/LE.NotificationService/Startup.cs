using AutoMapper;
using LE.Library.Consul;
using LE.Library.Host;
using LE.Library.Kernel;
using LE.Library.MessageBus.Extensions;
using LE.Library.MessageBus.Kafka;
using LE.Library.Warmup;
using LE.NotificationService.Events;
using LE.NotificationService.Extensions;
using LE.NotificationService.Hubs;
using LE.NotificationService.Infrastructure.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
            services.AddConsul();
            services.AddRequestHeader();

            AddAutoMappers(services);
            AddDbContext(services);

            services.AddMessageBus(Configuration, new Dictionary<Type, string>
            {
                [typeof(InteractPostEvent)] = MessageValue.INTERACTED_POST_EVENT,
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

            app.UseCors(x => x
              .AllowAnyMethod()
              .AllowAnyHeader()
              .SetIsOriginAllowed(origin => true) // allow any origin
              .AllowCredentials()); // allow credentials

            app.UseCustomAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("/hub/notification");
            });
        }

        private void AddAutoMappers(IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(mc => {
                //mc.AddProfile(new UserProfile());
                //mc.AddProfile(new LanguageProfile());
                //mc.AddProfile(new PostProfile());

                //neo4j mapper
                //mc.AddProfile(new CountryProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
        }
        private void AddDbContext(IServiceCollection services)
        {
            services.AddDbContext<LanggeneralDbContext>(options => options.UseNpgsql(Env.DB_CONNECTION_STRING));
        }
    }
}
