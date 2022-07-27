using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authorization;
using LightenceServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using LightenceServer.Interfaces;
using LightenceServer.Services;
using LightenceServer.Hubs;
using System.Net;
using LightenceServer.Models;

namespace LightenceServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("AzureDB")); //change if Azure DB is enable
                //options.UseInMemoryDatabase("LightenceDatabase");
            });

            services.AddIdentity<LightUser, IdentityRole>(setup =>
            {
                setup.Password.RequiredLength = 8;
                setup.Password.RequireDigit = true;
                setup.Password.RequireUppercase = true;
                setup.Password.RequireLowercase = true;
                setup.Password.RequireNonAlphanumeric = false;
                //setup.User.RequireUniqueEmail = true;
                setup.SignIn.RequireConfirmedEmail = true;
            }).AddEntityFrameworkStores<AppDbContext>()
              .AddDefaultTokenProviders();

            services.AddSession(options =>
            {
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.MaxAge = TimeSpan.FromMinutes(60);
                options.Cookie.Path = "/Admin";
            });

            var tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Token:Key"]));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = tokenKey,
                    ValidateAudience = true,
                    ValidAudience = Configuration["Token:Audience"],
                    ValidateIssuer = true,
                    ValidIssuer = Configuration["Token:Issuer"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    //RequireExpirationTime = true
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/lighthub")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };

            });

            services.AddAuthorization(config =>
            {
                config.AddPolicy("RequirePremium", policy =>
                {
                    policy.RequireRole("Premium");
                });
                config.AddPolicy("RequireAdmin", policy =>
                {
                    policy.RequireRole("Admin");
                });
                config.AddPolicy("RequireUser", policy =>
                {
                    policy.RequireRole("User", "Premium", "Admin");
                });
            });

            services.AddTransient<IProductKeyManager, ProductKeyManager>();
            services.AddTransient<IServerLogManager, ServerLogManager>();
            services.AddSingleton<ILiveCountManager<string>, LiveCountManager<string>>();
            services.AddSingleton<IHubGroupManager, HubGroupManager>();

            services.AddControllersWithViews();

            services.AddSignalR().AddHubOptions<LightHub>(options =>
            {
                //options.MaximumReceiveMessageSize = 1024000;
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
                options.StreamBufferCapacity = 10;
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            }).AddAzureSignalR(Configuration["SignalR:Key"]);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseRouting();

            /*
            app.UseStatusCodePages(async context =>
            {
                context.HttpContext.Response.ContentType = "text/html";
                await context.HttpContext.Response.WriteAsync(
                    "<html><body><div style='display:flex;flex-direction:row;justify-content:center;padding-left:40pt;font-size:48;color:#2196f3'>"
                    + context.HttpContext.Response.StatusCode + "</div>"
                    + "<div style='display:flex;flex-direction:row;justify-content:center;padding-left:40pt;font-size:48;color:#383f51'>"
                    + ((HttpStatusCode)(context.HttpContext.Response.StatusCode)).ToString()
                    + "</div></body></html>");
            });
            */

            app.UseSession();

            app.Use(async (context, next) =>
            {
                var token = context.Session.GetString("JWToken");
                if (context.Request.Path.Value.Contains("/Admin"))
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Request.Headers.Add("Authorization", "Bearer " + token);
                    }
                }
                await next();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHttpsRedirection();
            app.UseFileServer(true);

            app.UseEndpoints(config =>
            {
                config.MapControllers();
                config.MapHub<LightHub>("/lighthub");
            });
        }
    }
}
