using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.FeatureManagement;
using Microsoft.Graph;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Directory = System.IO.Directory;
using File = System.IO.File;
using WebApplication = Microsoft.AspNetCore.Builder.WebApplication;



namespace EscortsReady
{
    public class Program
    {
        public static CancellationToken ct = new CancellationToken(false);
        private static WebApplicationBuilder builder;
        public static ConfigurationManager Configuration { get; private set; }
        private static WebApplication app;
        public static ILogger logger { get; private set; }

        public static async Task Main(params string[] args)
        {
            Console.Title = "EscortReady";
            await SetupWebServer(args);
            Task.Run(async () => await DiscordService.StartAsync(logger)).GetAwaiter();
            await Task.Delay(TimeSpan.FromSeconds(3));
            await app.RunAsync(ct);
            ct = new CancellationToken(true);
        }

        private static async Task SetupWebServer(params string[] args)
        {
            builder = WebApplication.CreateBuilder(args);
            Configuration = builder.Configuration;            
            Configuration.AddAzureAppConfiguration(Configuration.GetValue<string>("Endpoints:EscortReadyAppConfig"));
            ConfigureServices(builder.Services);
            app = builder.Build();
            logger = app.Logger;
            await Configure(app, app.Environment);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddControllers();
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration.GetValue<string>("jwt:ValidIssuer"),
                    ValidAudience = Configuration.GetValue<string>("jwt:ValidAudience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("jwt:EncryptionKey"))),
                };
            })
            .AddOAuth("Discord", o =>
            {
                o.AuthorizationEndpoint = Configuration.GetValue<string>("Discord:BaseAuthURL");
                o.Scope.Add("identify");
                o.Scope.Add("email");
                o.CallbackPath = new PathString("/auth/oauthCallback");
                o.ClientId = Configuration.GetValue<string>("Discord:ClientID");
                o.ClientSecret = Configuration.GetValue<string>("Discord:ClientSecret");
                o.TokenEndpoint = Configuration.GetValue<string>("Discord:BaseTokenEndpoint");
                o.UserInformationEndpoint = Configuration.GetValue<string>("Discord:BaseUserEndpoint");
                o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                o.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
                o.ClaimActions.MapJsonKey(ClaimTypes.Hash, "discriminator");
                o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

                o.AccessDeniedPath = "/";
                o.Events = new OAuthEvents
                {
                    OnCreatingTicket = async c =>
                    {
                        var req = new HttpRequestMessage(HttpMethod.Get, c.Options.UserInformationEndpoint);
                        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", c.AccessToken);
                        var res = await c.Backchannel.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, c.HttpContext.RequestAborted);
                        res.EnsureSuccessStatusCode();
                        var user = JsonDocument.Parse(await res.Content.ReadAsStringAsync()).RootElement;
                        c.RunClaimActions(user);
                    }
                };

            });
            // Bind configuration "TestApp:Settings" section to the Settings object
            services.Configure<Settings>(builder.Configuration.GetSection("EscortReady:Settings"));
            services.AddFeatureManagement();
        }

        private static async Task Configure(WebApplication app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Augment the ConfigurationBuilder with Azure App Configuration
                // Pull the connection string from an environment variable

                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Retrieve the connection string
            var appConfigConnectionString = builder.Configuration.GetValue<string>("Endpoints:EscortReadyAppConfig");
            var storageConnectionString = builder.Configuration.GetValue<string>("Endpoints:EscortReadyStorage");

            // Load configuration from Azure App Configuration
            builder.Configuration.AddAzureAppConfiguration(appConfigConnectionString);
            await Storage.Setup(storageConnectionString);
            app.UseHttpsRedirection();
            app.UseStaticFiles();


            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

        }
    }
}







