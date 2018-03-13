using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.WindowsAzure.Storage;
using SlidableLive.Clients;
using SlidableLive.Identity;
using SlidableLive.Internals;
using SlidableLive.Services;

namespace SlidableLive
{
    [PublicAPI]
    public class Startup
    {
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
            services.AddDbContextPool<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.Configure<IdentityOptions>(options => { options.User.RequireUniqueEmail = true; });

            services.Configure<Options.ServiceOptions>(Configuration.GetSection("Services"));
            services.AddSingleton<IShowsClient, ShowsClient>();
            services.AddSingleton<ISlidesClient, SlidesClient>();
            services.AddSingleton<INotesClient, NotesClient>();
            services.AddSingleton<IQuestionsClient, QuestionsClient>();

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUserInfo, UserInfo>();

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                    options => { options.ExpireTimeSpan = TimeSpan.FromHours(24); })
                .AddTwitter(o =>
                {
                    o.ConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"];
                    o.ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"];
                });


            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, SlidableClaimsPrincipalFactory>();

            services.AddSingleton<IApiKeyProvider, ApiKeyProvider>();

            if (!_env.IsDevelopment())
            {
                var connectionString =
                    Configuration.GetValue("DataProtection:AzureStorageConnectionString", string.Empty);
                if ((!string.IsNullOrWhiteSpace(connectionString)) &&
                    CloudStorageAccount.TryParse(connectionString, out var cloudStorageAccount))
                {
                    services.AddDataProtection()
                        .SetApplicationName("slidable")
                        .PersistKeysToAzureBlobStorage(cloudStorageAccount, "keys/keys.xml");
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
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
}