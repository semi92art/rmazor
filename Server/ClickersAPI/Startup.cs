#define MySQL

using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Specialized;
using System.Text;
using ClickersAPI.Helpers;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Quartz;
using Quartz.Impl;

namespace ClickersAPI
{
    public class Startup
    {
        private static string ConnectionString => "ReleaseMySQLConnection"; // "DebugMySqlConnection";

        private readonly IScheduler  m_Scheduler;
        private readonly FirebaseApp m_FirebaseApp;
        
        public Startup(IConfiguration _Configuration)
        {
            Configuration = _Configuration;
            m_Scheduler = ConfigureQuartz();
            m_FirebaseApp = ConfigureFirebase();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection _Services)
        {
#if MySQL
            _Services.AddDbContext<ApplicationDbContext>(_Options =>
                _Options.UseMySql(Configuration.GetConnectionString(ConnectionString))
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
#else
            _Services.AddDbContext<ApplicationDbContext>(_Options =>
                _Options.UseSqlServer(Configuration.GetConnectionString("connectionString"))
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
#endif

            _Services.AddAutoMapper(typeof(Startup));

            _Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            _Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            _Services.AddSingleton(_Provider => m_Scheduler);
            _Services.AddSingleton(_Provider => m_FirebaseApp);

            _Services.AddControllers().AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();

            _Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(_Options =>
                _Options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = false,
                    ValidateAudience         = false,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["jwt:key"])),
                    ClockSkew                = TimeSpan.Zero
                });
        }

        public void Configure(IApplicationBuilder _App, IWebHostEnvironment _Env)
        {
            if (_Env.IsDevelopment()) 
                _App.UseDeveloperExceptionPage();
            
            _App.UseHttpsRedirection();
            _App.UseRouting();

            _App.UseAuthentication();
            _App.UseAuthorization();

            _App.UseEndpoints(_Endpoints =>
            {
                _Endpoints.MapControllers();
            });
        }

        public IScheduler ConfigureQuartz()
        {
            var props = new NameValueCollection
            {
                {"quarts.serializer.type", "binary"}
            };
            var factory = new StdSchedulerFactory(props);
            var scheduler = factory.GetScheduler().Result;
            scheduler.Start().Wait();
            return scheduler;
        }

        public FirebaseApp ConfigureFirebase()
        {
            var credential = GoogleCredential.FromJson(Credentials.FirebaseAdmin);
            var appOptions = new AppOptions
            {
                Credential = credential, 
                ProjectId = "minigames-collection-299814"
            };
            return FirebaseApp.Create(appOptions);
        }

        private void OnShutdown()
        {
            if (!m_Scheduler.IsShutdown)
                m_Scheduler.Shutdown();
        }
    }
}
