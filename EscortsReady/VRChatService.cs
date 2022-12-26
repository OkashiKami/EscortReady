// See https://aka.ms/new-console-template for more information

using DSharpPlus.Entities;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Text;
using VRChat.API.Client;
using VRChat.API.Model;

namespace EscortsReady
{
    public class VRChatService
    {
        public static bool IsLoggedIn;
        public static CurrentUser _currentUser;

        //public const string worldid = "wrld_b096eca5-e552-40c7-8db5-7ad74cc8100e";
        public static async Task<string?> worldid()
        {
            var id = string.Empty;
            if (string.IsNullOrEmpty(Program.Configuration.GetValue<string>("VRChat:World")))
                id = "wrld_9cc731f8-2651-41a8-a69f-330da4dfed67";
            id = Program.Configuration.GetValue<string>("VRChat:World");
            return id;
        }
        public static async Task<string?> instanceID()
        {
            var id = string.Empty;
            if (string.IsNullOrEmpty(Program.Configuration.GetValue<string>("VRChat:Instance")))
                id = "EscortsReady";
            id = Program.Configuration.GetValue<string>("VRChat:Instance");
            return id;
        }

        private static IVRChat vrchatapi;
        private static DateTime timeSinceLstRequest = DateTime.Now;

        public static bool UsingVRC { get; set; } = true;

        public static async Task<bool> LoginAsync()
        {
            if (!UsingVRC) return false;
            if (vrchatapi != null && IsLoggedIn) return true;
            Configuration config = new Configuration();
            config.Username = Program.Configuration.GetValue<string>("VRChat:Username");
            config.Password = Program.Configuration.GetValue<string>("VRChat:Password");
            config.AccessToken = Program.Configuration.GetValue<string>("VRChat:Token");
            if (string.IsNullOrEmpty(config.Username) || string.IsNullOrEmpty(config.Password))
            {
                Program.logger.LogInformation("The VRChat _vrcUsername and or _vrcPassword is incorrect, please check your settings file to ensure that the login is correct");
                IsLoggedIn = false;
                return false;
            }
            // Setup VRChat Client
            vrchatapi = new VRChatClientBuilder(config).Build();
            try
            {
                var cu = await vrchatapi.Authentication.GetCurrentUserWithHttpInfoAsync();
                if (cu != null)
                {
                    try
                    {
                        var authResponse = await vrchatapi.Authentication.VerifyAuthTokenAsync();
                        var authToken = authResponse.Token;
                        Program.logger.LogInformation($"AuthToken: {authToken}");
                    }
                    catch (Exception ex)
                    {
                        Utils.ReportException(ex);
                        Program.logger.LogError($"ERROR: Unable to fetch auth token. Make sure to successfully login before starting to stream.\n{ex}");
                        IsLoggedIn = false;
                        return false;
                    }

                    IsLoggedIn = true;
                    _currentUser = JsonConvert.DeserializeObject<CurrentUser>(cu.RawContent, Utils.serializerSettings);
                    Program.logger.LogInformation("VRC Ok!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Utils.ReportException(ex);
                Program.logger.LogError($"VRC Error!\n{ex}");
                IsLoggedIn = false;
                return false;
            }
            return false;
        }

        /// <summary>
        /// Sends a event invite to the specified user
        /// </summary>
        /// <param name="ownerid">the id of the owner </param>
        /// <param name="user">thie user that will be reciveing the invite</param>
        /// <param name="onSuccess">run this function when the action has succeeed</param>
        public static async Task SendEventInviteAsync(DiscordGuild guild, ulong? ownerid, User? user, Action? onSuccess = null)
        {
            if (!await LoginAsync()) return;
            var _member = Member.LoadAsync(guild, x => x.vrcuserid == user.Id);
            try
            {
                var duser = await Member.LoadAsync(guild, x => x.id == ownerid);
                Program.logger.LogInformation(duser.vrcdisplayname);

                var instanceid = $"{await worldid()}:{await instanceID()}~private({duser.id})~nonce({Guid.NewGuid()})";
                Program.logger.LogInformation($"Instance Data: {instanceid}");
                var curUser = await vrchatapi.Authentication.GetCurrentUserAsync();
                var inviteRequest = new InviteRequest(instanceid);
                var result = await vrchatapi.Invites.InviteUserAsync(user.Id, inviteRequest);
                Program.logger.LogInformation($"{result}");
                if (onSuccess != null)
                    onSuccess?.Invoke();
            }
            catch (ApiException ex)
            {
                Utils.ReportException(ex);
                Program.logger.LogError($"{ex}");
            }
        }
        public static async Task<User?> SendFriendRequestAsync(DiscordMember discordMember, Action? onSuccess = null)
        {
            if (!await LoginAsync()) return default;
            var member = await Member.LoadAsync(discordMember.Guild, x => x.id == discordMember.Id);
            try
            {
                var user = await GetUserByIdAsync(member.vrcuserid);
                Task.Run(async () =>
                {
                    // Send Friend Request User
                    if (user != null && !user.IsFriend)
                    {
                        var elaps = TimeSpan.FromSeconds(0);
                        while (elaps.TotalSeconds < 120)
                            elaps = DateTime.Now - timeSinceLstRequest;
                        timeSinceLstRequest = DateTime.Now;

                        Notification result = await vrchatapi.Friends.FriendAsync(user.Id);
                        Program.logger.LogInformation($"{result}");
                        onSuccess?.Invoke();
                    }
                }).GetAwaiter();
                return user;
            }
            catch (ApiException ex)
            {
                Utils.ReportException(ex);
                Program.logger.LogError($"{ex}");
                return default;
            }

        }

        public static async Task<User?> SendUnFriendRequestAsync(DiscordMember discordMember, Action? onSuccess = null)
        {
            if (!await LoginAsync()) return default;
            var _member = await Member.LoadAsync(discordMember.Guild, x => x.id == discordMember.Id);
            var user = await GetUserByIdAsync(_member.vrcuserid);
            try
            {
                // Invite User
                if (user.IsFriend)
                {
                    var result = await vrchatapi.Friends.UnfriendAsync(user.Id);
                    Program.logger.LogInformation($"{result}");
                    onSuccess?.Invoke();
                    return await Task.FromResult(user);
                }
                else
                {
                    Program.logger.LogError($"{discordMember.Username}#{discordMember.Discriminator}'s vrchat account is already friends with {_currentUser.Username}");
                }

                return user;
            }
            catch (ApiException ex)
            {
                Utils.ReportException(ex);
                Program.logger.LogError($"{ex}");
                return default;
            }
        }

        public static async Task<User?> GetUserByIdAsync(string? userid, Action? onSuccess = null)
        {
            if (!await LoginAsync()) return default;
            try
            {
                User OtherUser = await vrchatapi.Users.GetUserAsync(userid);
                if (OtherUser == null) return default;
                var user = Task.FromResult(OtherUser).Result;
                onSuccess?.Invoke();
                return user;
            }
            catch (ApiException ex)
            {
                Utils.ReportException(ex);
                Program.logger.LogInformation($"{ex}");
                return default;
            }
        }
        public static async Task<User?> GetUserByUsernameAsync(string? username, Action? onSuccess = null)
        {
            if (!await LoginAsync()) return default;
            try
            {
                var OtherUser = await vrchatapi.Users.GetUserByNameAsync(username);
                if (OtherUser == null) return default;
                onSuccess?.Invoke();
                return OtherUser;
            }
            catch (ApiException ex)
            {
                Utils.ReportException(ex);
                Program.logger.LogError($"{ex}");
                return default;
            }
        }
        public static async Task<User?> GetUserByDisplaynameAsync(string? displayname, Action? onSuccess = null)
        {
            if (!await LoginAsync()) return default;
            try
            {
                var users = await vrchatapi.Users.SearchUsersAsync(displayname, n: 1);
                if (users.Count <= 0) return default;
                var user = await GetUserByUsernameAsync(users[0].Username);
                onSuccess?.Invoke();
                return Task.FromResult(user).Result;
            }
            catch (ApiException ex)
            {
                Utils.ReportException(ex);
                Program.logger.LogError($"{ex}");
                return default;
            }
        }
        public static async Task<User?> GetUserByProfileURLAsync(string? url, Action? onSuccess = null)
        {
            if (!await LoginAsync()) return default;
            var ignore = "https://vrchat.com/home/user/";
            if (string.IsNullOrEmpty(url)) return default;
            url = url.Replace(ignore, string.Empty);

            // check if the id is a valid guid
            if (!Guid.TryParse(url.Replace("usr_", string.Empty), out Guid res))
            {
                return default;
            }

            try
            {
                var user = await vrchatapi.Users.GetUserAsync(url);
                onSuccess?.Invoke();
                return Task.FromResult(user).Result;
            }
            catch (ApiException ex)
            {
                Utils.ReportException(ex);
                Program.logger.LogError($"{ex}");
                return default;
            }
        }
        public static async Task<World?> GetWorldByIdAsync(string worldid, Action? onSuccess = null)
        {
            if (!await LoginAsync()) return default;
            try
            {
                Program.logger.LogInformation("Getting World Info");
                World World = await vrchatapi.Worlds.GetWorldAsync(worldid);
                if (World == null) return default;
                Program.logger.LogInformation($"Found world {World.Name}, visits: {World.Visits}");
                onSuccess?.Invoke();
                return Task.FromResult(World).Result;
            }
            catch (ApiException ex)
            {
                Utils.ReportException(ex);
                Program.logger.LogError($"{ex}");
                return default;
            }
        }
        public static async Task SendEventInviteToAll(Action? onSuccess = null)
        {
            if (!await LoginAsync()) return;
            var friends = await vrchatapi.Friends.GetFriendsAsync();
            foreach (var friend in friends)
            {
                try
                {
                    // Invite User
                    var curUser = await vrchatapi.Authentication.GetCurrentUserAsync();
                    var inviteRequest = new InviteRequest($"{await worldid()}:{await instanceID()}~hidden({curUser.Id})~region(use)~nonce({Guid.NewGuid()})");
                    var result = await vrchatapi.Invites.InviteUserAsync(friend.Id, inviteRequest);
                    Program.logger.LogInformation($"{result}");
                    onSuccess?.Invoke();
                }
                catch (ApiException ex)
                {
                    Utils.ReportException(ex);
                    Program.logger.LogError($"{ex}");
                }
            }
        }
        public static async Task PushData(string avatarid, params string[] infos)
        {
            if (!await LoginAsync()) return;
            var avi = await GetAvatarAsync(avatarid);
            if (infos.Length > 1)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < infos.Length; i++)
                {

                    sb.Append(infos[i]);
                    if (i < infos.Length - 1) sb.Append(",");
                }
                await CreateImageUploadAsync(avi, sb.ToString());
            }
            else await CreateImageUploadAsync(avi, infos[0]);
            await Task.CompletedTask;
        }
        public static Task CheckNotificationAsync()
        {
            Task.Run(async () =>
            {
                if (!await LoginAsync()) return;
                //Debug.LogAsync("Checking notification...");
                var notifications = await vrchatapi.Notifications.GetNotificationsAsync();
                if (notifications == null) return;
                foreach (var notification in notifications)
                {
                    switch (notification.Type)
                    {
                        case NotificationType.FriendRequest: await vrchatapi.Notifications.AcceptFriendRequestAsync(notification.Id); return;
                        case NotificationType.RequestInvite:
                            break;
                        case NotificationType.Invite:
                            var instance = await vrchatapi.Instances.GetInstanceAsync("wrld_b096eca5-e552-40c7-8db5-7ad74cc8100e", "Events");
                            notification.Validate(new ValidationContext(instance));
                            notification.Seen = true;
                            break;
                        default:
                            Program.logger.LogInformation(notification.ToJson());
                            break;
                    }
                }
            });
            return Task.CompletedTask;
        }
        public static async Task<Avatar> GetAvatarAsync(string id)
        {
            if (!await LoginAsync()) return default;
            var avijson = await vrchatapi.Avatars.GetAvatarWithHttpInfoAsync(id);
            var avi = JsonConvert.DeserializeObject<Avatar>(avijson.RawContent);
            return avi;
        }
        private static async Task CreateImageUploadAsync(Avatar avi, string json)
        {
            if (!await LoginAsync()) return;
            var map = await Utils.EncodeImageAsync(json);
            if (map != null && avi != null)
            {
                var fileId = default(string);
                var match = Regex.Match(avi.imageUrl, "file_[0-9A-Za-z-]+");
                if (!match.Success) { Program.logger.LogInformation($"Cannot upload image: Failed to extract fileId"); }
                else fileId = match.Value;
                var bytes = await System.IO.File.ReadAllBytesAsync("image.png");
                var md5 = Utils.CalculateMD5(bytes);
                decimal signatureSize = 2048;
                decimal fileSize = bytes.Length;
                VRChat.API.Model.File fv;
                var curfile = await vrchatapi.Files.GetFileWithHttpInfoAsync(fileId);
                var _curfile = curfile.Data.Versions.FindAll(x => x.Status == FileStatus.Waiting);
                if (_curfile.Count > 0) fv = curfile.Data;
                else
                {
                    var fvr = new CreateFileVersionRequest(md5, signatureSize, md5, fileSize);
                    var _fv = await vrchatapi.Files.CreateFileVersionWithHttpInfoAsync(fileId, fvr);
                    fv = _fv.Data;
                }
                var fvn = fv.Versions.Last()._Version;
                var fileuploadurl = await vrchatapi.Files.StartFileDataUploadAsync(fileId, fvn, "file", 1);
                var bac = new ByteArrayContent(bytes);
                bac.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                bac.Headers.Add("Content-MD5", md5);
                bac.Headers.ContentLength = bytes.Length;
                var _req = new HttpRequestMessage(System.Net.Http.HttpMethod.Put, fileuploadurl.Url);
                _req.Content = bac;
                _req.Headers.Add("User-Agent", vrchatapi.Files.Configuration.UserAgent);
                using (var httpc = new HttpClient())
                {
                    var _res = await httpc.SendAsync(_req);
                    if (_res.IsSuccessStatusCode)
                    {
                        //Console.ForegroundColor = ConsoleColor.Green;
                        //Console.Write("Ok!");
                        //Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        var str = await _res.Content.ReadAsStringAsync();
                        Program.logger.LogInformation($"{_res.StatusCode} [{_res.ReasonPhrase}]\n{str}\n");
                    }
                }
                await vrchatapi.Files.FinishFileDataUploadAsync(fileId, fvn, "file");
                await vrchatapi.Files.FinishFileDataUploadAsync(fileId, fvn, "signature");
                await vrchatapi.Files.FinishFileDataUploadAsync(fileId, fvn, "delta");
                var imageurl = $"{vrchatapi.Files.Configuration.BasePath}/file/{fileId}/{fvn}/file";
                var request = new UpdateAvatarRequest(imageUrl: imageurl, version: avi.version);
                var adata = await vrchatapi.Avatars.UpdateAvatarAsync(avi.id, request);
            }
            await Task.CompletedTask;
        }
    }
}