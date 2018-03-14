using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SlidableLive.Clients;
using SlidableLive.Internals;
using SlidableLive.Services;
using StackExchange.Redis;

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

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

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

            services.AddMvc();
        }

        private void ConfigureAuth(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();
            //var identityPaths = new IdentityPaths(Configuration);
            //services.AddAuthentication(options =>
            //    {
            //        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //    })
            //    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
            //        options =>
            //        {
            //            options.ExpireTimeSpan = TimeSpan.FromHours(24);
            //            options.LoginPath = identityPaths.Login;
            //            options.LogoutPath = identityPaths.Logout;
            //            options.Cookie.Name = ".AspNetCore.Cookies";
            //        });
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
    public static class SlidableClaimTypes
    {
        public const string Handle = "http://schema.slidable.io/identity/claims/handle";
    }

}