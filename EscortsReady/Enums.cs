namespace EscortsReady
{
    [Flags]
    public enum GType
    {
        Unset = 0,
        Male = 1 << 0,
        Female = 1 << 1,
        Other = 1 << 2,
    }
    [Flags]
    public enum SType
    {
        Unset = 0,
        DiscordOnly = 1 << 0,
        VRCOnly = 1 << 1,
        EventsOnly = 1 << 2,
        HalfService = 1 << 3,
        FullService = 1 << 4,
    }
    [Flags]
    public enum EType
    {
        Unset = 0,
        Desktop = 1 << 0,
        Quest = 1 << 1,
        PC = 1 << 2,
        Halfbody = 1 << 3,
        Fullbody = 1 << 4,
    }
    [Flags]
    public enum RType
    {
        Unset = 0,
        Submissive = 1 << 0,
        Dominant = 1 << 1,
        Switch = 1 << 2,
    }
    [Flags]
    public enum Rank
    {
        Unranked = 0,
        rank1 = 1 ,
        rank2 = 2,
        rank3 = 3,
        rank4 = 4,
        rank5 = 5,
    }

    [Flags]
    public enum EAM
    {
        None = 0,
        Head = 1 << 0,
        General = 1 << 1,
        All = Head | General,
    }
    public enum StorageFileType { Settings, Image, Video, Document, Misc }

    public enum GuildRank
    {
        Rank1,
        Rank2,
        Rank3,
        Rank4,
        Premium,
    }
}