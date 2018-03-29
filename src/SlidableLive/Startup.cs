using System;
using System.Threading.Tasks;
using App.Metrics;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SlidableLive.Clients;
using SlidableLive.Internals;
using SlidableLive.Services;
using StackExchange.Redis;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace SlidableLive
{
    [PublicAPI]
    public class Startup
    {
        private static ConnectionMultiplexer _connectionMultiplexer;
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            _env = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.Configure<IdentityOptions>(options => { options.User.RequireUniqueEmail = true; });
            services.AddSingleton<IIdentityPaths, IdentityPaths>();

            services.Configure<Options.ServiceOptions>(Configuration.GetSection("Services"));
            services.AddSingleton<IShowsClient, ShowsClient>();
            services.AddSingleton<ISlidesClient, SlidesClient>();
            services.AddSingleton<INotesClient, NotesClient>();
            services.AddSingleton<IQuestionsClient, QuestionsClient>();

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUserInfo, UserInfo>();

            ConfigureAuth(services);

            services.AddSingleton<IApiKeyProvider, ApiKeyProvider>();

            var redisHost = Configuration.GetSection("Redis").GetValue<string>("Host");
            if (!string.IsNullOrWhiteSpace(redisHost))
            {
                var redisPort = Configuration.GetSection("Redis").GetValue<int>("Port");
                if (redisPort == 0)
                {
                    redisPort = 6379;
                }

                _connectionMultiplexer = ConnectionMultiplexer.Connect($"{redisHost}:{redisPort}");
                services.AddSingleton(_connectionMultiplexer);
            }

            if (!_env.IsDevelopment())
            {
                var dpBuilder = services.AddDataProtection().SetApplicationName("slidable");

                if (_connectionMultiplexer != null)
                {
                    dpBuilder.PersistKeysToRedis(_connectionMultiplexer, "DataProtection:Keys");
                }
            }
            else
            {
                services.AddDataProtection()
                    .DisableAutomaticKeyGeneration()
                    .SetApplicationName("slidable");
            }

            services.AddMvc(o => { o.AddMetricsResourceFilter(); });

            var influxDb = Configuration["AppMetrics:InfluxDbServer"];
            var influxDatabase = Configuration["AppMetrics:InfluxDbDatabase"];
            if (!string.IsNullOrWhiteSpace(influxDb))
            {
                Console.WriteLine($"AppMetrics reporting to {influxDb}/{influxDatabase}");
                var metrics = AppMetrics.CreateDefaultBuilder()
                    .Report.ToInfluxDb(influxDb, influxDatabase, TimeSpan.FromSeconds(5))
                    .Report.ToConsole()
                    .Configuration.Configure(o =>
                    {
                        o.DefaultContextLabel = "slidable-web";
                        o.AddServerTag(Environment.MachineName);
                        o.Enabled = true;
                        o.ReportingEnabled = true;
                    })
                    .Build();

                services.AddMetrics(metrics);
                services.AddMetricsTrackingMiddleware();
                services.AddSingleton<IHostedService>(new MetricsReportWriter(metrics, TimeSpan.FromSeconds(5)));
            }
        }

        private void ConfigureAuth(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();
        }

        private string AdjustedIdentityPath(string path)
        {
            var identityPathPrefix = Configuration["Runtime:IdentityPathPrefix"];
            return string.IsNullOrWhiteSpace(identityPathPrefix)
                ? path
                : "/" + string.Join('/', new[] {identityPathPrefix.Trim('/'), path.Trim('/')});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment() || string.Equals(Configuration["Runtime:DeveloperExceptionPage"], "true", StringComparison.OrdinalIgnoreCase))
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseMetricsAllMiddleware()
                .UseMetricsApdexTrackingMiddleware()
                .UseMetricsErrorTrackingMiddleware()
                .UseMetricsRequestTrackingMiddleware();

            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseMiddleware<BypassAuthMiddleware>();
            }
            else
            {
                app.UseAuthentication();
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}