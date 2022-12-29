using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using EscortsReady.VRC;
using EscortsReady;

namespace EscortsReady
{
    public class DiscordService
    {
        private SlashCommandsExtension slash;
        private InteractivityExtension interact;
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
            // Init the bot first
            try
            {
                _token = Program.configuration.GetValue<string>("Discord:Token");
                if (string.IsNullOrEmpty(_token))
                {
                    Logger.LogWarning("Please provide a discord bot token before continuing. this can be done in the data.json file.");
                    Logger.LogInformation("What is your discord Bot Token? Token:");
                    _token = Console.ReadLine();
                    Environment.SetEnvironmentVariable("Discord:Token", _token);

                    if (checkTokenCount > 3) return;
                    else
                    {
                        checkTokenCount++;
                        goto retry;
                    }
                }
                _vrcusername = Program.configuration.GetValue<string>("VRChat:Username");
                _vrcpassword = Program.configuration.GetValue<string>("VRChat:Password");
                if (string.IsNullOrEmpty(_vrcusername) || string.IsNullOrEmpty(_vrcpassword))
                {
                    Logger.LogWarning("Please provide a VRChat username and password before continuing. this can be done in the data.json file.");
                    Logger.LogInformation("What is your VRCUsername? Username:");

                    _vrcusername = Console.ReadLine();
                    Environment.SetEnvironmentVariable("VRChat:Username", _vrcusername);
                    Console.Write("What is your VRCPassword? Password:");
                    _vrcpassword = Console.ReadLine();
                    Environment.SetEnvironmentVariable("VRChat:Password", _vrcpassword);

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
                }).GetAwaiter();
            };
            Client.GuildAvailable += async (s, e) =>
            {
                var escots = await AssetDatabase.LoadAsync<Escorts>(e.Guild);
                await AssetDatabase.SaveAsync(e.Guild, escots);
                await EscortProfile.ResumeSessions(e.Guild);

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
                Console.WriteLine($"Discord Bot Ready!");
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
            GC.SuppressFinalize(this);
        }       


        public static async Task<List<DiscordGuild>> MyGuildsAsync()
        {
            var guilds = Client.Guilds.Select(x => x.Value).ToList();
            var user = Session.CurrentUser;
            return user == null ? guilds : guilds.Where(x => x.Members.ContainsKey(user.id)).ToList();
        }
    }
}