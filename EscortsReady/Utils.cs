﻿using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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
        public static string PrettyName(string value) => Regex.Replace(value, @"[^0-9a-zA-Z\._]", "");
        public static string CalculateMD5(byte[] imgBytes)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(imgBytes);
            return Convert.ToBase64String(hash);
        }
        public static string GetDomain(string item)
        {
            if (item.ToLower().Contains("discord.gg")) return "Discord";
            if (item.ToLower().Contains("gumroad.com")) return "Gumroad";
            if (item.ToLower().Contains("twitter.com")) return "Twitter";
            if (item.ToLower().Contains("youtube.com")) return "Youtube";
            if (item.ToLower().Contains("udonvr.com")) return "UdonVR";
            return new Uri(item).Host;
        }
        public static async Task<string> GetLocation(string value)
        {
            var split = value.Split('(', '~');
            value = split[0];

            if (value == string.Empty) return "N/A";
            if (value == "offline") return value;
            if (split.ToList().Contains("hidden")) return "In a Private World";
            var _world = await VRChatService.GetWorldByIdAsync(value);
            if (_world != null)
                return $"**{_world.Name}** {_world.AuthorName}";
            return "In a Private World";
        }
        public static async Task<Bitmap> EncodeImageAsync(string textToEncode, string fileName = "image.png")
        {
            //Debug.LogAsync(textToEncode, header: false);
            var image = await EncodeTextAsync(textToEncode);

            image.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);

            return image;
        }
        private static async Task<Bitmap> EncodeTextAsync(string input)
        {
            var textBytes_list = Encoding.Unicode.GetBytes(input);
            textBytes_list = new byte[] { 255, 254 }.Concat(textBytes_list).ToArray();
            var totalBytes = new byte[] { 0, 0, (byte)textBytes_list.Length };
            //totalBytes = totalBytes.Reverse().ToArray();

            //textBytes_list = new byte[] { 255, 254 }.Concat(textBytes_list).ToArray();

            byte b = 16;
            if (textBytes_list.Length % 3 != 0)
                textBytes_list = textBytes_list.Append(b).ToArray();

            if (textBytes_list.Length % 3 != 0)
                textBytes_list = textBytes_list.Append(b).ToArray();

            //for (int index = 0; index < textBytes_list.Length; index++)
            //{
            //    Console.WriteLine($"{index} : {(char)textBytes_list[index]}");
            //}

            //Console.WriteLine($"A{textBytes_list.Length}");
            //Console.WriteLine($"B{(totalBytes[0], totalBytes[1], totalBytes[2])}");

            var image_width = 128;
            var image_height = 96;

            var img = DrawFilledRectangle(image_width, image_height);

            //var image1 = new Bitmap(@"imgp.png", true);
            //Console.WriteLine("Pixel format P : " + image1.PixelFormat.ToString());
            //Console.WriteLine("Pixel format C#: " + img.PixelFormat.ToString());
            img.SetPixel(img.Width - 1, 0, Color.FromArgb(totalBytes[0], totalBytes[1], totalBytes[2]));

            foreach (var x in Enumerable.Range(1, (textBytes_list.Length / 3) + 1 - 1))
            {
                //Console.WriteLine($"(({img.Width}w - 1) - {x}x) % {img.Width}w = {((img.Width - 1) - x) % img.Width}");
                //Console.WriteLine($"{Mod(((img.Width - 1) - x), img.Width) }");
                int cx = (int)Mod(((img.Width - 1) - x), img.Width);
                //Console.WriteLine(cx);
                img.SetPixel(cx, x / img.Width, Color.FromArgb(textBytes_list[(x - 1) * 3], textBytes_list[((x - 1) * 3) + 1], textBytes_list[((x - 1) * 3) + 2]));
            }
            //Debug.LogAsync("Done!", header: false);
            await Task.CompletedTask;
            return img;
        }
        static float Mod(float a, float b)
        {
            return a - (b * MathF.Floor(a / b));
        }
        static private Bitmap DrawFilledRectangle(int x, int y)
        {
            Bitmap bmp = new Bitmap(x, y, PixelFormat.Format24bppRgb);
            using (Graphics graph = Graphics.FromImage(bmp))
            {
                Rectangle ImageSize = new Rectangle(0, 0, x, y);
                graph.FillRectangle(Brushes.White, ImageSize);
            }
            return bmp;
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