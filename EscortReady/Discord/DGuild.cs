using DSharpPlus.Entities;

namespace EscortsReady
{
    public class DGuild
    {
        public int MemberCount { get; set; } = 0;
        public ulong Id { get; set; }
        public string Name { get; set; } = "Demo Server";
        public GuildRank rank { get; set; } = GuildRank.Premium;
        public string image { get; set; }
        public List<DUser> UserEntries = new List<DUser>();


        public DiscordGuild? source
        {
            get
            {
                var guilds = DiscordService.Client.Guilds.Select(x => x.Value).ToList();
                var guild = guilds.Find(x => x.Id == Id);
                if (guild != null) return guild;
                else return null;
            }
        }

        public string GetRank()
        {
            switch (rank)
            {
                case GuildRank.Rank1:
                    return "https://cdn.discordapp.com/attachments/946818290003623989/1033941561970524160/LogoYellow.png";
                case GuildRank.Rank2:
                    return "https://cdn.discordapp.com/attachments/946818290003623989/1033941561546907658/LogoRed.png";
                case GuildRank.Rank3:
                    return "https://cdn.discordapp.com/attachments/946818290003623989/1033941560821305384/LogoGreen.png";
                case GuildRank.Rank4:
                    return "https://cdn.discordapp.com/attachments/946818290003623989/1033941560510918726/LogoBlue.png";
                case GuildRank.Premium:
                    return "https://cdn.discordapp.com/attachments/946818290003623989/1033941561177804871/LogoPurple.png";
            }
            return string.Empty;
        }

        public DGuild()
        {

        }
        public DGuild(DiscordGuild discordGuild)
        {
            MemberCount = discordGuild.MemberCount;
            Id = discordGuild.Id;
            Name = discordGuild.Name;
            image = discordGuild.IconUrl;
        }

        public static implicit operator DGuild(DiscordGuild discordGuild) => new DGuild(discordGuild);
    }
}