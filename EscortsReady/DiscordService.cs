using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PermissionEx;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EscortsReady
{
    public class DiscordService
    {
        private SlashCommandsExtension slash;
        private InteractivityExtension interact;
        private ulong _guildid;
        private string? _token;
        private string? _vrcusername;
        private string? _vrcpassword;
        public static DiscordService bot;
        private int checkTokenCount;
        private int checkVRCCred;
        private ILogger Logger;
        public static DiscordClient Client { get; private set; }


        public DiscordService(ILogger logger)
        {
            this.Logger = logger;
        }

        public static async Task StartAsync(ILogger logger)
        {
            
            bot = new DiscordService(logger);
            await bot.InitializeAsync();
            await bot.RunAsync();
        }

        private async Task InitializeAsync()
        {
        retry:
            Console.Title = "EscortReady";
            // Init the bot first
            try
            {
                _token = Utils.MKToken();
                if (string.IsNullOrEmpty(_token))
                {
                    Logger.LogWarning("Please provide a discord bot token before continuing. this can be done in the data.json file.");
                    Logger.LogInformation("What is your discord Bot Token? Token:");
                    _token = Console.ReadLine();
                    Environment.SetEnvironmentVariable("DTOKEN", _token);

                    if (checkTokenCount > 3) return;
                    else
                    {
                        checkTokenCount++;
                        goto retry;
                    }
                }
                _vrcusername = Program.Configuration.GetValue<string>("VRCU");
                _vrcpassword = Program.Configuration.GetValue<string>("VRCP");
                if (string.IsNullOrEmpty(_vrcusername) || string.IsNullOrEmpty(_vrcpassword))
                {
                    Logger.LogWarning("Please provide a VRChat username and password before continuing. this can be done in the data.json file.");
                    Logger.LogInformation("What is your VRCUsername? Username:");

                    _vrcusername = Console.ReadLine();
                    Environment.SetEnvironmentVariable("VRCU", _vrcusername);
                    Console.Write("What is your VRCPassword? Password:");
                    _vrcpassword = Console.ReadLine();
                    Environment.SetEnvironmentVariable("VRCP", _vrcpassword);

                    if (checkVRCCred > 3) return;
                    else
                    {
                        checkVRCCred++;
                        goto retry;
                    }
                }
                Client = new DiscordClient(new DiscordConfiguration
                {
                    Token = _token,
                    TokenType = TokenType.Bot,
                    MinimumLogLevel = LogLevel.Debug,
                    Intents = DiscordIntents.All,
                });
                slash = Client.UseSlashCommands();
                interact = Client.UseInteractivity();               
                await RegisterSlashCommandsAsync();
                RegisterEvents();
                await VRChatService.LoginAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name} {ex}");
            }
            Logger.LogInformation($"{GetType().Name} Initialization complete!");
        }
        private async Task RegisterSlashCommandsAsync()
        {
            slash.RegisterCommands<SlashCommandsEscort>();
            if (await VRChatService.LoginAsync())
                slash.RegisterCommands<SlashVRChatCommands>();
            else VRChatService.UsingVRC = false;
        }
        private void RegisterEvents()
        {
            Client.ComponentInteractionCreated += async(s, e) =>
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                Task.Factory.StartNew(async () =>
                {
                    if (e.Id.StartsWith("escort_")) await EscortProfile.HandelInteractionAsync(s, e);
                });
            };
        }
        public async Task RunAsync()
        {
            if (string.IsNullOrEmpty(_token)) return;

            if (Client != null)
            {
                Logger.LogInformation($"{GetType().Name} is starting.");
                Program.ct.Register(() => Logger.LogInformation($"{GetType().Name} is stopping."));
                await Client.ConnectAsync();
                Logger.LogInformation($"{GetType().Name} has connected, and is now Ready.");
                await Task.Delay(-1, Program.ct);
                Dispose();
                Logger.LogInformation("DiscordService has stopped.");                
            }
            Dispose();
        }
        public void Dispose()
        {   
            if (Client != null)
                Client.DisconnectAsync();
            Client = null;
            GC.SuppressFinalize(this);
        }       
    }
}