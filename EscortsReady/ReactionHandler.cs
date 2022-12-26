// See https://aka.ms/new-console-template for more information

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus;
using System.Diagnostics;

namespace PermissionEx
{
    public class ReactionHandler
    {
        public static async Task OnInteractionCreated(ComponentInteractionCreateEventArgs e)
        {
            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            await LoggerEx.LogAsync(e);
            if (e.Id.Contains("-location")) await DoLocationAsync(e);
            else if (e.Id.Contains("-gender")) await DoGenderAsync(e);
            else if (e.Id.Contains("-dms")) await DoDMsAsync(e);
            else if (e.Id.Contains("-relationship")) await DoRelationshipAsync(e);
            else if (e.Id.Contains("-playmode")) await DoPlaymodeAsync(e);
            else if (e.Id.Contains("-headsettype")) await DoHeadsetTypeAsync(e);
            else if (e.Id.Contains("-creatorworld")) await DoWorldCreatorAsync(e);
            else if (e.Id.Contains("-creatoravatar")) await DoAvatarCreatorAsync(e);
            else if (e.Id.Contains("-creatorassets")) await DoAssetCreatorAsync(e);

            else if (e.Id.Contains("-gamesand")) await DoSandboxAsync(e);
            else if (e.Id.Contains("-gamerts")) await DoRTSAsync(e);
            else if (e.Id.Contains("-gamefps")) await DoFPSAsync(e);
            else if (e.Id.Contains("-gamemoba")) await DoMOBAAsync(e);
            else if (e.Id.Contains("-gamesim")) await DoSimulationAsync(e);
            else if (e.Id.Contains("-gamepuz")) await DoPuzzlersAsync(e);
            else if (e.Id.Contains("-gameact")) await DoActionAsync(e);
            else if (e.Id.Contains("-gamesur")) await DoSurvivalAsync(e);
            else if (e.Id.Contains("-gamepla")) await DoPlatformerAsync(e);
            else if (e.Id.Contains("-gamerpg")) await DoRPGAsync(e);




            await Task.Delay(TimeSpan.FromSeconds(30));
            var wb = new DiscordWebhookBuilder();
            wb.AddEmbed(new DiscordEmbedBuilder().WithDescription("Okay ✅"));
            await e.Interaction.EditOriginalResponseAsync(wb);
        }

        #region Reactions Process

        private static async Task DoLocationAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> locationroles = new List<DiscordRole>();
            // Create roles if they don't exist

            if (roles.Find(x => x.Name == "Canada") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Canada", color: DiscordColor.Gray);
                locationroles.Add(crole);
            }
            else locationroles.Add(roles.Find(x => x.Name == "Canada"));

            if (roles.Find(x => x.Name == "Chaina") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Chaina", color: DiscordColor.Gray);
                locationroles.Add(crole);
            }
            else locationroles.Add(roles.Find(x => x.Name == "Chaina"));

            if (roles.Find(x => x.Name == "NorthAmerica") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("NorthAmerica", color: DiscordColor.Gray);
                locationroles.Add(crole);
            }
            else locationroles.Add(roles.Find(x => x.Name == "NorthAmerica"));

            if (roles.Find(x => x.Name == "SouthAmerica") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("SouthAmerica", color: DiscordColor.Gray);
                locationroles.Add(crole);
            }
            else locationroles.Add(roles.Find(x => x.Name == "SouthAmerica"));

            if (roles.Find(x => x.Name == "Europe") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Europe", color: DiscordColor.Gray);
                locationroles.Add(crole);
            }
            else locationroles.Add(roles.Find(x => x.Name == "Europe"));

            if (roles.Find(x => x.Name == "France") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("France", color: DiscordColor.Gray);
                locationroles.Add(crole);
            }
            else locationroles.Add(roles.Find(x => x.Name == "France"));

            if (roles.Find(x => x.Name == "Japan") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Japan", color: DiscordColor.Gray);
                locationroles.Add(crole);
            }
            else locationroles.Add(roles.Find(x => x.Name == "Japan"));

            if (roles.Find(x => x.Name == "UnitedKingdom") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("UnitedKingdom", color: DiscordColor.Gray);
                locationroles.Add(crole);
            }
            else locationroles.Add(roles.Find(x => x.Name == "UnitedKingdom"));

            if (roles.Find(x => x.Name == "Africa") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Africa", color: DiscordColor.Gray);
                locationroles.Add(crole);
            }
            else locationroles.Add(roles.Find(x => x.Name == "Africa"));

            if (roles.Find(x => x.Name == "Australia") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Australia", color: DiscordColor.Gray);
                locationroles.Add(crole);
            }
            else locationroles.Add(roles.Find(x => x.Name == "Australia"));


            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in locationroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Location Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Location Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        private static async Task DoGenderAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> genderroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "He/Him") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("He/Him", color: new DiscordColor("#1871ec"));
                genderroles.Add(crole);
            }
            else genderroles.Add(roles.Find(x => x.Name == "He/Him"));

            if (roles.Find(x => x.Name == "She/Her") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("She/Her", color: new DiscordColor("#ff0000"));
                genderroles.Add(crole);
            }
            else genderroles.Add(roles.Find(x => x.Name == "She/Her"));

            if (roles.Find(x => x.Name == "They/Them") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("They/Them", color: new DiscordColor("#e7da19"));
                genderroles.Add(crole);
            }
            else genderroles.Add(roles.Find(x => x.Name == "They/Them"));

            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);

            DiscordMessage msg = null;
            // remove all location roles
            foreach (var lr in genderroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Gender Role {lr.Name} has been Revoked!";
                    msg = await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Gender Role {lr.Name} has been Granted!";
                        msg = await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }

            if (msg != null)
                await msg.DeleteAsync();
        }
        private static async Task DoDMsAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "DMs Open") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("DMs Open", color: new DiscordColor("#00ff00"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "DMs Open"));

            if (roles.Find(x => x.Name == "DMs Closed") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("DMs Closed", color: new DiscordColor("#ff0000"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "DMs Closed"));

            if (roles.Find(x => x.Name == "DMs Ask") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("DMs Ask", color: new DiscordColor("#e7da19"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "DMs Ask"));


            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord DM Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord DM Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        private static async Task DoRelationshipAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "Single") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Single", color: new DiscordColor("#0000ff"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Single"));

            if (roles.Find(x => x.Name == "Taken") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Taken", color: new DiscordColor("#ff0000"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Taken"));

            if (roles.Find(x => x.Name == "Complicated") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Complicated", color: DiscordColor.Yellow);
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Complicated"));

            if (roles.Find(x => x.Name == "Open") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Open", color: DiscordColor.Blue);
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Open"));


            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord DM Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord DM Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        private static async Task DoPlaymodeAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "Full Body") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Full Body", color: new DiscordColor("#0000ff"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Full Body"));

            if (roles.Find(x => x.Name == "Half Body") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Half Body", color: new DiscordColor("#ff0000"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Half Body"));

            if (roles.Find(x => x.Name == "Desktop") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Desktop", color: DiscordColor.Yellow);
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Desktop"));


            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Playmode Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Playmode Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        private static async Task DoHeadsetTypeAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "Index") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Index", color: new DiscordColor("#0000ff"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Index"));

            if (roles.Find(x => x.Name == "Vive") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Vive", color: new DiscordColor("#ff0000"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Vive"));

            if (roles.Find(x => x.Name == "Oculus") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Oculus", color: DiscordColor.Yellow);
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Oculus"));


            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Headset Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Headset Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        private static async Task DoWorldCreatorAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "World Creator") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("World Creator", color: new DiscordColor("#33ff33"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "World Creator"));

            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Creator Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Creator Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        private static async Task DoAvatarCreatorAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "Avatar Creator") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Avatar Creator", color: new DiscordColor("#33ff33"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Avatar Creator"));

            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Creator Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Creator Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        private static async Task DoAssetCreatorAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "Asset Creator") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Asset Creator", color: new DiscordColor("#33ff33"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Asset Creator"));

            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Creator Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Creator Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }


        #region Game Genere
        private static async Task DoSandboxAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "Sandbox") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Sandbox", color: new DiscordColor("#33ff33"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Sandbox"));

            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Genere Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Genere Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        private static async Task DoRTSAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "RTS") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("RTS", color: new DiscordColor("#33ff33"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "RTS"));

            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Genere Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Genere Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        private static async Task DoFPSAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "FPS") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("FPS", color: new DiscordColor("#33ff33"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "FPS"));

            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Genere Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Genere Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        private static async Task DoMOBAAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "MOBA") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("MOBA", color: new DiscordColor("#33ff33"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "MOBA"));

            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Genere Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Genere Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        private static async Task DoSimulationAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "Simulation") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Simulation", color: new DiscordColor("#33ff33"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Simulation"));

            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Genere Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Genere Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        private static async Task DoPuzzlersAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "Puzzlers") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Puzzlers", color: new DiscordColor("#33ff33"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Puzzlers"));

            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Genere Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Genere Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        private static async Task DoActionAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "Action") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Action", color: new DiscordColor("#33ff33"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Action"));

            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Genere Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Genere Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        private static async Task DoSurvivalAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "Survival") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Survival", color: new DiscordColor("#33ff33"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Survival"));

            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Genere Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Genere Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        private static async Task DoPlatformerAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "Platformer") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("Platformer", color: new DiscordColor("#33ff33"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "Platformer"));

            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Genere Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Genere Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        private static async Task DoRPGAsync(ComponentInteractionCreateEventArgs e)
        {
            var roles = e.Guild.Roles.Select(x => x.Value).ToList().OrderByDescending(x => x.Position).ToList();
            List<DiscordRole> dmroles = new List<DiscordRole>();
            // Create roles if they don't exist
            if (roles.Find(x => x.Name == "RPG") == null)
            {
                var crole = await e.Guild.CreateRoleAsync("RPG", color: new DiscordColor("#33ff33"));
                dmroles.Add(crole);
            }
            else dmroles.Add(roles.Find(x => x.Name == "RPG"));

            var member = (DiscordMember)e.User;
            var role = e.Guild.Roles.Select(x => x.Value).ToList().Find(x => x.Name == e.Id.Split('-')[1].Split('_')[1]);
            // remove all location roles
            foreach (var lr in dmroles)
            {
                if (member.Roles.Contains(lr))
                {
                    await member.RevokeRoleAsync(lr);

                    var wb = new DiscordWebhookBuilder();
                    wb.Content = $"Discord Genere Role {lr.Name} has been Revoked!";
                    await e.Interaction.EditOriginalResponseAsync(wb);
                    await Task.Delay(2000);
                }
                else
                {
                    if (lr.Id == role.Id)
                    {
                        await member.GrantRoleAsync(role);

                        var wb = new DiscordWebhookBuilder();
                        wb.Content = $"Discord Genere Role {lr.Name} has been Granted!";
                        await e.Interaction.EditOriginalResponseAsync(wb);
                        await Task.Delay(3000);
                    }
                }
            }
        }
        #endregion

        #endregion
    }
}