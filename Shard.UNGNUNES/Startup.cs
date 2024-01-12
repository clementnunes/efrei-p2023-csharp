using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Shard.Shared.Core;
using Shard.UNGNUNES.Services;
using Shard.UNGNUNES.Controllers;
using SystemClock = Shard.Shared.Core.SystemClock;

namespace Shard.UNGNUNES
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
            services.AddSingleton<IClock, SystemClock>();
            services.AddSingleton<MapGenerator>();
            services.AddSingleton<ShipFightingService>();
            services.AddSingleton<UnitsService>();
            services.AddSingleton<PlanetService>();
            services.AddSingleton<StarSystemService>();
            services.AddSingleton<SectorService>();
            services.AddSingleton<UsersService>();
            services.AddSingleton<BuildingService>();
            services.AddSingleton<MapGenerator>();
            services.AddHostedService<BackgroundTaskService>();
            services.Configure<MapGeneratorOptions>(options => options.Seed = "shard.UNGNUNES");
            services.AddAuthentication("Basic")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", options => { });
    
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Shard.UNGNUNES", Version = "v2"});
                c.EnableAnnotations();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shard.UNGNUNES v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}