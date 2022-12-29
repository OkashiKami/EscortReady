// See https://aka.ms/new-console-template for more information

using DSharpPlus.Entities;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Text;
using EscortsReady.Utilities;
using EscortsReady;

namespace EscortsReady
{
    public class Member : FileData
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

                var members = await AssetDatabase.LoadAsync<Members>(guild);
                members.Clear();

                var tasklist = new List<Task>();
                guildmembers.ForEach(gm => tasklist.Add(Task.Factory.StartNew(async () =>
                {
                    var member = members.Find(x => x != null && x.id == gm.Id);
                    if (member == null)
                    {
                        member = gm;
                        member.roles = gm.Roles.ToList();
                    }
                })));
                Task.WaitAll(tasklist.ToArray());
                await AssetDatabase.SaveAsync(guild, members);

            }
            catch (Exception ex)
            {
                Utils.ReportException(ex);
                Program.logger.LogError($"{ex}");
            }
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
            var profile = await AssetDatabase.LoadAsync<Members>(guild);
            profile.Remove(member.id);
            await AssetDatabase.SaveAsync(guild, profile);
        }
    }
}