using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LogIngester.DnsIngest.Configuration;
using LogIngester.DnsIngest.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VictoriaMetrics.VictoriaMetrics.Client;
using VictoriaMetrics.VictoriaMetrics.Models.Configuration;
using VictoriaMetrics.VictoriaMetrics.Services.Converters;
using VictoriaMetrics.VictoriaMetrics.Services.Formatters;

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

            services.AddHttpClient<IVictoriaMetricClient, VictoriaMetricClient>();
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
    }
}