// See https://aka.ms/new-console-template for more information

using DSharpPlus.Entities;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Text;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace PermissionEx
{
    public class Member
    {
        public static string GetDirectory(DiscordGuild guild)
        {
            var m = @$"Resources\\{(guild != null ? $"{Utils.PrettyName(guild.Name)}\\" : string.Empty)}Members";
            Directory.CreateDirectory(m);
            return m;
        }
        [JsonIgnore]
        public string GetFileName
        {
            get
            {
                var un = username;
                un = Regex.Replace(un, @"[^0-9a-zA-Z\._]", "");

                var source = $"{un}#{discriminator}_{id}.json";
                var bytes = Encoding.UTF8.GetBytes(source);
                return Encoding.UTF8.GetString(bytes);
            }
        }



        public ulong guildid;
        public ulong id;
        public string? username;
        public string? discriminator;
        public string? displayname;
        public string? email;
        public DateTimeOffset joindate;
        public List<DiscordRole> roles = new List<DiscordRole>();
        public string? vrcuserid;
        public string? vrcdisplayname;


        public async Task UpdateAsync(Member member)
        {
            id = member.id;
            username = member.username;
            discriminator = member.discriminator;
            displayname = member.displayname;
            email = member.email;
            joindate = member.joindate;
            roles = member.roles;
            await Task.CompletedTask;
        }

        public static implicit operator Member(DiscordMember discordMember)
        {
            return new Member()
            {
                id = discordMember.Id,
                username = discordMember.Username,
                discriminator = discordMember.Discriminator,
                displayname = discordMember?.DisplayName,
                email = discordMember?.Email,
                joindate = discordMember.JoinedAt,
                roles = discordMember.Roles.ToList()
            };
        }


        public static async Task ReCreate(DiscordGuild guild)
        {
            try
            {
                var guildmembers = (await guild.GetAllMembersAsync()).ToList();
                var filemembers = await CleanAllAsync(guild);

                if (filemembers == null) filemembers = new List<Member>();
                var tasklist = new List<Task>();
                guildmembers.ForEach(gm => tasklist.Add(Task.Factory.StartNew(async () =>
                {
                    var member = filemembers != null && filemembers.Count > 0 ? filemembers.Find(x => x != null && x.id == gm.Id) : null;
                    if (member == null)
                    {
                        member = gm;
                        member.roles = gm.Roles.ToList();
                        await member.CloseAsync(guild);
                    }
                })));
                Task.WaitAll(tasklist.ToArray());
            }
            catch (Exception ex) { await LoggerEx.LogAsync(ex); }
        }

        public void AddRole(DiscordRole _role)
        {
            if (roles.Find(x => x.Id == _role.Id) == null)
                roles.Add(_role);
        }
        public void RemoveRole(DiscordRole _role)
        {
            var _r = roles.Find(x => x.Id == _role.Id);
            if (_r != null)
                roles.RemoveAt(roles.IndexOf(_r));
        }

        public static async Task DeleteAsync(DiscordGuild guild, Member member)
        {
            var profile = await LoadAsync(guild, x => x.id == member.id);
            if (profile != null)
            {
                var file = Path.Combine(GetDirectory(guild), profile.GetFileName);
                if (File.Exists(file))
                    File.Delete(file);
            }
        }

        public static async Task<Member> LoadAsync(DiscordGuild guild,  Predicate<Member>? predicate = null)
        {
            var watch = Stopwatch.StartNew();
            var files = Directory.GetFiles(GetDirectory(guild), "*.json", SearchOption.AllDirectories);
            var members = files.Select(x => JsonConvert.DeserializeObject<Member>(File.ReadAllText(x), Utils.JsonSettings)).ToList();
            var member = predicate != null ? members.Find(predicate) : members.First();
            watch.Stop();
            await LoggerEx.LogAsync($"It took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).Milliseconds}ms to complete {nameof(LoadAsync)}");
            return member;
        }
        public static async Task<List<Member>> LoadAllAsync(DiscordGuild guild, Predicate<Member>? predicate = null)
        {
            var watch = Stopwatch.StartNew();
            var files = Directory.GetFiles(GetDirectory(guild), "*.json", SearchOption.AllDirectories);
            var members = files.Select(x => JsonConvert.DeserializeObject<Member>(File.ReadAllText(x), Utils.JsonSettings)).ToList();
            members = predicate != null ? members.FindAll(predicate) : members;
            watch.Stop();
            await LoggerEx.LogAsync($"It took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).Milliseconds}ms to complete {nameof(LoadAllAsync)}");
            return members;
        }
        public static async Task<List<Member>> CleanAllAsync(DiscordGuild guild, Predicate<Member>? predicate = null)
        {
            var watch = Stopwatch.StartNew();
            var gms = await guild.GetAllMembersAsync();
            var files = Directory.GetFiles(GetDirectory(guild), "*.json", SearchOption.AllDirectories);
            files.ToList().ForEach(async x =>
            {
                try
                {

                    var m = JsonConvert.DeserializeObject<Member>(File.ReadAllText(x), Utils.JsonSettings);
                    if (m != null && gms.ToList().Find(x => x.Id == m.id) == null)
                        File.Delete(x);
                }
                catch (Exception ex)
                {
                    await LoggerEx.LogAsync(ex);
                }
            });
            files = Directory.GetFiles(GetDirectory(guild), "*.json", SearchOption.AllDirectories);
            var members = files.Select(x => JsonConvert.DeserializeObject<Member>(File.ReadAllText(x), Utils.JsonSettings)).ToList();
            members = predicate != null ? members.FindAll(predicate) : members;
            watch.Stop();
            await LoggerEx.LogAsync($"It took {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).Milliseconds}ms to complete {nameof(LoadAllAsync)}");
            return members;
        }
        public static async Task SaveAsync(DiscordGuild guild, Member profile, string? directoryOverride = null)
        {
            try
            {
                var save = Path.Combine(!string.IsNullOrEmpty(directoryOverride) ? directoryOverride : GetDirectory(guild), profile.GetFileName);

                using (var fs = File.Open(save, FileMode.Create))
                {
                    using (var sw = new StreamWriter(fs, Encoding.Unicode))
                    {
                        var json = JsonConvert.SerializeObject(profile, Utils.JsonSettings);
                        await sw.WriteAsync(json);
                    }
                }
            }
            catch (Exception ex)
            {
                await LoggerEx.LogAsync(ex);
            }
        }


        public async Task CloseAsync(DiscordGuild guild)
        {
            await SaveAsync(guild, this);
            GC.SuppressFinalize(this);
        }
    }

    public class MemberStructor
    {

    }
}