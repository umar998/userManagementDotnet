using DigitalPrinting.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalPrinting
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
            services.AddControllers();
            services.AddCors(cors => cors.AddPolicy("MyPolicy", builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); }));
            services.Configure<ImagePathSettings>(Configuration.GetSection("ImagePathSettings"));
            LogManager.Configuration = CreateNLogConfig(); // Call the method to create NLog configuration
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders(); // Clear other logging providers (e.g., Microsoft ILogger)
                loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                loggingBuilder.AddNLog();
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DigitalPrinting", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DigitalPrinting v1"));
            }
            app.UseCors("MyPolicy");
            app.UseRouting();

            app.UseAuthorization();

            app.UseStaticFiles(); // Enable static file serving
            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private LoggingConfiguration CreateNLogConfig()
        {
            var config = new LoggingConfiguration();

            // Define NLog targets and rules here
            var fileTarget = new FileTarget("file")
            {
                FileName = "C:\\Users\\umara\\Desktop\\AKSA Tasks\\BackEndC#\\DigitalPrinting\\logs\\requestlog-${shortdate}.log",
                Layout = "${longdate}|${event-properties:item=EventId_Id}|${logger}|${uppercase:${level}}|${message} ${exception}|url:${aspnet-request-url}|action:${aspnet-mvc-action}|request-info:${event-properties:item=RequestInfo}|response-info:${event-properties:item=ResponseInfo}|response-content:${event-properties:item=ResponseContent}|error:${event-properties:item=Error}",
                MaxArchiveFiles = 10,
                ArchiveAboveSize = 524288  // 500 KB in bytes
            };

            config.AddTarget(fileTarget);

            config.AddRuleForAllLevels(fileTarget); // Log all levels to the file target

            return config;
        }

    }
}
