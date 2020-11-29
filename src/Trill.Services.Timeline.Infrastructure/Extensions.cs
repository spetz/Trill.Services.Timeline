using System;
using System.Text;
using Convey;
using Convey.Auth;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.CQRS.Queries;
using Convey.Discovery.Consul;
using Convey.Docs.Swagger;
using Convey.HTTP;
using Convey.LoadBalancing.Fabio;
using Convey.MessageBrokers;
using Convey.MessageBrokers.CQRS;
using Convey.MessageBrokers.RabbitMQ;
using Convey.Metrics.Prometheus;
using Convey.Persistence.Redis;
using Convey.Tracing.Jaeger;
using Convey.WebApi.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Trill.Services.Timeline.Core;
using Trill.Services.Timeline.Core.Events.External;
using Trill.Services.Timeline.Infrastructure.Decorators;
using Trill.Services.Timeline.Infrastructure.Logging;
using Trill.Services.Timeline.Infrastructure.Redis;

namespace Trill.Services.Timeline.Infrastructure
{
    public static class Extensions
    {
        public static IConveyBuilder AddInfrastructure(this IConveyBuilder builder)
        {
            builder.Services
                .AddScoped<LogContextMiddleware>()
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddTransient<IStorage, RedisStorage>();

            builder
                .AddCommandHandlers()
                .AddEventHandlers()
                .AddInMemoryCommandDispatcher()
                .AddInMemoryEventDispatcher()
                .AddQueryHandlers()
                .AddInMemoryQueryDispatcher()
                .AddJwt()
                .AddHttpClient()
                .AddConsul()
                .AddFabio()
                .AddRabbitMq()
                .AddRedis()
                .AddPrometheus()
                .AddJaeger()
                .AddWebApiSwaggerDocs();

            builder.Services.AddSingleton<ICorrelationIdFactory, CorrelationIdFactory>();
            
            builder.Services.TryDecorate(typeof(ICommandHandler<>), typeof(LoggingCommandHandlerDecorator<>));
            builder.Services.TryDecorate(typeof(IEventHandler<>), typeof(LoggingEventHandlerDecorator<>));
            
            return builder;
        }
        
        public static long ToUnixTimeMilliseconds(this DateTime dateTime)
            => new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
        {
            app.UseMiddleware<LogContextMiddleware>()
                .UseSwaggerDocs()
                .UseJaeger()
                .UsePrometheus()
                .UseConvey()
                .UseAccessTokenValidator()
                .UseAuthentication()
                .UseRabbitMq()
                .SubscribeEvent<UserFollowed>()
                .SubscribeEvent<UserUnfollowed>();

            return app;
        }
        
        internal static string GetSpanContext(this IMessageProperties messageProperties, string header)
        {
            if (messageProperties is null)
            {
                return string.Empty;
            }

            if (messageProperties.Headers.TryGetValue(header, out var span) && span is byte[] spanBytes)
            {
                return Encoding.UTF8.GetString(spanBytes);
            }

            return string.Empty;
        }
    }
}