using System;
using System.Threading.Tasks;
using Destiny.ScrimTracker.Logic.Adapters;
using Destiny.ScrimTracker.Logic.Repositories;
using Destiny.ScrimTracker.Logic.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

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
            
            // ASP.NET Registrations
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddOpenIdConnect("Auth0", options => {
                    // Set the authority to your Auth0 domain
                    options.Authority = $"https://{Configuration["Auth0:Domain"]}";

                    // Configure the Auth0 Client ID and Client Secret
                    options.ClientId = Configuration["Auth0:ClientId"];
                    options.ClientSecret = Environment.GetEnvironmentVariable("Auth0:ClientSecret");

                    // Set response type to code
                    options.ResponseType = OpenIdConnectResponseType.Code;

                    // Configure the scope
                    options.Scope.Add("openid");

                    // Set the callback path, so Auth0 will call back to http://localhost:3000/callback
                    // Also ensure that you have added the URL as an Allowed Callback URL in your Auth0 dashboard
                    options.CallbackPath = new PathString("/callback");

                    // Configure the Claims Issuer to be Auth0
                    options.ClaimsIssuer = "Auth0";

                    options.Events = new OpenIdConnectEvents
                    {
                        // handle the logout redirection
                        OnRedirectToIdentityProviderForSignOut = (context) =>
                        {
                            var logoutUri = $"https://{Configuration["Auth0:Domain"]}/v2/logout?client_id={Configuration["Auth0:ClientId"]}";

                            var postLogoutUri = context.Properties.RedirectUri;
                            if (!string.IsNullOrEmpty(postLogoutUri))
                            {
                                if (postLogoutUri.StartsWith("/"))
                                {
                                    // transform to absolute
                                    var request = context.Request;
                                    postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                                }
                                logoutUri += $"&returnTo={ Uri.EscapeDataString(postLogoutUri)}";
                            }

                            context.Response.Redirect(logoutUri);
                            context.HandleResponse();

                            return Task.CompletedTask;
                        }
                    };
                });
            
            services.AddControllers();
            services.AddMvc(options => options.EnableEndpointRouting = false);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
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
