using Azure.Identity;
using EscortsReady.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
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
        public static ConfigurationManager configuration { get; private set; }
        public static WebApplication app { get; private set; }
        public static ILogger logger { get; private set; }

        public static async Task Main(params string[] args)
        {
            Console.Title = "EscortsReady";
            Directory.CreateDirectory(Utils.tmpDire);
            Directory.GetFiles(Utils.tmpDire).ToList().ForEach(f => File.Delete(f));
            Directory.Delete(Utils.tmpDire, true);
            Directory.CreateDirectory(Utils.tmpDire);

            var builder = WebApplication.CreateBuilder(args);
            configuration = builder.Configuration;

            var keyVaultEndpoint = new Uri(configuration.GetValue<string>("Endpoints:EscortReadyKeyVault"));
            var azureAppConfigurationEnpoint = configuration.GetValue<string>("Endpoints:EscortReadyAppConfig");
            var storageConnectionString = configuration.GetValue<string>("Endpoints:EscortReadyStorage");

            try { configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential()); } catch { }
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

            builder.Services.AddControllers();
            // Add services to the container.
            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddBlobServiceClient(configuration["OkashiTechApStorage:blob"]);
                clientBuilder.AddQueueServiceClient(configuration["OkashiTechApStorage:queue"]);
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
            await Storage.Setup(storageConnectionString);
            // Add services to the container.
            builder.Services.AddControllersWithViews();
            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
            logger = app.Logger;


            Task.Run(async () => await DiscordService.StartAsync(logger)).GetAwaiter();
            await Task.Delay(TimeSpan.FromSeconds(3));
            await app.RunAsync(ct);
            ct = new CancellationToken(true);
        }
    }
}