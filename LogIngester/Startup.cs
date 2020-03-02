using System;
using System.Net.Http;
using LogIngester.DnsIngest.Configuration;
using LogIngester.DnsIngest.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using VictoriaMetrics.Client;
using VictoriaMetrics.Models.Configuration;
using VictoriaMetrics.Services.Converters;
using VictoriaMetrics.Services.Formatters;

namespace LogIngester
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddControllers().AddNewtonsoftJson();

            services.AddSingleton<IMetricFormatter<string>, LineFormatter>();
            services.AddSingleton<IMetricConverter, MetricConverter>();

            services.AddSingleton(provider =>
            {
                var influxDbConfig = new VictoriaConfig();
                Configuration.GetSection("VictoriaMetrics").Bind(influxDbConfig);
                return influxDbConfig;
            });


            services.AddSingleton(provider =>
            {
                var workerConfig = new WorkerConfig();
                Configuration.GetSection("Worker").Bind(workerConfig);
                return workerConfig;
            });

            services.AddHttpClient<IVictoriaMetricClient, VictoriaMetricClient>()
                    .SetHandlerLifetime(TimeSpan.FromHours(1)) //Set lifetime to five minutes
                    .AddPolicyHandler(GetRetryPolicy());
            services.AddSingleton<IIngestWorker, IngestWorker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var jitterer = new Random();
            return HttpPolicyExtensions
                   .HandleTransientHttpError()
                   .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                   .WaitAndRetryAsync(6, // exponential back-off plus some jitter
                       retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                       + TimeSpan.FromMilliseconds(jitterer.Next(0, 100))
                   );
        }
    }
}