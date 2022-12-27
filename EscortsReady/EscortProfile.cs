using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Text;

namespace EscortsReady
{
    internal class EscortProfile : FileData
    {
        public Dictionary<ulong, Profile> library = new Dictionary<ulong, Profile>();
        public static async Task<EscortProfile> LoadAsync(DiscordGuild guild)
        {
            var file = $"{rootdir}\\{guild.Name}_{guild.Id}\\Profiles.json";
            Directory.CreateDirectory(new FileInfo(file).Directory.FullName);
            EscortProfile settingsObject = null;
            if (!File.Exists(file))
            {
                settingsObject = new EscortProfile();
                return settingsObject;
            }
            using (var fs = File.OpenRead(file))
            {
                using (var sr = new StreamReader(fs, Encoding.Unicode))
                {
                    try
                    {
                        var json = await sr.ReadToEndAsync();
                        settingsObject = JsonConvert.DeserializeObject<EscortProfile>(json, Utils.serializerSettings);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        settingsObject = new EscortProfile();
                    }
                }
            }
            return settingsObject;
        }
        public static async Task SaveAsync(DiscordGuild guild, EscortProfile profileContainer)
        {
            var file = $"{rootdir}\\{guild.Name}_{guild.Id}\\Profiles.json";
            Directory.CreateDirectory(new FileInfo(file).Directory.FullName);
            using (var fs = File.Open(file, FileMode.Create))
            {
                using (var sw = new StreamWriter(fs, Encoding.Unicode))
                {
                    var json = JsonConvert.SerializeObject(profileContainer, Utils.serializerSettings);
                    await sw.WriteAsync(json);
                }
            }
        }
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
        public bool HasKey(ulong key) => library.ContainsKey(key);
        public static async Task<Profile> UpdateOrCreateProfileEmbedAsync(DiscordMessage msg, Profile profile)
        {
            var settings = await Settings.LoadAsync(msg.Channel.Guild);
            var headEscortRole = Convert.ToUInt64(settings["escortManagementRole"]);
            var normalEscortRole = Convert.ToUInt64(settings["escortDefaultRole"]);
            //Create embed for the message 
            var texture = await Utils.CreateEscortEmblem(Utils.CalculateRangking(profile), profile.isHead);
            var embed = new DiscordEmbedBuilder();
            var m = await Utils.GetMemberAsync(profile);
            var mr = await Utils.GetRoleAsynce(msg.Channel.Guild, profile.isHead ? headEscortRole : normalEscortRole);
            embed.WithAuthor($"{m.DisplayName} ({mr.Name})", $"https://{msg.Channel.Guild.Id}.com", $"https://cdn.discordapp.com/attachments/946818290003623989/1056710179271495680/EscortsReady.png");
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
            embed.WithFooter("ERS", $"https://cdn.discordapp.com/attachments/946818290003623989/1056710179271495680/EscortsReady.png");
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
            var settings = await Settings.LoadAsync(guild);
            var headEscortRole = Convert.ToUInt64(settings["escortManagementRole"]);
            var normalEscortRole = Convert.ToUInt64(settings["escortDefaultRole"]);
            var mb = new DiscordMessageBuilder();
            
            //Create embed for the message 
            var texture = await Utils.CreateEscortEmblem(Utils.CalculateRangking(profile), profile.isHead);
            var embed = new DiscordEmbedBuilder();
            var m = await Utils.GetMemberAsync(profile);
            var mr = await Utils.GetRoleAsynce(guild, profile.isHead ? headEscortRole : normalEscortRole);
            embed.WithAuthor($"{m.DisplayName} ({mr.Name})", $"https://{guild.Id}.com", $"https://cdn.discordapp.com/attachments/946818290003623989/1056710179271495680/EscortsReady.png");
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

            embed.WithFooter("ERS (type exit to cancel)", $"https://cdn.discordapp.com/attachments/946818290003623989/1056710179271495680/EscortsReady.png");
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
        public Profile? this[ulong key]
        {
            get
            {
                var hasKey = library.ContainsKey(key);
                if (hasKey)
                    return library[key];
                else return null;
            }
            set
            {
                var hasKey = library.ContainsKey(key);
                if (!hasKey && value != null)
                    library.Add(key, value);
                else if (hasKey && value != null)
                    library[key] = value;
            }
        }
        public void Remove(ulong key)
        {
            if (library.ContainsKey(key))
                library.Remove(key);
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
                var settings = await Settings.LoadAsync(guild);
                var member = await Utils.GetMemberAsync(guild, e.Message.MentionedUsers.First().Id);
                var profiles = await LoadAsync(guild);
                var profile = profiles[member.Id];

                switch (e.Id)
                {
                    case "escort_request": break;
                    case "escort_like": break;
                    case "escort_dislike": break;
                    case "escort_tip":
                        var m = await Utils.GetMemberAsync(guild, e.Interaction.User.Id);
                        await m.SendMessageAsync(profile.tipLink);
                        break;
                    case "escort_edit":
                        var headEscortRole = Convert.ToUInt64(settings["escortManagementRole"]);
                        m = await Utils.GetMemberAsync(guild, e.User.Id);
                        var r = await Utils.GetRoleAsynce(guild, headEscortRole);
                        if (profile.memberID == m.Id || m.Roles.Contains(r))
                            await RequestEditAsync(e, profile);
                        break;
                    case "escort_edit_submit":
                        profile =  profiles[e.Message.MentionedUsers.First().Id];
                        
                        
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
                        profile.tipLink = e.Message.Embeds.First().Fields[11].Value;
                        if (e.Message.Embeds.First().Image != null)
                            profile.AvatarUrl = e.Message.Embeds.First().Image.Url.ToString();
                        
                        profiles[e.Message.MentionedUsers.First().Id] = profile;
                        await SaveAsync(guild, profiles);
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
            var profiles = await LoadAsync(guild);
            var member = await Utils.GetMemberAsync(guild, e.Message.MentionedUsers.First().Id);
            var profile = profiles[member.Id];


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
                    var options = Utils.CreateSelectOptionsFromEnum(profile.serviceType, Enum.GetNames<GType>());
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
            profiles[e.Message.MentionedUsers.First().Id] = profile;
            await SaveAsync(guild, profiles);
            await e.Message.DeleteAsync();
            var msg = await member.SendMessageAsync(await ProfileToEmbed(guild, profile));

            await Task.CompletedTask;
        }
        

        private static async Task RequestEditAsync(ComponentInteractionCreateEventArgs e, Profile? profile)
        {
            var member = await Utils.GetMemberAsync(e.Guild, e.Message.MentionedUsers.First().Id);
            var msg = await member.SendMessageAsync(await ProfileToEmbed(e.Guild, profile));
        }
    }
}