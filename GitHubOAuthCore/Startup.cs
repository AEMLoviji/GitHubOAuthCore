using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace GitHubOAuthCore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            AddAuthenticationServices(services);
        }

        private void AddAuthenticationServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                SetAuthOptions(options);
            })
            .AddCookie()
            .AddOAuth("GitHub", options =>
            {
                ConfigureGitHubOAuthOptions(options);
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc();
        }

        private static void SetAuthOptions(AuthenticationOptions options)
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = "GitHub";
        }

        private void ConfigureGitHubOAuthOptions(OAuthOptions options)
        {
            ConfigureGitHubConnector(options);
            PrepareClaimActions(options);
            BindOAuthEvents(options);
        }

        private void ConfigureGitHubConnector(OAuthOptions options)
        {
            options.ClientId = Configuration["GitHub:ClientId"];
            options.ClientSecret = Configuration["GitHub:ClientSecret"];
            options.CallbackPath = new PathString(Configuration["GitHub:CalbackPath"]);
            options.AuthorizationEndpoint = Configuration["GitHub:AuthroziationEndpoint"];
            options.TokenEndpoint = Configuration["GitHub:TokenEndpoint"];
            options.UserInformationEndpoint = Configuration["GitHub:UserInfoEndpoint"];
        }

        private static void PrepareClaimActions(OAuthOptions options)
        {
            options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
            options.ClaimActions.MapJsonKey("urn:github:login", "login");
            options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
            options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");
            options.ClaimActions.MapJsonKey("urn:github:email", "email");
            options.ClaimActions.MapJsonKey("urn:github:bio", "bio");
            options.ClaimActions.MapJsonKey("urn:github:created_at", "created_at");
        }

        private static void BindOAuthEvents(OAuthOptions options)
        {
            options.Events = new OAuthEvents
            {
                OnCreatingTicket = async context =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                    var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                    response.EnsureSuccessStatusCode();

                    var user = JObject.Parse(await response.Content.ReadAsStringAsync());

                    context.RunClaimActions(user);
                }
            };
        }

    }
}
