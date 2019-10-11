﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Msa.Comm.Lab.Events;
using Msa.Comm.Lab.Services.Order.ApiClients;
using Polly;
using Refit;

namespace Msa.Comm.Lab.Services.Order
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            bool RetryableStatusCodesPredicate(HttpStatusCode statusCode) =>
                statusCode == HttpStatusCode.BadGateway
                    || statusCode == HttpStatusCode.ServiceUnavailable
                    || statusCode == HttpStatusCode.GatewayTimeout;

            services.AddRefitClient<ICatalogApiClient>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://msa.comm.lab.services.catalog"))
                .AddPolicyHandler(Policy
                    .Handle<HttpRequestException>()
                    .OrResult<HttpResponseMessage>(msg => RetryableStatusCodesPredicate(msg.StatusCode))
                    //.RetryAsync(5)
                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                ));

            services.AddMassTransit(x =>
            {
                x.AddBus(provider =>
                Bus.Factory.CreateUsingRabbitMq(cfg => 
                {
                    cfg.Host(new Uri($"rabbitmq://rabbitmq:/"),
                    hostConfig =>
                    {
                        hostConfig.Username("guest");
                        hostConfig.Password("guest");
                    });
                    cfg.UseExtensionsLogging(provider.GetRequiredService<ILoggerFactory>());
                }));

                EndpointConvention.Map<IOrderCreatedEvent>(new Uri("rabbitmq://rabbitmq:/integration"));
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
