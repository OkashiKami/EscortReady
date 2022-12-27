using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Channels;

namespace EscortsReady
{
    public class SlashCommandsEscort : ApplicationCommandModule
    {
        [SlashCommand("SetEscortManagementRole", "Set's the headEscortRole that will manage the escorts")]
        public async Task SetEscortManagementRole(InteractionContext ctx, [Option("Role", "The headEscortRole that will be set as the default management headEscortRole")] DiscordRole role)
        {
            await ctx.CreateResponseAsync("Please wait...", true);
            if (!await Utils.CheckPerms(ctx)) return;
            try
            {
                var settings = await Settings.LoadAsync(ctx.Guild);
                settings["escortManagementRole"] = role.Id;
                await Settings.SaveAsync(ctx.Guild, settings);


                var dwb = new DiscordWebhookBuilder();
                dwb.WithContent($"**{role.Name}** has been set as the escorts management default headEscortRole ✅");
                await ctx.EditResponseAsync(dwb);
            }
            catch (Exception ex)
            {
                Utils.ReportException(ex, ctx);
            }
        }
        [SlashCommand("SetEscortRole", "Set's the headEscortRole that the escorts have.")]
        public async Task SetEscortRole(InteractionContext ctx, [Option("Role", "The headEscortRole that will be set as the default headEscortRole")]DiscordRole role)
        {
            await ctx.CreateResponseAsync("Please wait...", true);
            if (!await Utils.CheckPerms(ctx, EAM.Head)) return;
            try
            {
                var settings = await Settings.LoadAsync(ctx.Guild);
                settings["escortDefaultRole"] = role.Id;
                await Settings.SaveAsync(ctx.Guild, settings);


                var dwb = new DiscordWebhookBuilder();
                dwb.WithContent($"**{role.Name}** has been set as the escorts default headEscortRole ✅");
                await ctx.EditResponseAsync(dwb);
            }
            catch (Exception ex)
            {
                Utils.ReportException(ex, ctx);
            }
        }
        [SlashCommand("SetAnnouncementChannel", "Set's the role where the announcments will be posted.")]
        public async Task SetAnnouncmentChannel(InteractionContext ctx, [Option("Channel", "The role that will be set as the default headEscortRole")] DiscordChannel channel)
        {
            await ctx.CreateResponseAsync("Please wait...", true);
            if (!await Utils.CheckPerms(ctx, EAM.Head)) return;
            try
            {
                var settings = await Settings.LoadAsync(ctx.Guild);
                settings["escortAnnouncmentChannel"] = channel.Id;
                await Settings.SaveAsync(ctx.Guild, settings);

                var dwb = new DiscordWebhookBuilder();
                dwb.WithContent($"**{channel.Name}** has been set as the escorts default announcment role ✅");
                await ctx.EditResponseAsync(dwb);
            }
            catch (Exception ex)
            {
                Utils.ReportException(ex, ctx);
            }
        }
        [SlashCommand("SetProfileChannel", "Set's the role where the profiles will be posted.")]
        public async Task SetProfileChannel(InteractionContext ctx, [Option("Channel", "The role that will be set as the default headEscortRole")] DiscordChannel channel)
        {
            await ctx.CreateResponseAsync("Please wait...", true);
            if (!await Utils.CheckPerms(ctx, EAM.Head)) return;
            try
            {
                var settings = await Settings.LoadAsync(ctx.Guild);
                settings["escortProfileChannel"] = channel.Id;
                await Settings.SaveAsync(ctx.Guild, settings);

                var dwb = new DiscordWebhookBuilder();
                dwb.WithContent($"**{channel.Name}** has been set as the escorts default prfile role ✅");
                await ctx.EditResponseAsync(dwb);
            }
            catch (Exception ex)
            {
                Utils.ReportException(ex, ctx);
            }
        }

        [SlashCommand("UCProfiles", "Create  or update the profiels for the escorts")]
        public async Task CreateOrUpdateProfiles(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync("Please wait...", true);
            if (!await Utils.CheckPerms(ctx, EAM.Head)) return;
            try
            {
                var settings = await Settings.LoadAsync(ctx.Guild);
                await Utils.CheckAndRemoveInvalidProfilesAsync(ctx.Guild);
                var profileContainer = await EscortProfile.LoadAsync(ctx.Guild);
                // do profile for head escorts
                var headEscortRole = Convert.ToUInt64(settings["escortManagementRole"]);
                var normalEscortRole = Convert.ToUInt64(settings["escortDefaultRole"]);

                var members = ctx.Guild.Members.ToList().FindAll(x => x.Value.Roles.Contains(Utils.GetRoleAsynce(ctx.Guild, headEscortRole).Result));
                foreach (var member in members)
                {
                    if (!profileContainer.HasKey(member.Key) || profileContainer[member.Key] == null)
                    {
                        // profile does not exsit so we need to create one 
                        var profile = EscortProfile.CreateProfile(ctx, member.Value);
                        //Create the message that will hold the embed for this profile
                        var ch =  await Utils.GetChannelAsync(ctx.Guild, Convert.ToUInt64(settings["escortProfileChannel"]));
                        var msg = await ch.SendMessageAsync("please wait...");
                        profile.channelID = ch.Id;
                        profile.messageID = msg.Id;
                        profile.isHead = true;
                        profile  = await EscortProfile.UpdateOrCreateProfileEmbedAsync(msg, profile);
                        if (profile != null)
                            profileContainer[member.Key] = profile;
                    }
                    else
                    {
                        // profile does not exsit so we need to create one 
                        var profile = profileContainer[member.Key];
                        //Get or create the message that will hold the embed for this profile
                        var ch = await Utils.GetChannelAsync(profile);
                        if(ch ==null) ch = ctx.Guild.Channels[Convert.ToUInt64(settings["escortProfileChannel"])];
                        if (ch == null) throw new Exception($"The profile section of the settings for {ctx.Guild} has not been setup yet");
                        var msg = await Utils.GetMessageAsync(profile);
                        if (msg == null)
                        {
                            msg = await ch.SendMessageAsync("please wait...");
                            profile.messageID = msg.Id;
                            profile.channelID = msg.Channel.Id;
                            profile.serverID = ctx.Guild.Id;
                        }
                        profile = await EscortProfile.UpdateOrCreateProfileEmbedAsync(msg, profile);
                        if(profile != null)
                            profileContainer[member.Key] = profile;
                    }
                }

                members = ctx.Guild.Members.ToList().FindAll(x => x.Value.Roles.Contains(Utils.GetRoleAsynce(ctx.Guild, normalEscortRole).Result));
                foreach (var member in members)
                {
                    if (!profileContainer.HasKey(member.Key) || profileContainer[member.Key] == null)
                    {
                        // profile does not exsit so we need to create one 
                        var profile = EscortProfile.CreateProfile(ctx, member.Value);
                        //Create the message that will hold the embed for this profile
                        var ch = await Utils.GetChannelAsync(ctx.Guild, Convert.ToUInt64(settings["escortProfileChannel"]));
                        var msg = await ch.SendMessageAsync("please wait...");
                        profile.channelID = ch.Id;
                        profile.messageID = msg.Id;
                        profile.isHead = false;
                        profile = await EscortProfile.UpdateOrCreateProfileEmbedAsync(msg, profile);
                        if (profile != null)
                            profileContainer[member.Key] = profile;
                    }
                    else
                    {
                        // profile does not exsit so we need to create one 
                        var profile = profileContainer[member.Key];
                        //Get or create the message that will hold the embed for this profile
                        var ch = await Utils.GetChannelAsync(profile);
                        if (ch == null) ch = ctx.Guild.Channels[Convert.ToUInt64(settings["escortProfileChannel"])];
                        if (ch == null) throw new Exception($"The profile section of the settings for {ctx.Guild} has not been setup yet");
                        var msg = await Utils.GetMessageAsync(profile);
                        if (msg == null)
                        {
                            msg = await ch.SendMessageAsync("please wait...");
                            profile.messageID = msg.Id;
                            profile.channelID = msg.Channel.Id;
                            profile.serverID = ctx.Guild.Id;
                        }
                        profile = await EscortProfile.UpdateOrCreateProfileEmbedAsync(msg, profile);
                        if (profile != null)
                            profileContainer[member.Key] = profile;
                    }
                }
                await EscortProfile.SaveAsync(ctx.Guild, profileContainer);


                var dwb = new DiscordWebhookBuilder();
                dwb.WithContent($"profile cration has been complete ✅");
                await ctx.EditResponseAsync(dwb);
            }
            catch (Exception ex)
            {
                Utils.ReportException(ex, ctx);
            }
        }
        [SlashCommand("UCProfile", "Create  or update the profiel for the escorts")]
        public async Task CreateOrUpdateProfile(InteractionContext ctx,
            [Option("Member", "The member that the profile that we want to update is linked to.")] DiscordUser user = null)
        {
            await ctx.CreateResponseAsync("Please wait...", true);
            if (!await Utils.CheckPerms(ctx, EAM.All)) return;
            try
            {
                var member = (DiscordMember)user;
                var settings = await Settings.LoadAsync(ctx.Guild);
                await Utils.CheckAndRemoveInvalidProfilesAsync(ctx.Guild);
                var profileContainer = await EscortProfile.LoadAsync(ctx.Guild);
                member = member != null ? member :  ctx.Member;
                if (!profileContainer.HasKey(member.Id) || profileContainer[member.Id] == null)
                {
                    // profile does not exsit so we need to create one 
                    var profile = EscortProfile.CreateProfile(ctx, member);
                    //Create the message that will hold the embed for this profile
                    var ch = await Utils.GetChannelAsync(ctx.Guild, Convert.ToUInt64(settings["escortProfileChannel"]));
                    var msg = await ch.SendMessageAsync("please wait...");
                    profile.channelID = ch.Id;
                    profile.messageID = msg.Id;
                    profile.isHead = true;
                    profile = await EscortProfile.UpdateOrCreateProfileEmbedAsync(msg, profile);
                    if (profile != null)
                        profileContainer[member.Id] = profile;
                }
                else
                {
                    // profile does not exsit so we need to create one 
                    var profile = profileContainer[member.Id];
                    //Get or create the message that will hold the embed for this profile
                    var ch = await Utils.GetChannelAsync(profile);
                    if (ch == null) ch = ctx.Guild.Channels[Convert.ToUInt64(settings["escortProfileChannel"])];
                    if (ch == null) throw new Exception($"The profile section of the settings for {ctx.Guild} has not been setup yet");
                    var msg = await Utils.GetMessageAsync(profile);
                    if (msg == null)
                    {
                        msg = await ch.SendMessageAsync("please wait...");
                        profile.messageID = msg.Id;
                        profile.channelID = msg.Channel.Id;
                        profile.serverID = ctx.Guild.Id;
                    }
                    profile = await EscortProfile.UpdateOrCreateProfileEmbedAsync(msg, profile);
                    if (profile != null)
                        profileContainer[member.Id] = profile;
                }

                await EscortProfile.SaveAsync(ctx.Guild, profileContainer);


                var dwb = new DiscordWebhookBuilder();
                dwb.WithContent($"profile cration has been complete ✅");
                await ctx.EditResponseAsync(dwb);
            }
            catch (Exception ex)
            {
                Utils.ReportException(ex, ctx);
            }
        }

    }
}