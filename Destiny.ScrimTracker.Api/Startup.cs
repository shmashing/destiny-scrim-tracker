using System;
using Destiny.ScrimTracker.Logic.Adapters;
using Destiny.ScrimTracker.Logic.Repositories;
using Destiny.ScrimTracker.Logic.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Destiny.ScrimTracker.Api
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
            // Register Third Party 
            var connectionString = ParseConnection(Environment.GetEnvironmentVariable("DATABASE_URL"));
            
            // Register Adapters
            services.AddDbContext<DatabaseContext>(options => 
                options.UseNpgsql(connectionString,b => b.MigrationsAssembly("Destiny.ScrimTracker.Api")));

            // Register Repositories
            services.AddTransient<IGuardianRepository, GuardianRepository>();
            services.AddTransient<IGuardianEloRepository, GuardianEloRepository>();
            services.AddTransient<IGuardianEfficiencyRepository, GuardianEfficiencyRepository>();
            services.AddTransient<IGuardianMatchResultsRepository, GuardianMatchResultsRepository>();
            services.AddTransient<IMatchTeamRepository, MatchTeamRepository>();
            services.AddTransient<IMatchRepository, MatchRepository>();
            
            // Register Services
            services.AddTransient<IGuardianService, GuardianService>();
            services.AddTransient<IMatchService, MatchService>();
            
            services.AddControllers();
            services.AddMvc();
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private string ParseConnection(string connectionUri)
        {
            if (string.IsNullOrEmpty(connectionUri)) // local development
            {
                return Configuration.GetConnectionString("GuardianDatabase");
            }
            var uri = new Uri(connectionUri);
            var database = uri.AbsolutePath.Trim('/');
            var user = uri.UserInfo.Split(':')[0];
            var password = uri.UserInfo.Split(':')[1];
            var port = uri.Port > 0 ? uri.Port : 5432;
            var connectionString = $"Server={uri.Host};Database={database};User Id={user};Password={password};Port={port}";
            return connectionString;
        }
    }
}
