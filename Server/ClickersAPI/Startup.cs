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
using System.Text;

namespace ClickersAPI
{
    public class Startup
    {
        public Startup(IConfiguration _Configuration)
        {
            Configuration = _Configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection _Services)
        {
            string connectionString = string.Empty;
#if MySQL && DEBUG
            connectionString = "DebugMySqlConnection";
#elif MySQL && !DEBUG
            connectionString = "ReleaseMySQLConnection";
#elif !MySQL && DEBUG
            connectionString = "DebugMSSqlConnection";
#elif !MySQL && !DEBUG
            connectionString = "ReleaseMSSqlConnection";
#endif
            
#if MySQL
            _Services.AddDbContext<ApplicationDbContext>(_Options =>
                _Options.UseMySql(Configuration.GetConnectionString(connectionString))
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

            _Services.AddControllers().AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();

            _Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(_Options =>
                _Options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["jwt:key"])),
                    ClockSkew = TimeSpan.Zero
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
    }
}
