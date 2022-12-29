using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using EscortReady;
using EscortsReady.Utilities;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;
using System.Text;

namespace EscortsReady
{
    internal class EscortProfile : FileData
    {
        public static Profile? CreateProfile(InteractionContext ctx, DiscordMember member)
        {
            var profile = new Profile();
            profile.serverID = ctx.Guild.Id;
            profile.memberID = member.Id;
            profile.name = member.DisplayName;
            profile.AvatarUrl = member.AvatarUrl;
            profile.dateCreated = DateTime.Now;
            return profile;
        }
        
        public static async Task<Profile> UpdateOrCreateProfileEmbedAsync(DiscordMessage msg, Profile profile)
        {
            var settings = await AssetDatabase.LoadAsync<Settings>(msg.Channel.Guild);
            var headEscortRole = Convert.ToUInt64(settings["escortManagementRole"]);
            var normalEscortRole = Convert.ToUInt64(settings["escortDefaultRole"]);
            //Create embed for the message 
            var texture = await Utils.CreateEscortEmblem(Utils.CalculateRangking(profile), profile);
            var embed = new DiscordEmbedBuilder();
            var m = await Utils.GetMemberAsync(profile);
            var mr = await Utils.GetRole(msg.Channel.Guild, profile.isHead ? headEscortRole : normalEscortRole);
            embed.WithAuthor($"{m.DisplayName} ({mr.Name})", $"https://{msg.Channel.Guild.Id}.com", Storage.GetFileUrl("EscortsReady.png"));
            embed.WithThumbnail(texture);
            embed.AddField("VRChat Name", profile.vrcName, true);
            embed.AddField("Gender", profile.gender.ToString(), true);
            embed.AddField("Service Type", profile.serviceType.ToString(), true);
            embed.AddField("Equipment Type", profile.equipmentType.ToString(), true);
            embed.AddField("Role Type", profile.roleType.ToString(), true);
            embed.AddField("Prefered Body Types", profile.bodyTypes.ToString(), true);
            embed.AddField("Available Times", profile.timesAvailable, true);
            embed.AddField("Kinks/Fetishes", profile.kinksAndFetishes, true);
            embed.AddField("Do’s/Donts (These should be made very clear for members to understand)", profile.doAndDonts);
            embed.AddField("Why they became and escort", profile.ReasonForJoining);
            embed.AddField("About Them", profile.AboutMe);
            if (!string.IsNullOrEmpty(profile.AvatarUrl) && !profile.AvatarUrl.Equals(Utils.Unset, StringComparison.OrdinalIgnoreCase))
                embed.WithImageUrl(profile.AvatarUrl);
            embed.WithFooter("ERS", Storage.GetFileUrl("EscortsReady.png"));
            await msg.ModifyAsync(async x =>
            {
                x.WithAllowedMentions(Mentions.All);
                var member = await Utils.GetMemberAsync(profile);
                x.Content = member.Mention;
                x.Embed = embed;

                x.AddComponents(new[]
                {

                    new DiscordButtonComponent(ButtonStyle.Primary, "escort_request", "Request", false, new DiscordComponentEmoji("📲")),
                    new DiscordButtonComponent(ButtonStyle.Success, "escort_like", "Like", false, new DiscordComponentEmoji("👍")),
                    new DiscordButtonComponent(ButtonStyle.Danger, "escort_dislike", "Dislike", false, new DiscordComponentEmoji("👎")),
                    new DiscordButtonComponent(ButtonStyle.Primary, "escort_tip", "Tip", string.IsNullOrEmpty(profile.tipLink) || profile.tipLink.Equals(Utils.Unset, StringComparison.OrdinalIgnoreCase), new DiscordComponentEmoji(1030292832155619339)),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "escort_edit", "Edit", false, new DiscordComponentEmoji(874852097135345676)),
                });
            });
            msg = await msg.Channel.GetMessageAsync(msg.Id);
            return profile;
        }
        public static async Task<DiscordMessageBuilder> ProfileToEmbed(DiscordGuild guild, Profile profile)
        {
            var settings = await AssetDatabase.LoadAsync<Settings>(guild);
            var headEscortRole = Convert.ToUInt64(settings["escortManagementRole"]);
            var normalEscortRole = Convert.ToUInt64(settings["escortDefaultRole"]);
            var mb = new DiscordMessageBuilder();
            
            //Create embed for the message 
            var texture = await Utils.CreateEscortEmblem(Utils.CalculateRangking(profile), profile);
            var embed = new DiscordEmbedBuilder();
            var m = await Utils.GetMemberAsync(profile);
            var mr = await Utils.GetRole(guild, profile.isHead ? headEscortRole : normalEscortRole);
            embed.WithAuthor($"{m.DisplayName} ({mr.Name})", $"https://{guild.Id}.com", Storage.GetFileUrl("EscortsReady.png"));
            embed.WithThumbnail(texture);
            
            embed.AddField("**1** VRChat Name", profile.vrcName, true);
            embed.AddField("**2** Gender", profile.gender.ToString(), true);
            embed.AddField("**3** Service Type", profile.serviceType.ToString(), true);
            embed.AddField("**4** Equipment Type", profile.equipmentType.ToString(), true);
            embed.AddField("**5** Role Type", profile.roleType.ToString(), true);
            embed.AddField("**6** Prefered Body Types", profile.bodyTypes.ToString(), true);
            embed.AddField("**7** Available Times", profile.timesAvailable, true);
            embed.AddField("**8** Kinks/Fetishes", profile.kinksAndFetishes, true);
            embed.AddField("**9** Do’s/Dont's (These should be made very clear for members to understand)", profile.doAndDonts);
            embed.AddField("**10** Why they became and escort", profile.ReasonForJoining);
            embed.AddField("**11** About Them", profile.AboutMe);
            if (!string.IsNullOrEmpty(profile.AvatarUrl) && !profile.AvatarUrl.Equals(Utils.Unset, StringComparison.OrdinalIgnoreCase))
            {
                embed.AddField("**12** Profile Image", "** **");
                embed.WithImageUrl(profile.AvatarUrl);
            }
            embed.AddField("**13** tipLink", profile.tipLink);

            embed.WithFooter("ERS (type exit to cancel)", Storage.GetFileUrl("EscortsReady.png"));
            mb.WithAllowedMentions(Mentions.All);
            var member = await Utils.GetMemberAsync(profile);
            mb.WithContent(member.Mention);
            mb.WithEmbed(embed);
            mb.AddComponents(new[] 
            { 
                new DiscordButtonComponent(ButtonStyle.Secondary, "escort_edit_field_1", "VRC Name"),
                new DiscordButtonComponent(ButtonStyle.Secondary, "escort_edit_field_2", "Gender"),
                new DiscordButtonComponent(ButtonStyle.Secondary, "escort_edit_field_3", "Service"),
                new DiscordButtonComponent(ButtonStyle.Secondary, "escort_edit_field_4", "Equipment"),
                new DiscordButtonComponent(ButtonStyle.Secondary, "escort_edit_field_5", "Role"),
            });
            mb.AddComponents(new[]
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, "escort_edit_field_6", "Prefered Gender"),
                new DiscordButtonComponent(ButtonStyle.Secondary, "escort_edit_field_7", "Times Available"),
                new DiscordButtonComponent(ButtonStyle.Secondary, "escort_edit_field_8", "Kinks/Fetishes"),
                new DiscordButtonComponent(ButtonStyle.Secondary, "escort_edit_field_9", "Do’s/Dont's"),
                new DiscordButtonComponent(ButtonStyle.Secondary, "escort_edit_field_10", "Join Reson"),
            });
            mb.AddComponents(new[]
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, "escort_edit_field_11", "About Me"),
                new DiscordButtonComponent(ButtonStyle.Secondary, "escort_edit_field_12", "Tip Link"),
                new DiscordButtonComponent(ButtonStyle.Secondary, "escort_edit_field_13", "Profile Image"),
            });
            mb.AddComponents(new[] { new DiscordButtonComponent(ButtonStyle.Primary, "escort_edit_submit", "Submit", false, new DiscordComponentEmoji("✅")) });
            return mb;
        }
       
        private static DiscordGuild GetInteractionGuild(ComponentInteractionCreateEventArgs e)
        {
            var msgurl = e.Message.Embeds.First().Author.Url.ToString();
            var guidid = msgurl.Replace("https://", string.Empty).Replace(".com/", string.Empty);
            var guildid = ulong.Parse(guidid);
            var guild = e.Guild != null ? e.Guild : DiscordService.Client.Guilds[guildid];
            return guild;
        }

        public static async Task HandelInteractionAsync(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {
            try
            {
                var guild = GetInteractionGuild(e);
                var settings = await AssetDatabase.LoadAsync<Settings>(guild);
                var member = await Utils.GetMember(guild, e.Message.MentionedUsers.First().Id);
                var escorts = await AssetDatabase.LoadAsync<Escorts>(guild);
                var profile = escorts[member.Id];

                switch (e.Id)
                {
                    case "escort_request":
                        var escort = await Utils.GetMember(e.Guild, e.Message.MentionedUsers.First().Id);
                        var client = await Utils.GetMember(e.Guild, e.Interaction.User.Id);


                        // Check if the user has made a request to an escort already
                        if (!escorts.library.Any(x => x.Value.requestLists.Any(x => x.member.Id == client.Id)))
                        {
                            Task.Factory.StartNew(async () =>
                            {
                                var escorts = await AssetDatabase.LoadAsync<Escorts>(guild);
                                var profile = escorts[member.Id];
                                while(profile.requestLists.Count > 0)
                                {
                                    var requestIndex = profile.requestLists.IndexOf(profile.requestLists.Find(x => x.member.Id == client.Id));
                                    var ts = DateTime.Now - profile.requestLists[requestIndex].requestSubmitTime;
                                    if(ts.TotalHours >= 1) profile.requestLists.RemoveAt(requestIndex);
                                    await Task.Delay(500);
                                }
                                escorts[escort.Id] = profile;
                                await AssetDatabase.SaveAsync(guild, escorts);

                            }).GetAwaiter();
                            var mb = new DiscordMessageBuilder();
                            mb.Content = $"⚠️ **Warning** ⚠️" +
                                $"Hello {escort.Mention} you have been request by {client.Mention}." +
                                $"Would you liek to accet or decline this request?";
                            mb.WithAllowedMentions(Mentions.All);
                            mb.AddComponents(new[]
                            {
                                new DiscordButtonComponent(ButtonStyle.Primary, "escort_accept_request", "Accept"),
                                new DiscordButtonComponent(ButtonStyle.Danger, "escort_decline_request", "Declien"),
                            });
                            var msg = await escort.SendMessageAsync(mb);


                            var rs = await msg.WaitForButtonAsync(TimeSpan.FromDays(1));
                            if (rs.TimedOut)
                            {
                                await escort.SendMessageAsync("Hello, we notices that you havent replied to this message with in the 24h time frame, the request has been canceld.");
                                await client.SendMessageAsync("Sorry the escort that you have request has not responed in 24h, the request has been canceld. you can try to request again in 15m");
                                await msg.DeleteAsync();
                                break;
                            }
                            switch (rs.Result.Id)
                            {
                                case "escort_accept_request":
                                    await client.SendMessageAsync("Coagulations, our escort has chosen to  accept your request they will be in touch with you in a few.");
                                    await UpdateOrCreateProfileEmbedAsync(await Utils.GetMessageAsync(profile), profile);

                                    break;
                                case "escort_decline_request":
                                    profile.requestLists.RemoveAt(profile.requestLists.IndexOf(profile.requestLists.Find(x => x.member.Id == client.Id)));
                                    escorts[escort.Id] = profile;
                                    await AssetDatabase.SaveAsync(guild, escorts);
                                    await client.SendMessageAsync("Sorry to inform you but the escort that you have request has chosen to decline your request. No further action will be taken.");
                                    await UpdateOrCreateProfileEmbedAsync(await Utils.GetMessageAsync(profile), profile);
                                   
                                    break;
                            }
                            await msg.DeleteAsync();
                        }
                        else
                        {
                            await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder
                            {
                                Content = "Sorry it seam like you have already tried to make a request to one of our escorts please wait till one of them to replay or come back in a hour and try again."
                            });
                        }
                        break;
                    case "escort_like":
                        profile = escorts[e.Message.MentionedUsers.First().Id];
                        profile.likes++;
                        await AssetDatabase.SaveAsync(guild, escorts);
                        await UpdateOrCreateProfileEmbedAsync(await Utils.GetMessageAsync(profile), profile);
                        break;
                    case "escort_dislike":
                        profile = escorts[e.Message.MentionedUsers.First().Id];
                        profile.dislikes--;
                        await AssetDatabase.SaveAsync(guild, escorts);
                        await UpdateOrCreateProfileEmbedAsync(await Utils.GetMessageAsync(profile), profile);
                        break;
                    case "escort_tip":
                        var m = await Utils.GetMember(guild, e.Interaction.User.Id);
                        await m.SendMessageAsync(profile.tipLink);
                        break;
                    case "escort_edit":
                        var headEscortRole = Convert.ToUInt64(settings["escortManagementRole"]);
                        m = await Utils.GetMember(guild, e.User.Id);
                        var r = await Utils.GetRole(guild, headEscortRole);
                        if (profile.memberID == m.Id || m.Roles.Contains(r))
                            await RequestEditAsync(e, profile);
                        break;
                    case "escort_edit_submit":
                        profile =  escorts[e.Message.MentionedUsers.First().Id];
                        
                        
                        profile.vrcName = e.Message.Embeds.First().Fields[0].Value;
                        profile.gender = Enum.Parse<GType>(e.Message.Embeds.First().Fields[1].Value, true);
                        profile.serviceType = Enum.Parse<SType>(e.Message.Embeds.First().Fields[2].Value, true);
                        profile.equipmentType = Enum.Parse<EType>(e.Message.Embeds.First().Fields[3].Value, true);
                        profile.roleType = Enum.Parse<RType>(e.Message.Embeds.First().Fields[4].Value, true);
                        profile.bodyTypes = Enum.Parse<GType>(e.Message.Embeds.First().Fields[5].Value, true);
                        profile.timesAvailable = e.Message.Embeds.First().Fields[6].Value;
                        profile.kinksAndFetishes = e.Message.Embeds.First().Fields[7].Value;
                        profile.doAndDonts = e.Message.Embeds.First().Fields[8].Value;
                        profile.ReasonForJoining = e.Message.Embeds.First().Fields[9].Value;
                        profile.AboutMe = e.Message.Embeds.First().Fields[10].Value;
                        profile.tipLink = e.Message.Embeds.First().Fields[12].Value;
                        if (e.Message.Embeds.First().Image != null)
                            profile.AvatarUrl = e.Message.Embeds.First().Image.Url.ToString();
                        
                        escorts[e.Message.MentionedUsers.First().Id] = profile;
                        await AssetDatabase.SaveAsync(guild, escorts);
                        await UpdateOrCreateProfileEmbedAsync(await Utils.GetMessageAsync(profile), profile);
                        await e.Message.DeleteAsync("Profile has been submitted");
                        await e.Channel.SendMessageAsync("Profile has been updated");
                        break;

                    // Edit functions 
                    case "escort_edit_field_1": await UpdateProfileAsync(e, FieldEdit.VRChatName); break;
                    case "escort_edit_field_2": await UpdateProfileAsync(e, FieldEdit.Gender); break;
                    case "escort_edit_field_3": await UpdateProfileAsync(e, FieldEdit.ServiceType); break;
                    case "escort_edit_field_4": await UpdateProfileAsync(e, FieldEdit.EquipmentType); break;
                    case "escort_edit_field_5": await UpdateProfileAsync(e, FieldEdit.RoleType); break;
                    case "escort_edit_field_6": await UpdateProfileAsync(e, FieldEdit.PreferedBodyTypes); break;
                    case "escort_edit_field_7": await UpdateProfileAsync(e, FieldEdit.AvailableTimes); break;
                    case "escort_edit_field_8": await UpdateProfileAsync(e, FieldEdit.KinksFetishes); break;
                    case "escort_edit_field_9": await UpdateProfileAsync(e, FieldEdit.DoDonts); break;
                    case "escort_edit_field_10": await UpdateProfileAsync(e, FieldEdit.JoinReson); break;
                    case "escort_edit_field_11": await UpdateProfileAsync(e, FieldEdit.AbountMe); break;
                    case "escort_edit_field_12": await UpdateProfileAsync(e, FieldEdit.TipLink); break;
                    case "escort_edit_field_13": await UpdateProfileAsync(e, FieldEdit.ProfileImage); break;
                }
            }
            catch(Exception ex)
            {
                Utils.ReportException(ex);
                return;
            }
        }

      

        private static async Task UpdateProfileAsync(ComponentInteractionCreateEventArgs e, FieldEdit field)
        {
            var guild = GetInteractionGuild(e);
            var escots = await AssetDatabase.LoadAsync<Escorts>(guild);
            var member = await Utils.GetMember(guild, e.Message.MentionedUsers.First().Id);
            var profile = escots[member.Id];


            switch (field)
            {
                case FieldEdit.VRChatName:
                    var fm = await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { Content = "Please enter your answer below" });
                    var nm = await e.Channel.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                    if (nm.TimedOut || nm.Result.Content.Equals("exit", StringComparison.OrdinalIgnoreCase)) { await fm.DeleteAsync(); break; }
                    profile.vrcName = nm.Result.Content;
                    await fm.DeleteAsync();
                    break;
                case FieldEdit.Gender:
                    // Create the options for the user to pick
                    var options = Utils.CreateSelectOptionsFromEnum(profile.gender, Enum.GetNames<GType>());
                    // Make the dropdown
                    var dropdown = new DiscordSelectComponent("genderdropdown", null, options, false, 1, 1);
                    var builder = new DiscordMessageBuilder()
                        .WithContent("Please selct your gender")
                        .AddComponents(dropdown);
                    var m = await builder.SendAsync(e.Channel); // Replace with any method of getting a channel. //
                    var r = await m.WaitForSelectAsync("genderdropdown");
                    profile.gender = Enum.Parse<GType>(r.Result.Interaction.Data.Values[0].Split('_').Last(), true);
                    await m.DeleteAsync();
                    break;
                case FieldEdit.ServiceType:
                    // Create the options for the user to pick
                    options = Utils.CreateSelectOptionsFromEnum(profile.serviceType, Enum.GetNames<SType>());
                    // Make the dropdown
                    dropdown = new DiscordSelectComponent("servicedropdown", null, options, false, 1, 1);
                    builder = new DiscordMessageBuilder()
                        .WithContent("Please selct the service that you would like to provide")
                        .AddComponents(dropdown);
                    m = await builder.SendAsync(e.Channel); // Replace with any method of getting a channel. //
                    r = await m.WaitForSelectAsync("servicedropdown");
                    profile.serviceType = Enum.Parse<SType>(r.Result.Interaction.Data.Values[0].Split('_').Last(), true);
                    await m.DeleteAsync();
                    break;
                case FieldEdit.EquipmentType:
                    // Create the options for the user to pick
                    options = Utils.CreateSelectOptionsFromEnum(profile.equipmentType, Enum.GetNames<EType>());                   
                    // Make the dropdown
                    dropdown = new DiscordSelectComponent("equipmentdropdown", null, options, false, 1, 1);
                    builder = new DiscordMessageBuilder()
                        .WithContent("Please selct your current setup")
                        .AddComponents(dropdown);
                    m = await builder.SendAsync(e.Channel); // Replace with any method of getting a channel. //
                    r = await m.WaitForSelectAsync("equipmentdropdown");
                    profile.equipmentType = Enum.Parse<EType>(r.Result.Interaction.Data.Values[0].Split('_').Last(), true);
                    await m.DeleteAsync();
                    break;
                case FieldEdit.RoleType:
                    // Create the options for the user to pick
                    options = Utils.CreateSelectOptionsFromEnum(profile.roleType, Enum.GetNames<RType>());
                    // Make the dropdown
                    dropdown = new DiscordSelectComponent("roledropdown", null, options, false, 1, 1);
                    builder = new DiscordMessageBuilder()
                        .WithContent("Please selct the your prefered role")
                        .AddComponents(dropdown);
                    m = await builder.SendAsync(e.Channel); // Replace with any method of getting a channel. //
                    r = await m.WaitForSelectAsync("roledropdown");
                    profile.roleType = Enum.Parse<RType>(r.Result.Interaction.Data.Values[0].Split('_').Last(), true);
                    await m.DeleteAsync();
                    break;
                case FieldEdit.PreferedBodyTypes:
                    // Create the options for the user to pick
                    options = Utils.CreateSelectOptionsFromEnum(profile.bodyTypes, Enum.GetNames<GType>());
                    // Make the dropdown
                    dropdown = new DiscordSelectComponent("bodydropdown", null, options, false, 1, 1);
                    builder = new DiscordMessageBuilder()
                        .WithContent("Please selct prefered body type")
                        .AddComponents(dropdown);
                    m = await builder.SendAsync(e.Channel); // Replace with any method of getting a channel. //
                    r = await m.WaitForSelectAsync("bodydropdown");
                    profile.bodyTypes = Enum.Parse<GType>(r.Result.Interaction.Data.Values[0].Split('_').Last(), true);
                    await m.DeleteAsync();
                    break;
                case FieldEdit.AvailableTimes:
                    fm = await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { Content = "What times are you available?" });
                    nm = await e.Channel.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                    if (nm.TimedOut || nm.Result.Content.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
                    profile.timesAvailable = nm.Result.Content;
                    await fm.DeleteAsync();
                    break;
                case FieldEdit.KinksFetishes:
                    fm = await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { Content = "What are some of your Kink's and Fetishes?" });
                    nm = await e.Channel.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                    if (nm.TimedOut || nm.Result.Content.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
                    profile.kinksAndFetishes = nm.Result.Content;
                    await fm.DeleteAsync();
                    break;
                case FieldEdit.DoDonts:
                    fm = await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { Content = "What are some of your Do's and Dont's?" });
                    nm = await e.Channel.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                    if (nm.TimedOut || nm.Result.Content.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
                    profile.doAndDonts = nm.Result.Content;
                    await fm.DeleteAsync();
                    break;
                case FieldEdit.JoinReson:
                    fm = await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { Content = "Why did you decied to be come and escort?" });
                    nm = await e.Channel.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                    if (nm.TimedOut || nm.Result.Content.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
                    profile.ReasonForJoining = nm.Result.Content;
                    await fm.DeleteAsync();
                    break;
                case FieldEdit.AbountMe:
                    fm = await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { Content = "Tell us a little bit about you." });
                    nm = await e.Channel.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                    if (nm.TimedOut || nm.Result.Content.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
                    profile.AboutMe = nm.Result.Content;
                    await fm.DeleteAsync();
                    break;
                case FieldEdit.TipLink:
                    fm = await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { Content = "Please enter a link to receive tips." });
                    nm = await e.Channel.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                    if (nm.TimedOut || nm.Result.Content.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
                    profile.tipLink = nm.Result.Content;
                    await fm.DeleteAsync();
                    break;
                case FieldEdit.ProfileImage:
                    fm = await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { Content = "Please enter a link or drop and image you would like to set as your profile image." });
                    nm = await e.Channel.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                    if (nm.TimedOut || nm.Result.Content.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
                    profile.AvatarUrl = !string.IsNullOrEmpty(nm.Result.Content) ?  nm.Result.Content.Equals("unset", StringComparison.OrdinalIgnoreCase) ? nm.Result.Author.AvatarUrl : nm.Result.Content : nm.Result.Attachments.Count >= 1 ? nm.Result.Attachments[0].Url : nm.Result.Author.AvatarUrl;
                    await fm.DeleteAsync();
                    break;
            }
            escots[e.Message.MentionedUsers.First().Id] = profile;
            await AssetDatabase.SaveAsync(guild, escots);
            await e.Message.DeleteAsync();
            var msg = await member.SendMessageAsync(await ProfileToEmbed(guild, profile));

            await Task.CompletedTask;
        }
        

        private static async Task RequestEditAsync(ComponentInteractionCreateEventArgs e, Profile? profile)
        {
            var member = await Utils.GetMember(e.Guild, e.Message.MentionedUsers.First().Id);
            var msg = await member.SendMessageAsync(await ProfileToEmbed(e.Guild, profile));
        }

        internal static async Task ResumeSessions(DiscordGuild guild)
        {
            var escorts = await AssetDatabase.LoadAsync<Escorts>(guild);
            foreach (var profile in escorts.library)
            {
                Task.Factory.StartNew(async () =>
                {
                    while (escorts[profile.Key].requestLists.Count > 0)
                    {
                        foreach (var request in profile.Value.requestLists)
                        {
                            var ts = DateTime.Now - escorts[profile.Key].requestLists.Find(x => x.member.Id == request.member.Id).requestSubmitTime;
                            if (ts.TotalHours >= 1) escorts[profile.Key].requestLists.RemoveAt(escorts[profile.Key].requestLists.IndexOf(request));
                        }
                        await AssetDatabase.SaveAsync(guild, escorts);
                    }

                }).GetAwaiter();
            }
        }
    }
}