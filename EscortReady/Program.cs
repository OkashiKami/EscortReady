using Azure.Identity;
using EscortsReady.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Azure;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace EscortsReady
{
    public class Program
    {
       public static CancellationToken ct;
       private static WebApplicationBuilder builder;
       public static ConfigurationManager configuration { get; private set; }
       public static WebApplication app { get; private set; }
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
            configuration = builder.Configuration;
            
            var keyVaultEndpoint = new Uri(configuration.GetValue<string>("Endpoints:EscortReadyKeyVault"));
            var azureAppConfigurationEnpoint = configuration.GetValue<string>("Endpoints:EscortReadyAppConfig");
            var storageConnectionString = configuration.GetValue<string>("Endpoints:EscortReadyStorage");

            configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
            // Add Azure App Configuration to the container.
            if (!string.IsNullOrEmpty(azureAppConfigurationEnpoint))
            {
                // Use the connection string if it is available.
                builder.Configuration.AddAzureAppConfiguration(options =>
                {
                    options.Connect(azureAppConfigurationEnpoint)
                    .ConfigureRefresh(refresh =>
                    {
                        // All configuration values will be refreshed if the sentinel key changes.
                        refresh.Register("TestApp:Settings:Sentinel", refreshAll: true);
                    });
                });
            }
            else if (Uri.TryCreate(configuration["Endpoints:EscortReadyAppConfig"], UriKind.Absolute, out var endpoint))
            {
                // Use Azure Active Directory authentication.
                // The identity of this app should be assigned 'App Configuration Data Reader' or 'App Configuration Data Owner' role in App Configuration.
                // For more information, please visit https://aka.ms/vs/azure-app-configuration/concept-enable-rbac
                builder.Configuration.AddAzureAppConfiguration(options =>
                {
                    options.Connect(endpoint, new DefaultAzureCredential())
                    .ConfigureRefresh(refresh =>
                    {
                        // All configuration values will be refreshed if the sentinel key changes.
                        refresh.Register("TestApp:Settings:Sentinel", refreshAll: true);
                    });
                });
            }
            builder.Services.AddAzureAppConfiguration();
            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddControllersWithViews();
            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddBlobServiceClient(configuration["OkashiTechApStorage:blob"], preferMsi: true);
                clientBuilder.AddQueueServiceClient(configuration["OkashiTechApStorage:queue"], preferMsi: true);
            });
            builder.Services.AddAuthentication(o =>
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
                    ValidIssuer = configuration.GetValue<string>("jwt:ValidIssuer"),
                    ValidAudience = configuration.GetValue<string>("jwt:ValidAudience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("jwt:EncryptionKey"))),
                };
            })
            .AddOAuth("Discord", o =>
            {
                o.AuthorizationEndpoint = configuration.GetValue<string>("Discord:BaseAuthURL");
                o.Scope.Add("identify");
                o.Scope.Add("email");
                o.CallbackPath = new PathString("/auth/oauthCallback");
                o.ClientId = configuration.GetValue<string>("Discord:ClientID");
                o.ClientSecret = configuration.GetValue<string>("Discord:ClientSecret");
                o.TokenEndpoint = configuration.GetValue<string>("Discord:BaseTokenEndpoint");
                o.UserInformationEndpoint = configuration.GetValue<string>("Discord:BaseUserEndpoint");
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
            builder.Services.Configure<Settings>(builder.Configuration.GetSection("EscortReady:Settings"));
            builder.Services.AddFeatureManagement();
            await Storage.Setup(storageConnectionString);
            app = builder.Build();
            logger = app.Logger;
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAzureAppConfiguration();
            app.UseRouting();
            app.UseAuthorization();
            app.MapRazorPages();
            app.MapControllerRoute(
              name: "default",
              pattern: "{controller=Home}/{action=Index}/{id?}");
        }
    }
}










