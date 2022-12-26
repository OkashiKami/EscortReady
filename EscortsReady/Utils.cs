using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Text;
using System.Threading.Channels;

namespace EscortsReady
{
    internal class Utils
    {
        public static JsonSerializerSettings serializerSettings
        {
            get
            {
                var options = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    CheckAdditionalContent = true,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.All,
                };
                return options;
            }
        }

        public static string Unset  => "unset";

        internal static async Task<bool> CheckPerms(InteractionContext ctx, EAM allow = EAM.None)
        {
            ulong devid = 284913031572488194;
            if (ctx.Member.Id == devid) return true;
            var settings = await Settings.LoadAsync(ctx.Guild);
            if (allow.HasFlag(EAM.Head))
                if (ctx.Member.Roles.Contains(settings["escortManagementRole"])) return true;
            if (allow.HasFlag(EAM.General))
                if (ctx.Member.Roles.Contains(settings["escortDefaultRole"])) return true;
            return false;
        }

        public static async Task<string> CreateEscortEmblem(Rank rank, bool isHead = false)
        {
            var rootdir = $"{new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName}\\Resources\\Exports";
            Directory.CreateDirectory(rootdir);
            using (var map = new Bitmap(1024, 1024))
            {
                Image escortIcon;
                try
                {
                    escortIcon = Image.FromFile(isHead ? $"{rootdir}\\EscortsHead.png" : $"{rootdir}\\Escorts.png");
                }
                catch (Exception ex)
                {
                    ReportException(ex);
                    return default;
                }

                Image rankIcon;
                try
                {
                    rankIcon = Image.FromFile($"{rootdir}\\Heart.png");
                }
                catch (Exception ex)
                {

                    ReportException(ex);
                    return default;
                }

                using (var bitmap = new Bitmap(escortIcon.Width, escortIcon.Height))
                {
                    using (var canvas = Graphics.FromImage(bitmap))
                    {
                        var rankIconSize = 32;
                        canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        canvas.DrawImage(escortIcon, new Rectangle(0, 0, escortIcon.Width, escortIcon.Height));
                        for (int i = 0; i < (int)rank; i++)
                        {
                            var offset = (escortIcon.Width / 2) - ((rankIconSize / 2) * (int)rank);
                            canvas.DrawImage(rankIcon,  new Rectangle(offset + (i * rankIconSize), escortIcon.Height - (rankIconSize * 5), rankIconSize, rankIconSize));
                        }


                        canvas.Save();
                    }
                    try
                    {
                        if (File.Exists($"{rootdir}\\tmpImage.png"))
                            File.Delete($"{rootdir}\\tmpImage.png");
                        bitmap.Save($"{rootdir}\\tmpImage.png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                    catch (Exception ex) { ReportException(ex); }
                }

            }
            var imageurl = await PostImageToTmpServer($"{rootdir}\\tmpImage.png");
            return imageurl;
        }

        private static async Task<string> PostImageToTmpServer(string originalFile)
        {
            ulong tid = 946818290003623986;
            var tmpServer = await DiscordService.Client.GetGuildAsync(tid);
            var tmpChannel = tmpServer.Channels.First(x => x.Value.Type == DSharpPlus.ChannelType.Text).Value;
            DiscordMessage msg;
            using(var fs = new FileStream(originalFile, FileMode.Open, FileAccess.Read))
            {
                msg = await tmpChannel.SendMessageAsync("** **");
                await msg.ModifyAsync(x =>
                {
                    x.WithFile(fs, true);
                });
            }
            msg = await tmpChannel.GetMessageAsync(msg.Id, true);
            var attachment = msg.Attachments.First();
            return attachment.Url;
        }

        internal static async void ReportException(Exception ex, InteractionContext? ctx = null)
        {
            ulong devid = 284913031572488194;
            if (ctx != null)
            {
                var dwb = new DiscordWebhookBuilder();
                dwb.WithContent($"Operation failed, a message has been sent to the crator and it should be fixed, please try again in a few minutes ❌");
                await ctx.EditResponseAsync(dwb);
            }

            var creator = await DiscordService.Client.GetUserAsync(devid);
            DiscordService.Client.Guilds.ToList().ForEach(async x => await x.Value.Members.ToList().Find(x => x.Key == devid).Value.SendMessageAsync($"[EscortsReady]\n{ex}"));
        }

        public static Rank CalculateRangking(Profile profile)
        {
            // Calculate the total number of likes and dislikes
            int total = profile.likes + profile.dislikes;

            // Return the percentage of likes
            var percentage = (int)((float)profile.likes / total * 100);
            // Assign a corresponding number of stars based on the percentage
            if (percentage >= 90) return Rank.rank5;
            else if (percentage >= 70) return Rank.rank4;
            else if (percentage >= 50) return Rank.rank3;
            else if (percentage >= 30) return Rank.rank2;
            else if (percentage >= 10) return Rank.rank1;
            else return Rank.Unranked;
        }
        public static async Task<DiscordMember> GetMemberAsync(Profile profile)
        {
            var server = await DiscordService.Client.GetGuildAsync(profile.serverID);
            var member = server.Members[profile.memberID];
            return member;
        }
        public static async Task<DiscordMember> GetMemberAsync(DiscordGuild server, ulong memberID)
        {
            var member = server.Members[memberID];
            return member;
        }
        public static async Task<DiscordChannel> GetChannelAsync(Profile profile)
        {
            var server = await DiscordService.Client.GetGuildAsync(profile.serverID);
            var channel = server.Channels[profile.channelID];
            return channel;
        }
        public static async Task<DiscordChannel> GetChannelAsync(DiscordGuild server, ulong channelID)
        {
            var channel = server.Channels[channelID];
            return channel;
        }
        public static async Task<DiscordRole> GetRoleAsynce(DiscordGuild server, ulong roleID)
        {
            var role = server.Roles[roleID];
            return role;
        }
        public static async Task<DiscordMessage> GetMessageAsync(Profile profile)
        {
            var server = await DiscordService.Client.GetGuildAsync(profile.serverID);
            var channel = server.Channels[profile.channelID];
            DiscordMessage message = null;
            try { message = await channel.GetMessageAsync(profile.messageID); } catch { }
            return message;
        }

        public static async Task CheckAndRemoveInvalidProfilesAsync(DiscordGuild guild)
        {
            var settings = await Settings.LoadAsync(guild);
            var profileContainer = await EscortProfile.LoadAsync(guild);
            var headEscortRole = Convert.ToUInt64(settings["escortManagementRole"]);
            var normalEscortRole = Convert.ToUInt64(settings["escortDefaultRole"]);
            var _her = await GetRoleAsynce(guild, headEscortRole);
            var _ger = await GetRoleAsynce(guild, normalEscortRole);


            foreach (var key in profileContainer.library.Keys)
            {
                var item = profileContainer[key];
            
                //check if profile member is in the server;
                var member = await GetMemberAsync(guild, key);
                if (member == null)
                {
                    profileContainer.Remove(key);
                    continue;
                }
                // check if member still hase the role 
                var mr = member.Roles.ToList();
                if (!mr.Contains(_her) && !mr.Contains(_ger))
                    profileContainer.Remove(key);
            }
            await EscortProfile.SaveAsync(guild, profileContainer);
        }

        public static string? MKToken()
        {
            var sb = new StringBuilder();
            sb.Append($"{Program.Configuration.GetValue<string>("DTOKEN1")}.");
            sb.Append($"{Program.Configuration.GetValue<string>("DTOKEN2")}.");
            sb.Append($"{Program.Configuration.GetValue<string>("DTOKEN3")}");
            return sb.ToString();
        }
    }
}