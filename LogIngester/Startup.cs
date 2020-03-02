using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LogIngester.Configuration;
using LogIngester.DnsIngest.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vibrant.InfluxDB.Client;

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
            
            services.AddSingleton(provider =>
            {
                var influxDbConfig = new InfluxDbConfig();
                Configuration.GetSection("InfluxDb").Bind(influxDbConfig);
                return influxDbConfig;
            });
            
            services.AddSingleton(provider =>
            {
                var influxDbConfig = provider.GetService<InfluxDbConfig>();
                return new InfluxClient(new Uri(influxDbConfig.Uri), influxDbConfig.Username, influxDbConfig.Password, provider.GetService<HttpClient>());
            });
            services.AddSingleton(provider =>
            {
                var workerConfig = new WorkerConfig();
                Configuration.GetSection("Worker").Bind(workerConfig);
                return workerConfig;
            });

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