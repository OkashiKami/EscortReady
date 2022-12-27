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
            embed.AddField("**9** Do’s/Donts (These should be made very clear for members to understand)", profile.doAndDonts);
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
                new DiscordButtonComponent(ButtonStyle.Secondary, "escort_edit_field_1", "Submit", false, new DiscordComponentEmoji("✅")) 
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

        public static async Task HandelInteractionAsync(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {
            try
            {
                var msgurl = e.Message.Embeds.First().Author.Url.ToString();
                var guidid = msgurl.Replace("https://", string.Empty).Replace(".com/", string.Empty);
                

                var guildid = ulong.Parse(guidid);
                var guild = e.Guild != null ? e.Guild : DiscordService.Client.Guilds[guildid];


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
                        profile.AvatarUrl = e.Message.Embeds.First().Image.Url.ToString();
                        profile.tipLink = e.Message.Embeds.First().Fields[12].Value;
                        
                        profiles[e.Message.MentionedUsers.First().Id] = profile;
                        await SaveAsync(guild, profiles);
                        await UpdateOrCreateProfileEmbedAsync(await Utils.GetMessageAsync(profile), profile);
                        break;
                   
                }
            }
            catch(Exception ex)
            {
                Utils.ReportException(ex);
                return;
            }
        }

        private static async Task RequestEditAsync(ComponentInteractionCreateEventArgs e, Profile? profile)
        {
            var member = await Utils.GetMemberAsync(e.Guild, e.Message.MentionedUsers.First().Id);
            var msg = await member.SendMessageAsync(await ProfileToEmbed(e.Guild, profile));            
            var ch = msg.Channel;
            reask:

            var msg2 = await ch.SendMessageAsync("Please enter the number of the field that you would like to edit. or click one of the buttons above");
            var res = await ch.GetNextMessageAsync(TimeSpan.FromMinutes(5));
            if (!res.TimedOut)
            {
                if (res.Result.Content.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) goto end;
                int.TryParse(res.Result.Content, out int index);
                switch (index)
                {
                    case 1:
                        await ch.SendMessageAsync("Please enter your vrchat name or enter **exit** to cancel");
                        res = await ch.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                        if(!res.TimedOut)
                        {
                            if (res.Result.Content.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) goto end;
                            profile.vrcName = res.Result.Content;
                        }
                        break;
                    case 2:
                        await ch.SendMessageAsync("Please enter your gender or enter **exit** to cancel");
                        res = await ch.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                        if (!res.TimedOut)
                        {
                            if (res.Result.Content.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) goto end;
                            if (Enum.TryParse(res.Result.Content, true, out GType value))
                                profile.gender = value;
                            else
                                profile.gender = GType.Unset;
                        }
                        break;
                    case 3:
                        await ch.SendMessageAsync("Please enter your service type or enter **exit** to cancel");
                        res = await ch.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                        if (!res.TimedOut)
                        {
                            if (res.Result.Content.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) goto end;
                            if (Enum.TryParse(res.Result.Content, true, out SType value))
                                profile.serviceType = value;
                            else
                                profile.serviceType = SType.Unset;
                        }
                        break;
                    case 4:
                        await ch.SendMessageAsync("Please enter your equipment type or enter **exit** to cancel");
                        res = await ch.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                        if (!res.TimedOut)
                        {
                            if (res.Result.Content.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) goto end;
                            if (Enum.TryParse(res.Result.Content, true, out EType value))
                                profile.equipmentType = value;
                            else
                                profile.equipmentType = EType.Unset;
                        }
                        break;
                    case 5:
                        await ch.SendMessageAsync("Please enter your role type or enter **exit** to cancel");
                        res = await ch.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                        if (!res.TimedOut)
                        {
                            if (res.Result.Content.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) goto end;
                            if (Enum.TryParse(res.Result.Content, true, out RType value))
                                profile.roleType = value;
                            else
                                profile.roleType = RType.Unset;
                        }
                        break;
                    case 6:
                        await ch.SendMessageAsync("Please enter your body type prefered or enter **exit** to cancel");
                        res = await ch.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                        if (!res.TimedOut)
                        {
                            if (res.Result.Content.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) goto end;
                            if (Enum.TryParse(res.Result.Content, true, out GType value))
                                profile.bodyTypes = value;
                            else
                                profile.bodyTypes = GType.Unset;
                        }
                        break;
                    case 7:
                        await ch.SendMessageAsync("Please enter your time available or enter **exit** to cancel");
                        res = await ch.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                        if (!res.TimedOut)
                        {
                            if (res.Result.Content.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) goto end;
                            profile.timesAvailable = res.Result.Content;
                        }
                        break;
                    case 8:
                        await ch.SendMessageAsync("Please enter your kink's & Fetishes or enter **exit** to cancel");
                        res = await ch.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                        if (!res.TimedOut)
                        {
                            if (res.Result.Content.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) goto end;
                            profile.kinksAndFetishes = res.Result.Content;
                        }
                        break;
                    case 9:
                        await ch.SendMessageAsync("Please enter your Do's & Dont's or enter **exit** to cancel");
                        res = await ch.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                        if (!res.TimedOut)
                        {
                            if (res.Result.Content.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) goto end;
                            profile.doAndDonts = res.Result.Content;
                        }
                        break;
                    case 10:
                        await ch.SendMessageAsync("Please enter your reason for joining or enter **exit** to cancel");
                        res = await ch.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                        if (!res.TimedOut)
                        {
                            if (res.Result.Content.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) goto end;
                            profile.ReasonForJoining = res.Result.Content;
                        }
                        break;
                    case 11:
                        await ch.SendMessageAsync("Please enter your abount me or enter **exit** to cancel");
                        res = await ch.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                        if (!res.TimedOut)
                        {
                            if (res.Result.Content.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) goto end;
                            profile.AboutMe = res.Result.Content;
                        }
                        break;
                    case 12:
                        await ch.SendMessageAsync("Please enter your profile image url or enter **exit** to cancel");
                        res = await ch.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                        if (!res.TimedOut)
                        {
                            if (res.Result.Content.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) goto end;
                            profile.AvatarUrl = res.Result.Content;
                        }
                        break;
                    case 13:
                        await ch.SendMessageAsync("Please enter your link to receive tips or enter **exit** to cancel");
                        res = await ch.GetNextMessageAsync(TimeSpan.FromMinutes(5));
                        if (!res.TimedOut)
                        {
                            if (res.Result.Content.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) goto end;
                            profile.tipLink = res.Result.Content;
                        }
                        break;

                }
                await msg.DeleteAsync();
                msg = await member.SendMessageAsync(await ProfileToEmbed(e.Guild, profile));
            }
            goto reask;
            end:
            await ch.SendMessageAsync("Operation has ended");

        }
    }
}