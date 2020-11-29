using System;
using Convey;
using Convey.Types;
using Convey.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Trill.Services.Timeline.Core;
using Trill.Services.Timeline.Infrastructure;

namespace Trill.Services.Timeline.Api
{
    internal class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConvey()
                .AddWebApi()
                .AddInfrastructure()
                .Build();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseInfrastructure();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync(context.RequestServices.GetService<AppOptions>().Name);
                });
                endpoints.MapGet("/timelines/{userId:guid}", async context =>
                {
                    var userId = context.Request.RouteValues["userId"].ToString();
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        return;
                    }

                    var storage = context.RequestServices.GetRequiredService<IStorage>();
                    var timeline = await storage.GetTimelineAsync(Guid.Parse(userId));
                    await context.Response.WriteJsonAsync(timeline);
                });
            });
        }
    }
}
