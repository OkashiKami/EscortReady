using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace EscortsReady
{
    public class DiscordService
    {
        public ILogger Logger { get => Program.logger; }
        public static DiscordClient client { get; private set; }
        private SlashCommandsExtension slash;
        private InteractivityExtension interact;



        public void Initialize()
        {
            Console.Title = "Orbital [Bot]";
            Logger.LogInformation($"{GetType().Name} Initializing...");
            // Init the bot first
            try
            {
                var token = Program.config["DeveloperConfig:Token"];
                if (string.IsNullOrEmpty(token))
                {
                    Logger.LogError($"{GetType().Name} Please provide a discord bot token before continuing. this can be done in the data.json file.");
                    return;
                }
                
                client = new DiscordClient(new DiscordConfiguration
                {
                    Token = token,
                    TokenType = TokenType.Bot,
                    MinimumLogLevel = LogLevel.Debug,
                    Intents = DiscordIntents.All,
                });
                slash = client.UseSlashCommands();
                interact = client.UseInteractivity();
                RegisterSlashCommands();
                RegisterEvents();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name} {ex}");
            }
            Logger.LogInformation($"{GetType().Name} Initialization complete!");
        }

        private void RegisterSlashCommands()
        {
            slash.RegisterCommands<SlashCommandsEscort>();
        }
        private void RegisterEvents()
        {
            client.ComponentInteractionCreated += async(s, e) =>
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                Task.Factory.StartNew(async () =>
                {
                    if (e.Id.StartsWith("escort_")) await EscortProfile.HandelInteractionAsync(s, e);
                });
            };
        }


        public void Dispose()
        {   
            if (client != null)
                client.DisconnectAsync();
            client = null;
            GC.SuppressFinalize(this);
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (Program.config == null) { await Task.Delay(1); }

            Initialize();

            Logger.LogInformation($"{GetType().Name} is starting.");
            stoppingToken.Register(() => Logger.LogInformation($"{GetType().Name} is stopping."));
            await client.ConnectAsync();
            Logger.LogInformation($"{GetType().Name} has connected, and is now Ready.");
            await Task.Delay(-1, Program.cancelToken);
            Dispose(); 
            Logger.LogInformation("DiscordService has stopped.");
        }
    }
}